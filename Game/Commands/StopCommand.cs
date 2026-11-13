using App;

namespace SpaceBattle.Lib;

public class StopCommand : ICommand
{
    private IStopable obj;
    public StopCommand(IStopable obj)
    {
        this.obj = obj;
    }
    public void Execute()
    {
        obj.Properties.ToList().ForEach(o => Ioc.Resolve<ICommand>("Game.Commands.RemoveProperty", obj.Target, o).Execute());

        Ioc.Resolve<IInjectable>("Game.Commands.GetProperty", obj.Target, "Moving").Inject(Ioc.Resolve<ICommand>("Game.Commands.EmptyCommand"));
    }
}
