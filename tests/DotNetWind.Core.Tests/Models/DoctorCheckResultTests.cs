namespace DotNetWind.Core.Tests.Models;

public sealed class DoctorCheckResultTests
{
    [Fact]
    public void PassCheck_ShouldHaveCorrectFlags()
    {
        var check = new DoctorCheckResult("Node.js", DoctorStatus.Pass, "v22.0.0");
        check.IsPass.ShouldBeTrue();
        check.IsWarning.ShouldBeFalse();
        check.IsFail.ShouldBeFalse();
    }

    [Fact]
    public void FailCheck_ShouldHaveRecommendation()
    {
        var check = new DoctorCheckResult("Node.js", DoctorStatus.Fail, "Not found", "Install Node.js");
        check.IsFail.ShouldBeTrue();
        check.Recommendation.ShouldBe("Install Node.js");
    }
}
