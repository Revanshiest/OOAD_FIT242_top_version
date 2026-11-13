using App;
using App.Scopes;

using Moq;

namespace SpaceBattle.Lib.Test;

public class MovingStrategyTests
{
    [Fact]
    public void SuccesfulMovingRunStrategy()
    {
        new InitCommand().Execute();
        var scope = Ioc.Resolve<object>("IoC.Scope.Create");
        Ioc.Resolve<App.ICommand>("IoC.Scope.Current.Set", scope).Execute();

        var mockCommand = new Mock<SpaceBattle.Lib.ICommand>();

        var mockListCommand = new Mock<IEnumerable<SpaceBattle.Lib.ICommand>>();

        var mockStrategyReturnCommand = new Mock<IStrategy>();
        mockStrategyReturnCommand.Setup(x => x.RunStrategy(It.IsAny<object[]>())).Returns(mockCommand.Object).Verifiable();

        var mockStrategyReturnsIEnum = new Mock<IStrategy>();
        mockStrategyReturnsIEnum.Setup(x => x.RunStrategy(It.IsAny<object[]>())).Returns(mockListCommand.Object).Verifiable();

        new RegisterDependencyCommand("Game.CreateMove", (object[] args) => mockStrategyReturnsIEnum.Object.RunStrategy(args)).Execute();
        new RegisterDependencyCommand("Game.Command.Inject", (object[] args) => mockStrategyReturnCommand.Object.RunStrategy(args)).Execute();
        new RegisterDependencyCommand("Game.Command.Macro", (object[] args) => mockStrategyReturnCommand.Object.RunStrategy(args)).Execute();
        new RegisterDependencyCommand("Game.Command.Repeat", (object[] args) => mockStrategyReturnCommand.Object.RunStrategy(args)).Execute();

        var obj = new Mock<IUObject>();

        MovingStrategy strategy = new MovingStrategy();

        strategy.RunStrategy(obj.Object);

        mockStrategyReturnCommand.VerifyAll();
        mockStrategyReturnsIEnum.VerifyAll();
    }
}
