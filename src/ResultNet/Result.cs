namespace ResultNet;

public readonly struct Result
{
    private readonly Error _error;

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    private Result(bool isSuccess, Error error = default)
    {
        IsSuccess = isSuccess;
        _error = error;
    }

    public static Result Success() => new(true);
    public static Result Failure(Error error) => new(false, error);
    public static Result Failure(string message) => new(false, new Error("Error", message));

    public static implicit operator Result(Error error) => Failure(error);

    public Error Error => IsFailure
        ? _error
        : throw new InvalidOperationException("Cannot access Error on a successful result.");

    public TResult Match<TResult>(Func<TResult> onSuccess, Func<Error, TResult> onFailure)
        => IsSuccess ? onSuccess() : onFailure(_error);

    public void Match(Action onSuccess, Action<Error> onFailure)
    {
        if (IsSuccess)
            onSuccess();
        else
            onFailure(_error);
    }
}
