using App;
using App.Scopes;
using Moq;

namespace SpaceBattle.Lib.Test;


public class GameCommandTests
{
    [Fact]
    public void SuccessfulGameCommandExecute()
    {
        new InitCommand().Execute();
        var scope = Ioc.Resolve<object>("IoC.Scope.Create");
        Ioc.Resolve<App.ICommand>("IoC.Scope.Current.Set", scope).Execute();

        var queue = new Queue<ICommand>();

        var cmd = new Mock<ICommand>();
        cmd.Setup(c => c.Execute()).Callback(() => { }).Verifiable();

        var runCommandsStrategy = new Mock<IStrategy>();
        runCommandsStrategy.Setup(s => s.RunStrategy(It.IsAny<Queue<ICommand>>())).Returns(cmd.Object).Verifiable();

        new RegisterDependencyCommand("Game.RunCommands", (object[] args) => runCommandsStrategy.Object.RunStrategy(args)).Execute();

        var game = new GameCommand(queue, scope);

        var scopeNew = Ioc.Resolve<object>("IoC.Scope.Create");
        Ioc.Resolve<App.ICommand>("IoC.Scope.Current.Set", scopeNew).Execute();

        game.Execute();

        Assert.True(scope == Ioc.Resolve<object>("IoC.Scope.Current"));
        cmd.VerifyAll();
        runCommandsStrategy.VerifyAll();
    }
}
