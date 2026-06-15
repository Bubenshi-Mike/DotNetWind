namespace DotNetWind.Core.Tests.Models;

public sealed class ResultTests
{
    [Fact]
    public void Success_ShouldHaveIsSuccessTrue()
    {
        var result = Result.Success();
        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.ErrorMessage.ShouldBeNull();
    }

    [Fact]
    public void Failure_ShouldHaveIsFailureTrue()
    {
        var result = Result.Failure("Something went wrong");
        result.IsFailure.ShouldBeTrue();
        result.IsSuccess.ShouldBeFalse();
        result.ErrorMessage.ShouldBe("Something went wrong");
    }

    [Fact]
    public void GenericSuccess_ShouldContainValue()
    {
        var result = Result<string>.Success("hello");
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe("hello");
    }

    [Fact]
    public void GenericFailure_ShouldHaveErrorMessage()
    {
        var result = Result<string>.Failure("oops");
        result.IsFailure.ShouldBeTrue();
        result.ErrorMessage.ShouldBe("oops");
        result.Value.ShouldBeNull();
    }

    [Fact]
    public void ImplicitBoolConversion_Success_ShouldBeTrue()
    {
        Result result = Result.Success();
        bool value = result;
        value.ShouldBeTrue();
    }

    [Fact]
    public void ImplicitBoolConversion_Failure_ShouldBeFalse()
    {
        Result result = Result.Failure("error");
        bool value = result;
        value.ShouldBeFalse();
    }
}
