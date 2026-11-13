using System.Reflection;
using System.Text;
using App;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace SpaceBattle.Lib;

public class AdapterBuilder
{
    private Type? _targetInterface;

    public string SourceCodeOverride { get; set; } = string.Empty;

    public AdapterBuilder SetInterface(Type targetInterface)
    {
        if (targetInterface == null)
        {
            throw new ArgumentNullException(nameof(targetInterface));
        }

        if (!targetInterface.IsInterface)
        {
            throw new ArgumentException($"Type '{targetInterface.FullName}' must be an interface.", nameof(targetInterface));
        }

        _targetInterface = targetInterface;
        return this;
    }

    public static string GetAdapterClassName(Type interfaceType)
    {
        if (interfaceType == null)
        {
            throw new ArgumentNullException(nameof(interfaceType));
        }

        string typeName;
        if (interfaceType.IsGenericType)
        {
            var defName = interfaceType.GetGenericTypeDefinition().FullName!;
            var cleanDef = defName.Substring(0, defName.IndexOf('`'));
            var args = string.Join("_", interfaceType.GetGenericArguments().Select(t => t.FullName!.Replace(".", "_")));
            typeName = $"{cleanDef}_{args}";
        }
        else
        {
            typeName = interfaceType.FullName!;
        }

        var sanitized = typeName.Replace(".", "_").Replace("+", "_");
        return $"{sanitized}Adapter";
    }

    private static string GetTypeName(Type type)
    {
        if (type == typeof(void))
        {
            return "void";
        }

        if (type.IsGenericType)
        {
            var genericTypeDefinition = type.GetGenericTypeDefinition().FullName!;
            var cleanName = genericTypeDefinition.Substring(0, genericTypeDefinition.IndexOf('`')).Replace('+', '.');
            var genericArgs = string.Join(", ", type.GetGenericArguments().Select(GetTypeName));
            return $"{cleanName}<{genericArgs}>";
        }

        return type.FullName!.Replace('+', '.');
    }

    public string BuildSource()
    {
        if (_targetInterface == null)
        {
            throw new InvalidOperationException("Target interface is not set.");
        }

        var className = GetAdapterClassName(_targetInterface);
        var interfaceFullName = GetTypeName(_targetInterface);

        var sb = new StringBuilder();
        sb.AppendLine("using System;");
        sb.AppendLine("using App;");
        sb.AppendLine("using SpaceBattle.Lib;");
        sb.AppendLine();
        sb.AppendLine("namespace SpaceBattle.Lib;");
        sb.AppendLine();
        sb.AppendLine($"public class {className} : {interfaceFullName}");
        sb.AppendLine("{");
        sb.AppendLine("    private readonly IUObject _item;");
        sb.AppendLine();
        sb.AppendLine($"    public {className}(IUObject item)");
        sb.AppendLine("    {");
        sb.AppendLine("        _item = item ?? throw new ArgumentNullException(nameof(item));");
        sb.AppendLine("    }");

        var properties = _targetInterface.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var prop in properties)
        {
            sb.AppendLine();
            var propTypeName = GetTypeName(prop.PropertyType);
            sb.AppendLine($"    public {propTypeName} {prop.Name}");
            sb.AppendLine("    {");

            if (prop.CanRead)
            {
                sb.AppendLine($"        get => Ioc.Resolve<{propTypeName}>(\"Properties.{prop.Name}\", _item);");
            }

            if (prop.CanWrite)
            {
                sb.AppendLine($"        set => Ioc.Resolve<ICommand>(\"Properties.{prop.Name}.Set\", _item, value).Execute();");
            }

            sb.AppendLine("    }");
        }

        var methods = _targetInterface.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => !m.IsSpecialName);

        foreach (var method in methods)
        {
            sb.AppendLine();
            var returnTypeName = GetTypeName(method.ReturnType);
            var parameters = method.GetParameters();
            var paramList = string.Join(", ", parameters.Select(p => $"{GetTypeName(p.ParameterType)} {p.Name}"));
            var argList = parameters.Length > 0
                ? ", " + string.Join(", ", parameters.Select(p => p.Name))
                : string.Empty;

            sb.AppendLine($"    public {returnTypeName} {method.Name}({paramList})");
            sb.AppendLine("    {");

            if (method.ReturnType == typeof(void))
            {
                sb.AppendLine($"        Ioc.Resolve<ICommand>(\"Properties.{method.Name}\", _item{argList}).Execute();");
            }
            else
            {
                sb.AppendLine($"        return Ioc.Resolve<{returnTypeName}>(\"Properties.{method.Name}\", _item{argList});");
            }

            sb.AppendLine("    }");
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    public Type Build()
    {
        if (_targetInterface == null)
        {
            throw new InvalidOperationException("Target interface is not set.");
        }

        var source = string.IsNullOrEmpty(SourceCodeOverride) ? BuildSource() : SourceCodeOverride;
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var assemblyName = $"DynamicAdapter_{Guid.NewGuid():N}";

        var referencedAssemblies = new HashSet<Assembly>
        {
            typeof(object).Assembly,
            typeof(Console).Assembly,
            typeof(IUObject).Assembly,
            typeof(Ioc).Assembly,
            _targetInterface.Assembly,
            Assembly.Load("System.Runtime")
        };

        foreach (var prop in _targetInterface.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            referencedAssemblies.Add(prop.PropertyType.Assembly);
        }

        foreach (var method in _targetInterface.GetMethods(BindingFlags.Public | BindingFlags.Instance))
        {
            referencedAssemblies.Add(method.ReturnType.Assembly);
            foreach (var p in method.GetParameters())
            {
                referencedAssemblies.Add(p.ParameterType.Assembly);
            }
        }

        var references = referencedAssemblies
            .Select(a => MetadataReference.CreateFromFile(a.Location))
            .Cast<MetadataReference>()
            .ToList();

        var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
        var compilation = CSharpCompilation.Create(assemblyName, new[] { syntaxTree }, references, compilationOptions);

        using var ms = new MemoryStream();
        var result = compilation.Emit(ms);

        if (!result.Success)
        {
            var failures = result.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);

            var errorMsg = new StringBuilder();
            errorMsg.AppendLine("Failed to compile dynamic adapter:");
            foreach (var diagnostic in failures)
            {
                errorMsg.AppendLine($"{diagnostic.Id}: {diagnostic.GetMessage()}");
            }
            errorMsg.AppendLine("Source code:");
            errorMsg.AppendLine(source);

            throw new InvalidOperationException(errorMsg.ToString());
        }

        ms.Seek(0, SeekOrigin.Begin);
        var assembly = Assembly.Load(ms.ToArray());
        var className = GetAdapterClassName(_targetInterface);
        var fullClassName = $"SpaceBattle.Lib.{className}";
        return assembly.GetType(fullClassName)!;
    }
}
