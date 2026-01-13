namespace Application.Common.Results;

public class Result
{
    public bool IsSuccess { get; }
    public ResultError? Error { get; }

    protected Result(bool isSuccess, ResultError? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Ok() => new(true, null);
    public static Result Fail(string code, string message) => new(false, new ResultError(code, message));
}

public sealed class Result<T> : Result
{
    public T? Value { get; }

    private Result(bool isSuccess, T? value, ResultError? error) : base(isSuccess, error)
    {
        Value = value;
    }

    public static Result<T> Ok(T value) => new(true, value, null);
    public static new Result<T> Fail(string code, string message) => new(false, default, new ResultError(code, message));
}
