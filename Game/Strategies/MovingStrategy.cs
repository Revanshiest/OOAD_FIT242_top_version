using App;

namespace SpaceBattle.Lib;

public class MovingStrategy : IStrategy
{
    public object RunStrategy(params object[] args)
    {
        IUObject obj = (IUObject)args[0];

        IEnumerable<ICommand> list_command = Ioc.Resolve<IEnumerable<ICommand>>("Game.CreateMove", obj);

        ICommand macro_сommand = Ioc.Resolve<ICommand>("Game.Command.Macro", list_command);

        ICommand inject_command = Ioc.Resolve<ICommand>("Game.Command.Inject", macro_сommand);

        ICommand repeat_сommand = Ioc.Resolve<ICommand>("Game.Command.Repeat", inject_command);
        list_command.Append(repeat_сommand);

        return inject_command;
    }
}
