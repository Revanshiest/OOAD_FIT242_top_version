using App;

namespace SpaceBattle.Lib;

public class AddHandlerToExceptionTreeCommand : ICommand
{
    private IEnumerable<Type> list;
    private IHandler handler;
    public AddHandlerToExceptionTreeCommand(IEnumerable<Type> list, IHandler handler)
    {
        this.list = list;
        this.handler = handler;
    }
    public void Execute()
    {
        var tree = Ioc.Resolve<IDictionary<int, IHandler>>("Game.GetExceptionTree");

        var key = Ioc.Resolve<int>("Game.GetHashCode", list);

        tree.TryAdd(key, handler);
    }
}
