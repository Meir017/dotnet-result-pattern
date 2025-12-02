namespace ResultNet;

public static class Results
{
    public static Result<T> Try<T>(Func<T> func, Func<Exception, Error>? errorMapper = null)
    {
        try
        {
            return Result<T>.Success(func());
        }
        catch (Exception ex)
        {
            var error = errorMapper?.Invoke(ex) ?? new Error("Exception", ex.Message);
            return Result<T>.Failure(error);
        }
    }

    public static Result Try(Action action, Func<Exception, Error>? errorMapper = null)
    {
        try
        {
            action();
            return Result.Success();
        }
        catch (Exception ex)
        {
            var error = errorMapper?.Invoke(ex) ?? new Error("Exception", ex.Message);
            return Result.Failure(error);
        }
    }

    public static async Task<Result<T>> TryAsync<T>(Func<Task<T>> func, Func<Exception, Error>? errorMapper = null)
    {
        try
        {
            var value = await func();
            return Result<T>.Success(value);
        }
        catch (Exception ex)
        {
            var error = errorMapper?.Invoke(ex) ?? new Error("Exception", ex.Message);
            return Result<T>.Failure(error);
        }
    }

    public static async Task<Result> TryAsync(Func<Task> func, Func<Exception, Error>? errorMapper = null)
    {
        try
        {
            await func();
            return Result.Success();
        }
        catch (Exception ex)
        {
            var error = errorMapper?.Invoke(ex) ?? new Error("Exception", ex.Message);
            return Result.Failure(error);
        }
    }

    public static Result<T> Combine<T>(params Result<T>[] results)
    {
        foreach (var result in results)
        {
            if (result.IsFailure)
                return result;
        }
        return results.Length > 0 ? results[^1] : Result<T>.Failure("No results to combine");
    }

    public static Result Combine(params Result[] results)
    {
        foreach (var result in results)
        {
            if (result.IsFailure)
                return result;
        }
        return Result.Success();
    }

    public static Result<IEnumerable<T>> CombineAll<T>(params Result<T>[] results)
    {
        var values = new List<T>();
        foreach (var result in results)
        {
            if (result.IsFailure)
                return Result<IEnumerable<T>>.Failure(result.Error);
            
            values.Add(result.Value);
        }
        return Result<IEnumerable<T>>.Success(values);
    }

    public static Result<T, TCode> Try<T, TCode>(Func<T> func, Func<Exception, Error<TCode>>? errorMapper = null) where TCode : struct, Enum
    {
        try
        {
            return Result<T, TCode>.Success(func());
        }
        catch (Exception ex)
        {
            var error = errorMapper?.Invoke(ex) ?? new Error<TCode>(default, ex.Message);
            return Result<T, TCode>.Failure(error);
        }
    }



    public static async Task<Result<T, TCode>> TryAsync<T, TCode>(Func<Task<T>> func, Func<Exception, Error<TCode>>? errorMapper = null) where TCode : struct, Enum
    {
        try
        {
            var value = await func();
            return Result<T, TCode>.Success(value);
        }
        catch (Exception ex)
        {
            var error = errorMapper?.Invoke(ex) ?? new Error<TCode>(default, ex.Message);
            return Result<T, TCode>.Failure(error);
        }
    }



    public static Result<T, TCode> Combine<T, TCode>(params Result<T, TCode>[] results) where TCode : struct, Enum
    {
        foreach (var result in results)
        {
            if (result.IsFailure)
                return result;
        }
        return results.Length > 0 ? results[^1] : Result<T, TCode>.Failure("No results to combine");
    }



    public static Result<IEnumerable<T>, TCode> CombineAll<T, TCode>(params Result<T, TCode>[] results) where TCode : struct, Enum
    {
        var values = new List<T>();
        foreach (var result in results)
        {
            if (result.IsFailure)
                return Result<IEnumerable<T>, TCode>.Failure(result.Error);
            
            values.Add(result.Value);
        }
        return Result<IEnumerable<T>, TCode>.Success(values);
    }
}
