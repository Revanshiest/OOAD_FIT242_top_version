using SpaceBattle.Lib;

namespace SpaceBattle.Lib.Test;

public class ActionCommandTests
{
    [Fact]
    public void SuccessfulActionCommandExecute()
    {
        var executed = false;
        var command = new ActionCommand(() => executed = true);

        command.Execute();

        Assert.True(executed);
    }
}

