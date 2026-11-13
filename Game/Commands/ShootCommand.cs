using App;

namespace SpaceBattle.Lib;


public class ShootCommand : ICommand
{
    private IShootable obj;
    public ShootCommand(IShootable obj)
    {
        this.obj = obj;
    }

    public void Execute()
    {
        Ioc.Resolve<ICommand>("Game.Queue.Push", Ioc.Resolve<Queue<ICommand>>("Game.Queue.Get"),
            Ioc.Resolve<ICommand>("Game.IUObject.Shoot", obj)).Execute();
    }
}
