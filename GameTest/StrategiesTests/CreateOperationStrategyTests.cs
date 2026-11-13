using App;
using App.Scopes;
using Moq;

namespace SpaceBattle.Lib.Test;

public class CreateOperationStrategyTests
{
    [Fact]
    public void SuccessfulCreateOperationStrategyRun()
    {
        new InitCommand().Execute();
        var scope = Ioc.Resolve<object>("IoC.Scope.Create");
        Ioc.Resolve<App.ICommand>("IoC.Scope.Current.Set", scope).Execute();

        var objMock = new Mock<IUObject>();
        var subCommandMock = new Mock<ICommand>();
        var macroCommandMock = new Mock<ICommand>();

        new RegisterDependencyCommand("Game.Rules.Get.MoveWithFuel", (object[] args) => new List<string> { "CheckFuel", "Move" }).Execute();
        new RegisterDependencyCommand("Game.Command.Create.CheckFuel", (object[] args) => subCommandMock.Object).Execute();
        new RegisterDependencyCommand("Game.Command.Create.Move", (object[] args) => subCommandMock.Object).Execute();
        new RegisterDependencyCommand("Game.Command.Macro.Create", (object[] args) => macroCommandMock.Object).Execute();

        var strategy = new CreateOperationStrategy();
        var result = strategy.RunStrategy(objMock.Object, "MoveWithFuel");

        Assert.Equal(macroCommandMock.Object, result);
    }
}

