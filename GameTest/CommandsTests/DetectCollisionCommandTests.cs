using App;
using App.Scopes;

using Moq;

namespace SpaceBattle.Lib.Test;

public class DetectCollisionCommandTests
{
    public DetectCollisionCommandTests()
    {
        new InitCommand().Execute();
        var scope = Ioc.Resolve<object>("IoC.Scope.Create");
        Ioc.Resolve<App.ICommand>("IoC.Scope.Current.Set", scope).Execute();

        var tree = new Dictionary<int, object>()
        {
            {2, new Dictionary<int, object>()}
        };

        var mockStrategyReturnsTree = new Mock<IStrategy>();
        mockStrategyReturnsTree.Setup(m => m.RunStrategy()).Returns(tree);

        var mockStrategyReturnsList = new Mock<IStrategy>();
        mockStrategyReturnsList.Setup(m => m.RunStrategy(It.IsAny<IUObject>())).Returns(new List<int>());

        new RegisterDependencyCommand("Game.GetSolutionTree", (object[] args) => mockStrategyReturnsTree.Object.RunStrategy(args)).Execute();
        new RegisterDependencyCommand("Game.IUObject.UnionPropertiesForCollision", (object[] args) => mockStrategyReturnsList.Object.RunStrategy(args)).Execute();
    }

    [Fact]
    public void SuccesfulDetectCollisionCommandTests()
    {
        var obj1 = new Mock<IUObject>();
        var obj2 = new Mock<IUObject>();

        var cmd = new Mock<ICommand>();
        cmd.Setup(m => m.Execute()).Verifiable();

        new RegisterDependencyCommand("Game.PrepareDataToCollision", (object[] args) => new List<int>() { 2 }).Execute();
        new RegisterDependencyCommand("Game.Collision", (object[] args) => cmd.Object).Execute();

        var detect = new DetectCollisionCommand(obj1.Object, obj2.Object);
        detect.Execute();

        cmd.VerifyAll();
    }

    [Fact]
    public void NotSuccesfulDetectCollisionCommandTests()
    {
        var obj1 = new Mock<IUObject>();
        var obj2 = new Mock<IUObject>();

        var cmd = new Mock<ICommand>();
        cmd.Setup(m => m.Execute()).Verifiable();

        new RegisterDependencyCommand("Game.PrepareDataToCollision", (object[] args) => new List<int>() { 3 }).Execute();
        new RegisterDependencyCommand("Game.Collision", (object[] args) => cmd.Object).Execute();

        var detect = new DetectCollisionCommand(obj1.Object, obj2.Object);
        detect.Execute();

        cmd.Verify(c => c.Execute(), Times.Never);
    }
}
