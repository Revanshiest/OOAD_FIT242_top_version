using App;

namespace SpaceBattle.Lib;


public class CreateOperationStrategy : IStrategy
{
    public object RunStrategy(params object[] args)
    {
        var obj = (IUObject)args[0];
        var orderType = (string)args[1];

        var ruleList = Ioc.Resolve<IEnumerable<string>>("Game.Rules.Get." + orderType);

        var commandList = ruleList.ToList().Select(rule => Ioc.Resolve<ICommand>("Game.Command.Create." + rule, obj));

        return Ioc.Resolve<ICommand>("Game.Command.Macro.Create", commandList);
    }
}
