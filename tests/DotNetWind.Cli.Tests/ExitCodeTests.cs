namespace DotNetWind.Cli.Tests;

public sealed class ExitCodeTests
{
    [Fact]
    public void ExitCodes_ShouldHaveExpectedValues()
    {
        ExitCode.Success.ShouldBe(0);
        ExitCode.GeneralFailure.ShouldBe(1);
        ExitCode.ValidationFailed.ShouldBe(2);
        ExitCode.UnsupportedProjectType.ShouldBe(3);
        ExitCode.MissingDependency.ShouldBe(4);
        ExitCode.UserCancelled.ShouldBe(5);
    }
}
