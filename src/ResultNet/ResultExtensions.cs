namespace ResultNet;

public static class ResultExtensions
{
    public static Result<TOut> Map<TIn, TOut>(this Result<TIn> result, Func<TIn, TOut> mapper)
    {
        return result.IsSuccess
            ? Result<TOut>.Success(mapper(result.Value))
            : Result<TOut>.Failure(result.Error);
    }

    public static Result<TOut> Bind<TIn, TOut>(this Result<TIn> result, Func<TIn, Result<TOut>> binder)
    {
        return result.IsSuccess
            ? binder(result.Value)
            : Result<TOut>.Failure(result.Error);
    }

    public static Result<T> Ensure<T>(this Result<T> result, Func<T, bool> predicate, Error error)
    {
        if (result.IsFailure)
            return result;

        return predicate(result.Value)
            ? result
            : Result<T>.Failure(error);
    }

    public static Result<T> Tap<T>(this Result<T> result, Action<T> action)
    {
        if (result.IsSuccess)
            action(result.Value);

        return result;
    }

    public static Result Tap(this Result result, Action action)
    {
        if (result.IsSuccess)
            action();

        return result;
    }

    public static Result<T> TapError<T>(this Result<T> result, Action<Error> action)
    {
        if (result.IsFailure)
            action(result.Error);

        return result;
    }

    public static Result TapError(this Result result, Action<Error> action)
    {
        if (result.IsFailure)
            action(result.Error);

        return result;
    }

    public static T ValueOr<T>(this Result<T> result, T defaultValue)
    {
        return result.IsSuccess ? result.Value : defaultValue;
    }

    public static T ValueOr<T>(this Result<T> result, Func<Error, T> defaultValueFactory)
    {
        return result.IsSuccess ? result.Value : defaultValueFactory(result.Error);
    }

    public static Result<T> OnSuccess<T>(this Result<T> result, Action<T> action)
    {
        if (result.IsSuccess)
            action(result.Value);

        return result;
    }

    public static Result OnSuccess(this Result result, Action action)
    {
        if (result.IsSuccess)
            action();

        return result;
    }

    public static Result<T> OnFailure<T>(this Result<T> result, Action<Error> action)
    {
        if (result.IsFailure)
            action(result.Error);

        return result;
    }

    public static Result OnFailure(this Result result, Action<Error> action)
    {
        if (result.IsFailure)
            action(result.Error);

        return result;
    }

    // Direct Result<T> async extensions
    public static async Task<Result<TOut>> MapAsync<TIn, TOut>(this Result<TIn> result, Func<TIn, Task<TOut>> mapper)
    {
        if (result.IsFailure)
            return Result<TOut>.Failure(result.Error);

        var value = await mapper(result.Value);
        return Result<TOut>.Success(value);
    }

    public static async Task<Result<TOut>> BindAsync<TIn, TOut>(this Result<TIn> result, Func<TIn, Task<Result<TOut>>> binder)
    {
        if (result.IsFailure)
            return Result<TOut>.Failure(result.Error);

        return await binder(result.Value);
    }

    public static async Task<Result<T>> EnsureAsync<T>(this Result<T> result, Func<T, Task<bool>> predicate, Error error)
    {
        if (result.IsFailure)
            return result;

        var isValid = await predicate(result.Value);
        return isValid ? result : Result<T>.Failure(error);
    }

    public static async Task<Result<T>> TapAsync<T>(this Result<T> result, Func<T, Task> action)
    {
        if (result.IsSuccess)
            await action(result.Value);

        return result;
    }

    public static async Task<Result> TapAsync(this Result result, Func<Task> action)
    {
        if (result.IsSuccess)
            await action();

        return result;
    }

    public static async Task<Result<T>> TapErrorAsync<T>(this Result<T> result, Func<Error, Task> action)
    {
        if (result.IsFailure)
            await action(result.Error);

        return result;
    }

    public static async Task<Result> TapErrorAsync(this Result result, Func<Error, Task> action)
    {
        if (result.IsFailure)
            await action(result.Error);

        return result;
    }

    public static async Task<Result<T>> OnSuccessAsync<T>(this Result<T> result, Func<T, Task> action)
    {
        if (result.IsSuccess)
            await action(result.Value);

        return result;
    }

    public static async Task<Result> OnSuccessAsync(this Result result, Func<Task> action)
    {
        if (result.IsSuccess)
            await action();

        return result;
    }

    public static async Task<Result<T>> OnFailureAsync<T>(this Result<T> result, Func<Error, Task> action)
    {
        if (result.IsFailure)
            await action(result.Error);

        return result;
    }

    public static async Task<Result> OnFailureAsync(this Result result, Func<Error, Task> action)
    {
        if (result.IsFailure)
            await action(result.Error);

        return result;
    }

    // Task<Result<T>> extensions
    public static async Task<Result<TOut>> MapAsync<TIn, TOut>(this Task<Result<TIn>> resultTask, Func<TIn, TOut> mapper)
    {
        var result = await resultTask;
        return result.Map(mapper);
    }

    public static async Task<Result<TOut>> MapAsync<TIn, TOut>(this Task<Result<TIn>> resultTask, Func<TIn, Task<TOut>> mapper)
    {
        var result = await resultTask;
        return await result.MapAsync(mapper);
    }

    public static async Task<Result<TOut>> BindAsync<TIn, TOut>(this Task<Result<TIn>> resultTask, Func<TIn, Result<TOut>> binder)
    {
        var result = await resultTask;
        return result.Bind(binder);
    }

    public static async Task<Result<TOut>> BindAsync<TIn, TOut>(this Task<Result<TIn>> resultTask, Func<TIn, Task<Result<TOut>>> binder)
    {
        var result = await resultTask;
        return await result.BindAsync(binder);
    }

    public static async Task<Result<T>> EnsureAsync<T>(this Task<Result<T>> resultTask, Func<T, bool> predicate, Error error)
    {
        var result = await resultTask;
        return result.Ensure(predicate, error);
    }

    public static async Task<Result<T>> EnsureAsync<T>(this Task<Result<T>> resultTask, Func<T, Task<bool>> predicate, Error error)
    {
        var result = await resultTask;
        return await result.EnsureAsync(predicate, error);
    }

    public static async Task<Result<T>> TapAsync<T>(this Task<Result<T>> resultTask, Func<T, Task> action)
    {
        var result = await resultTask;
        return await result.TapAsync(action);
    }

    public static async Task<Result> TapAsync(this Task<Result> resultTask, Func<Task> action)
    {
        var result = await resultTask;
        return await result.TapAsync(action);
    }

    public static async Task<Result<T>> TapErrorAsync<T>(this Task<Result<T>> resultTask, Func<Error, Task> action)
    {
        var result = await resultTask;
        return await result.TapErrorAsync(action);
    }

    public static async Task<Result> TapErrorAsync(this Task<Result> resultTask, Func<Error, Task> action)
    {
        var result = await resultTask;
        return await result.TapErrorAsync(action);
    }

    public static async Task<Result<T>> OnSuccessAsync<T>(this Task<Result<T>> resultTask, Func<T, Task> action)
    {
        var result = await resultTask;
        return await result.OnSuccessAsync(action);
    }

    public static async Task<Result> OnSuccessAsync(this Task<Result> resultTask, Func<Task> action)
    {
        var result = await resultTask;
        return await result.OnSuccessAsync(action);
    }

    public static async Task<Result<T>> OnFailureAsync<T>(this Task<Result<T>> resultTask, Func<Error, Task> action)
    {
        var result = await resultTask;
        return await result.OnFailureAsync(action);
    }

    public static async Task<Result> OnFailureAsync(this Task<Result> resultTask, Func<Error, Task> action)
    {
        var result = await resultTask;
        return await result.OnFailureAsync(action);
    }
}
