using App;
using App.Scopes;

using Moq;

namespace SpaceBattle.Lib.Test;

public class AddExceptionHandlerStrategyTests
{
    public AddExceptionHandlerStrategyTests()
    {
        new InitCommand().Execute();
        var scope = Ioc.Resolve<object>("IoC.Scope.Create");
        Ioc.Resolve<App.ICommand>("IoC.Scope.Current.Set", scope).Execute();

        var mockHandler = new Mock<IHandler>();

        var tree = new Dictionary<int, IHandler>()
        {
            {0, mockHandler.Object},
            {1, mockHandler.Object},
            {2, mockHandler.Object},
            {12345, mockHandler.Object}
        };

        var mockStrategyReturnsExseptionTree = new Mock<IStrategy>();
        mockStrategyReturnsExseptionTree.Setup(m => m.RunStrategy()).Returns(tree);

        var StrategyReturnsHash = new GetHashCodeStrategy();

        new RegisterDependencyCommand("Game.GetExceptionTree", (object[] args) => mockStrategyReturnsExseptionTree.Object.RunStrategy(args)).Execute();
        new RegisterDependencyCommand("Game.GetHashCode", (object[] args) => StrategyReturnsHash.RunStrategy(args)).Execute();
    }

    [Fact]
    public void SuccesfulTryAddHandlerInTree()
    {
        var handler = new Mock<IHandler>();

        var cmd = new AddHandlerToExceptionTreeCommand(new List<Type>() { typeof(MoveCommand) }, handler.Object);

        cmd.Execute();

        Assert.Equal(5, Ioc.Resolve<IDictionary<int, IHandler>>("Game.GetExceptionTree").Count);
    }
}
