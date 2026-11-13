using Moq;
using App;
using App.Scopes;


namespace SpaceBattle.Lib.Test;


public class CreateCommandStrategyTests
{
    [Fact]
    public void SuccessfulCreateShootCommandInStrategy()
    {
        new InitCommand().Execute();
        var scope = Ioc.Resolve<object>("IoC.Scope.Create");
        Ioc.Resolve<App.ICommand>("IoC.Scope.Current.Set", scope).Execute();

        var shootableObj = new Mock<IShootable>();

        var adapterStrategy = new Mock<IStrategy>();
        adapterStrategy.Setup(s => s.RunStrategy(It.IsAny<object[]>())).Returns(shootableObj.Object);

        new RegisterDependencyCommand("Game.Adapter.Create", (object[] args) => adapterStrategy.Object.RunStrategy(args)).Execute();

        var strategy = new CreateShootCommandStrategy();

        var obj = new Mock<IUObject>();

        var result = strategy.RunStrategy(obj.Object);

        Assert.NotNull(result);
        Assert.IsType<ShootCommand>(result);
    }

    [Fact]
    public void SuccessfulCreateRotateCommandInStrategy()
    {
        new InitCommand().Execute();
        var scope = Ioc.Resolve<object>("IoC.Scope.Create");
        Ioc.Resolve<App.ICommand>("IoC.Scope.Current.Set", scope).Execute();

        var rotatableObj = new Mock<IRotatable>();

        var adapterStrategy = new Mock<IStrategy>();
        adapterStrategy.Setup(s => s.RunStrategy(It.IsAny<object[]>())).Returns(rotatableObj.Object);

        new RegisterDependencyCommand("Game.Adapter.Create", (object[] args) => adapterStrategy.Object.RunStrategy(args)).Execute();

        var strategy = new CreateRotateCommandStrategy();

        var obj = new Mock<IUObject>();

        var result = strategy.RunStrategy(obj.Object);

        Assert.NotNull(result);
        Assert.IsType<RotateCommand>(result);
    }

    [Fact]
    public void SuccessfulCreateStartMoveCommandInStrategy()
    {
        new InitCommand().Execute();
        var scope = Ioc.Resolve<object>("IoC.Scope.Create");
        Ioc.Resolve<App.ICommand>("IoC.Scope.Current.Set", scope).Execute();

        var startMoveObj = new Mock<IStartable>();

        var adapterStrategy = new Mock<IStrategy>();
        adapterStrategy.Setup(s => s.RunStrategy(It.IsAny<object[]>())).Returns(startMoveObj.Object);

        new RegisterDependencyCommand("Game.Adapter.Create", (object[] args) => adapterStrategy.Object.RunStrategy(args)).Execute();

        var strategy = new CreateStartMoveCommandStrategy();

        var obj = new Mock<IUObject>();

        var result = strategy.RunStrategy(obj.Object);

        Assert.NotNull(result);
        Assert.IsType<StartCommand>(result);
    }

    [Fact]
    public void SuccessfulCreateStopMoveCommandInStrategy()
    {
        new InitCommand().Execute();
        var scope = Ioc.Resolve<object>("IoC.Scope.Create");
        Ioc.Resolve<App.ICommand>("IoC.Scope.Current.Set", scope).Execute();

        var stopMoveObj = new Mock<IStopable>();

        var adapterStrategy = new Mock<IStrategy>();
        adapterStrategy.Setup(s => s.RunStrategy(It.IsAny<object[]>())).Returns(stopMoveObj.Object);

        new RegisterDependencyCommand("Game.Adapter.Create", (object[] args) => adapterStrategy.Object.RunStrategy(args)).Execute();

        var strategy = new CreateStopMoveCommandStrategy();

        var obj = new Mock<IUObject>();

        var result = strategy.RunStrategy(obj.Object);

        Assert.NotNull(result);
        Assert.IsType<StopCommand>(result);
    }
}
