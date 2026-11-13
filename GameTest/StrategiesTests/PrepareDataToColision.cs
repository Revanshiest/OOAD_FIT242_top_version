using App;
using App.Scopes;

using Moq;

namespace SpaceBattle.Lib.Test;

public class PrepareDataToColisionTests
{

    [Fact]
    public void SuccesfulPrepareDataToCollisionStrategyTest()
    {
        new InitCommand().Execute();
        var scope = Ioc.Resolve<object>("IoC.Scope.Create");
        Ioc.Resolve<App.ICommand>("IoC.Scope.Current.Set", scope).Execute();

        List<int> property1 = new List<int>() { 12, 32, 56, 4 };
        List<int> property2 = new List<int>() { 12, 32, 56, 4 };

        IStrategy PrepareData = new PrepareDataToCollisionStrategy();

        Object.Equals(new List<int>() { 0, 0, 0, 0 }, PrepareData.RunStrategy(property1, property2));
    }
}
