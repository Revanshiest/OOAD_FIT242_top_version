using App;

namespace SpaceBattle.Lib;

public class StartCommand : ICommand
{
    private IStartable obj;
    public StartCommand(IStartable obj)
    {
        this.obj = obj;
    }

    public void Execute()
    {
        obj.Properties.ToList().ForEach(o => Ioc.Resolve<ICommand>("Game.Commands.SetProperty", obj.Target, o.Key, o.Value).Execute());

        ICommand cmd = Ioc.Resolve<ICommand>("Game.Operations.Moving", obj.Target);

        Ioc.Resolve<ICommand>("Game.Commands.SetProperty", obj.Target, "Moving", cmd).Execute();

        Ioc.Resolve<ICommand>("Game.Queue.Push", Ioc.Resolve<Queue<ICommand>>("Game.Queue"), cmd).Execute();
    }
}
