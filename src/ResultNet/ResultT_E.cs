namespace ResultNet;

public readonly struct Result<T, TCode> where TCode : struct, Enum
{
    private readonly T? _value;
    private readonly Error<TCode> _error;

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    private Result(T value)
    {
        IsSuccess = true;
        _value = value;
        _error = default;
    }

    private Result(Error<TCode> error)
    {
        IsSuccess = false;
        _value = default;
        _error = error;
    }

    public static Result<T, TCode> Success(T value) => new(value);
    public static Result<T, TCode> Failure(Error<TCode> error) => new(error);
    public static Result<T, TCode> Failure(TCode code, string message) => new(new Error<TCode>(code, message));
    public static Result<T, TCode> Failure(string message) => new(new Error<TCode>(default, message));

    public static implicit operator Result<T, TCode>(T value) => new(value);
    public static implicit operator Result<T, TCode>(Error<TCode> error) => new(error);

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access Value on a failed result.");

    public Error<TCode> Error => IsFailure
        ? _error
        : throw new InvalidOperationException("Cannot access Error on a successful result.");

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<Error<TCode>, TResult> onFailure)
        => IsSuccess ? onSuccess(_value!) : onFailure(_error);

    public void Match(Action<T> onSuccess, Action<Error<TCode>> onFailure)
    {
        if (IsSuccess)
            onSuccess(_value!);
        else
            onFailure(_error);
    }
}
