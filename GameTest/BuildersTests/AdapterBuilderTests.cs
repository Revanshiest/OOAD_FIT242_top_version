using System.Reflection;
using Hwdtech;
using Hwdtech.Ioc;
using Moq;

namespace SpaceBattle.Lib.Test;

public interface ICustomTestObject
{
    Vector Position { get; set; }
    Vector Velocity { get; }
    void Fire(int power);
    int Calculate(int a, int b);
}

public interface IGenericContainer<T>
{
    T Value { get; set; }
}

public interface IInterfaceWithCompilationError
{
    // Будет использован в тесте на ошибку компиляции
}

public class AdapterBuilderTests
{
    public AdapterBuilderTests()
    {
        new InitScopeBasedIoCImplementationCommand().Execute();
        IoC.Resolve<Hwdtech.ICommand>(
            "Scopes.Current.Set",
            IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Root"))
        ).Execute();
    }

    [Fact]
    public void SetInterface_Null_ThrowsArgumentNullException()
    {
        var builder = new AdapterBuilder();
        Assert.Throws<ArgumentNullException>(() => builder.SetInterface(null!));
    }

    [Fact]
    public void SetInterface_NotInterface_ThrowsArgumentException()
    {
        var builder = new AdapterBuilder();
        Assert.Throws<ArgumentException>(() => builder.SetInterface(typeof(Vector)));
    }

    [Fact]
    public void BuildSource_WithoutSettingInterface_ThrowsInvalidOperationException()
    {
        var builder = new AdapterBuilder();
        Assert.Throws<InvalidOperationException>(() => builder.BuildSource());
    }

    [Fact]
    public void Build_WithoutSettingInterface_ThrowsInvalidOperationException()
    {
        var builder = new AdapterBuilder();
        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void BuildSource_GeneratesCorrectSourceCode()
    {
        var builder = new AdapterBuilder().SetInterface(typeof(IMovable));
        var source = builder.BuildSource();

        Assert.Contains("public class SpaceBattle_Lib_IMovableAdapter : SpaceBattle.Lib.IMovable", source);
        Assert.Contains("IoC.Resolve<SpaceBattle.Lib.Vector>(\"Properties.Speed\", _item)", source);
        Assert.Contains("IoC.Resolve<SpaceBattle.Lib.Vector>(\"Properties.Pos\", _item)", source);
        Assert.Contains("IoC.Resolve<ICommand>(\"Properties.Pos.Set\", _item, value).Execute()", source);
    }

    [Fact]
    public void Build_GeneratesFunctionalAdapter_ForIMovable()
    {
        var uObjectMock = new Mock<IUObject>();
        var expectedPos = new Vector(10, 20);
        var expectedSpeed = new Vector(1, 2);

        var setCommandMock = new Mock<ICommand>();

        IoC.Resolve<Hwdtech.ICommand>(
            "IoC.Register",
            "Properties.Pos",
            (object[] args) => expectedPos
        ).Execute();

        IoC.Resolve<Hwdtech.ICommand>(
            "IoC.Register",
            "Properties.Speed",
            (object[] args) => expectedSpeed
        ).Execute();

        Vector? setPosValue = null;
        IoC.Resolve<Hwdtech.ICommand>(
            "IoC.Register",
            "Properties.Pos.Set",
            (object[] args) =>
            {
                setPosValue = (Vector)args[1];
                return setCommandMock.Object;
            }
        ).Execute();

        var builder = new AdapterBuilder().SetInterface(typeof(IMovable));
        var adapterType = builder.Build();

        Assert.NotNull(adapterType);
        Assert.True(typeof(IMovable).IsAssignableFrom(adapterType));

        var adapter = (IMovable)Activator.CreateInstance(adapterType, uObjectMock.Object)!;

        Assert.Equal(expectedPos, adapter.Pos);
        Assert.Equal(expectedSpeed, adapter.Speed);

        var newPos = new Vector(30, 40);
        adapter.Pos = newPos;

        Assert.Equal(newPos, setPosValue);
        setCommandMock.Verify(c => c.Execute(), Times.Once);
    }

    [Fact]
    public void Build_GeneratesFunctionalAdapter_WithMethodsAndReturnTypes()
    {
        var uObjectMock = new Mock<IUObject>();
        var fireCommandMock = new Mock<ICommand>();

        int capturedPower = 0;
        IoC.Resolve<Hwdtech.ICommand>(
            "IoC.Register",
            "Properties.Fire",
            (object[] args) =>
            {
                capturedPower = (int)args[1];
                return fireCommandMock.Object;
            }
        ).Execute();

        IoC.Resolve<Hwdtech.ICommand>(
            "IoC.Register",
            "Properties.Calculate",
            (object[] args) => (object)((int)args[1] + (int)args[2])
        ).Execute();

        var builder = new AdapterBuilder().SetInterface(typeof(ICustomTestObject));
        var adapterType = builder.Build();

        var adapter = (ICustomTestObject)Activator.CreateInstance(adapterType, uObjectMock.Object)!;

        adapter.Fire(42);
        Assert.Equal(42, capturedPower);
        fireCommandMock.Verify(c => c.Execute(), Times.Once);

        var sum = adapter.Calculate(10, 15);
        Assert.Equal(25, sum);
    }

    [Fact]
    public void Adapter_Constructor_ThrowsWhenUObjectIsNull()
    {
        var builder = new AdapterBuilder().SetInterface(typeof(IMovable));
        var adapterType = builder.Build();

        var ex = Assert.Throws<TargetInvocationException>(() => Activator.CreateInstance(adapterType, (IUObject)null!));
        Assert.IsType<ArgumentNullException>(ex.InnerException);
    }

    [Fact]
    public void GetAdapterClassName_Null_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => AdapterBuilder.GetAdapterClassName(null!));
    }

    [Fact]
    public void Build_GenericInterface_CompilesSuccessfully()
    {
        var uObjectMock = new Mock<IUObject>();
        IoC.Resolve<Hwdtech.ICommand>(
            "IoC.Register",
            "Properties.Value",
            (object[] args) => "test_value"
        ).Execute();

        var builder = new AdapterBuilder().SetInterface(typeof(IGenericContainer<string>));
        var adapterType = builder.Build();

        Assert.NotNull(adapterType);
        var adapter = (IGenericContainer<string>)Activator.CreateInstance(adapterType, uObjectMock.Object)!;
        Assert.Equal("test_value", adapter.Value);
    }

    [Fact]
    public void Build_InvalidSourceCode_ThrowsInvalidOperationException()
    {
        var builder = new AdapterBuilder().SetInterface(typeof(IMovable));
        builder.SourceCodeOverride = "invalid syntax class { }";

        var ex = Assert.Throws<InvalidOperationException>(() => builder.Build());
        Assert.Contains("Failed to compile dynamic adapter", ex.Message);
    }
}
