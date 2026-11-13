using App;
using App.Scopes;

using Moq;

namespace SpaceBattle.Lib.Test;

public class CreateContiniousCommandStrategyTests
{
    [Fact]
    public void SuccesfulCreateContiniousCommandRunStrategy()
    {
        new InitCommand().Execute();
        var scope = Ioc.Resolve<object>("IoC.Scope.Create");
        Ioc.Resolve<App.ICommand>("IoC.Scope.Current.Set", scope).Execute();

        var mockCommand = new Mock<ICommand>();

        var mockIUObj = new Mock<IUObject>();

        var mockStrategyReturnsMacroCommand = new Mock<IStrategy>();
        mockStrategyReturnsMacroCommand.Setup(m => m.RunStrategy(It.IsAny<IEnumerable<ICommand>>())).Returns(mockCommand.Object).Verifiable();

        var mockStrategyReturnsCommand = new Mock<IStrategy>();
        mockStrategyReturnsMacroCommand.Setup(m => m.RunStrategy(It.IsAny<IUObject>())).Returns(mockCommand.Object).Verifiable();

        var mockIEnumString = new Mock<IEnumerable<string>>();

        var mockStrategyReturnsIEnumString = new Mock<IStrategy>();
        mockStrategyReturnsIEnumString.Setup(m => m.RunStrategy()).Returns(mockIEnumString.Object).Verifiable();

        new RegisterDependencyCommand("SetupOperation.MyName", (object[] args) => mockStrategyReturnsIEnumString.Object.RunStrategy(args)).Execute();
        new RegisterDependencyCommand("CreateMacroCommand", (object[] args) => mockStrategyReturnsMacroCommand.Object.RunStrategy(args)).Execute();
        new RegisterDependencyCommand("MyName", (object[] args) => mockStrategyReturnsCommand.Object.RunStrategy(args)).Execute();

        var strategy = new CreatePartCommandStrategy();

        strategy.RunStrategy("MyName", mockIUObj.Object);

        mockStrategyReturnsCommand.VerifyAll();
        mockStrategyReturnsIEnumString.VerifyAll();
    }
}
