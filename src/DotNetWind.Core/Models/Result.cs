namespace DotNetWind.Core.Models;

public sealed class Result
{
    private Result(bool isSuccess, string? errorMessage = null)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? ErrorMessage { get; }

    public static Result Success() => new(true);
    public static Result Failure(string errorMessage) => new(false, errorMessage);

    public static implicit operator bool(Result result) => result.IsSuccess;
}

public sealed class Result<T>
{
    private Result(bool isSuccess, T? value = default, string? errorMessage = null)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorMessage = errorMessage;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; }
    public string? ErrorMessage { get; }

    public static Result<T> Success(T value) => new(true, value);
    public static Result<T> Failure(string errorMessage) => new(false, errorMessage: errorMessage);

    public static implicit operator bool(Result<T> result) => result.IsSuccess;
}
