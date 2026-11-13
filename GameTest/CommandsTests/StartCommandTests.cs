using App;
using App.Scopes;

using Moq;

namespace SpaceBattle.Lib.Test;

public class StartMoveCommandTests
{
    public StartMoveCommandTests()
    {
        new InitCommand().Execute();
        var scope = Ioc.Resolve<object>("IoC.Scope.Create");
        Ioc.Resolve<App.ICommand>("IoC.Scope.Current.Set", scope).Execute();

        var mockCommand = new Mock<SpaceBattle.Lib.ICommand>();
        mockCommand.Setup(x => x.Execute());

        var mockStrategyReturnsCommand = new Mock<IStrategy>();
        mockStrategyReturnsCommand.Setup(x => x.RunStrategy(It.IsAny<object[]>())).Returns(mockCommand.Object);

        var mockStrategyReturnsQueue = new Mock<IStrategy>();
        mockStrategyReturnsQueue.Setup(x => x.RunStrategy()).Returns(new Queue<SpaceBattle.Lib.ICommand>());

        new RegisterDependencyCommand("Game.Commands.SetProperty", (object[] args) => mockStrategyReturnsCommand.Object.RunStrategy(args)).Execute();
        new RegisterDependencyCommand("Game.Operations.Moving", (object[] args) => mockStrategyReturnsCommand.Object.RunStrategy(args)).Execute();
        new RegisterDependencyCommand("Game.Queue.Push", (object[] args) => mockStrategyReturnsCommand.Object.RunStrategy(args)).Execute();
        new RegisterDependencyCommand("Game.Queue", (object[] args) => mockStrategyReturnsQueue.Object.RunStrategy()).Execute();
    }

    [Fact]
    public void SuccesfulStartCommandExecute()
    {
        var startable = new Mock<IStartable>();
        var obj = new Mock<IUObject>();

        startable.SetupGet(a => a.Target).Returns(obj.Object).Verifiable();
        startable.SetupGet(a => a.Properties).Returns(new Dictionary<string, object>() { { "Speed", new Vector(1, 1) } }).Verifiable();

        ICommand startMove = new StartCommand(startable.Object);

        startMove.Execute();

        startable.VerifyAll();
    }

    [Fact]
    public void TargetMethodReturnsExceptionInStartCommand()
    {
        var startable = new Mock<IStartable>();

        startable.SetupGet(a => a.Target).Throws<Exception>().Verifiable();
        startable.SetupGet(a => a.Properties).Returns(new Dictionary<string, object>() { { "Speed", new Vector(1, 1) } }).Verifiable();

        ICommand startMove = new StartCommand(startable.Object);

        Assert.Throws<Exception>(() => startMove.Execute());
    }

    [Fact]
    public void SpeedMethodReturnsExceptionInStartCommand()
    {
        var startable = new Mock<IStartable>();
        var obj = new Mock<IUObject>();

        startable.SetupGet(a => a.Target).Returns(obj.Object).Verifiable();
        startable.SetupGet(a => a.Properties).Throws<Exception>().Verifiable();

        ICommand startMove = new StartCommand(startable.Object);

        Assert.Throws<Exception>(() => startMove.Execute());
    }
}
