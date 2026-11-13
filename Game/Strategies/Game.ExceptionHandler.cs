using App;

namespace SpaceBattle.Lib;

public class FindExceptionHandlerStrategy : IStrategy
{
    public object RunStrategy(params object[] args)
    {
        var list = (IEnumerable<Type>)args[0];

        var tree = Ioc.Resolve<IDictionary<int, IHandler>>("Game.GetExceptionTree");

        return tree.TryGetValue(Ioc.Resolve<int>("Game.GetHashCode", list), out IHandler? value) ? value : tree[0];
    }
}
