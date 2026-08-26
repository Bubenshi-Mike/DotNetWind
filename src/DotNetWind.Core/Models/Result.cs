namespace DotNetWind.Core.Models;

public sealed class Result
{
    private Result(bool isSuccess, string? errorMessage = null, ResultErrorKind errorKind = ResultErrorKind.General)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        ErrorKind = errorKind;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? ErrorMessage { get; }
    public ResultErrorKind ErrorKind { get; }

    public static Result Success() => new(true);
    public static Result Failure(string errorMessage, ResultErrorKind errorKind = ResultErrorKind.General) =>
        new(false, errorMessage, errorKind);

    public static implicit operator bool(Result result) => result.IsSuccess;
}

public sealed class Result<T>
{
    private Result(bool isSuccess, T? value = default, string? errorMessage = null, ResultErrorKind errorKind = ResultErrorKind.General)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorMessage = errorMessage;
        ErrorKind = errorKind;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; }
    public string? ErrorMessage { get; }
    public ResultErrorKind ErrorKind { get; }

    public static Result<T> Success(T value) => new(true, value);
    public static Result<T> Failure(string errorMessage, ResultErrorKind errorKind = ResultErrorKind.General) =>
        new(false, errorMessage: errorMessage, errorKind: errorKind);

    public static implicit operator bool(Result<T> result) => result.IsSuccess;
}
