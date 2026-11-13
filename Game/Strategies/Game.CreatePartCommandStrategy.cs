using App;

namespace SpaceBattle.Lib;

public class CreatePartCommandStrategy : IStrategy
{
    public object RunStrategy(params object[] args)
    {
        var name = (string)args[0];
        var obj = (IUObject)args[1];

        var list_string = Ioc.Resolve<IEnumerable<string>>("SetupOperation." + name);
        var list_commands = list_string.Select(str => Ioc.Resolve<ICommand>(str, obj));

        var macro = Ioc.Resolve<ICommand>("CreateMacroCommand", list_commands);

        return macro;
    }
}
