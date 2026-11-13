using Hwdtech;
using Hwdtech.Ioc;
using Moq;

namespace SpaceBattle.Lib.Test;

public class CreateOperationStrategyTests
{
    [Fact]
    public void SuccessfulCreateOperationStrategyRun()
    {
        new InitScopeBasedIoCImplementationCommand().Execute();
        IoC.Resolve<Hwdtech.ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Root"))).Execute();

        var objMock = new Mock<IUObject>();
        var subCommandMock = new Mock<ICommand>();
        var macroCommandMock = new Mock<ICommand>();

        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Game.Rules.Get.MoveWithFuel", (object[] args) => new List<string> { "CheckFuel", "Move" }).Execute();
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Game.Command.Create.CheckFuel", (object[] args) => subCommandMock.Object).Execute();
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Game.Command.Create.Move", (object[] args) => subCommandMock.Object).Execute();
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Game.Command.Macro.Create", (object[] args) => macroCommandMock.Object).Execute();

        var strategy = new CreateOperationStrategy();
        var result = strategy.RunStrategy(objMock.Object, "MoveWithFuel");

        Assert.Equal(macroCommandMock.Object, result);
    }
}

