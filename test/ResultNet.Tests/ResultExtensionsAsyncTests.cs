namespace ResultNet.Tests;

public class ResultExtensionsAsyncTests
{
    [Fact]
    public async Task MapAsync_OnSuccess_TransformsValue()
    {
        var resultTask = Task.FromResult(Result<int>.Success(42));

        var mapped = await resultTask.MapAsync(x => x.ToString());

        Assert.True(mapped.IsSuccess);
        Assert.Equal("42", mapped.Value);
    }

    [Fact]
    public async Task MapAsync_OnFailure_PropagatesError()
    {
        var error = new Error("TestError", "Test message");
        var resultTask = Task.FromResult(Result<int>.Failure(error));

        var mapped = await resultTask.MapAsync(x => x.ToString());

        Assert.True(mapped.IsFailure);
        Assert.Equal(error, mapped.Error);
    }

    [Fact]
    public async Task MapAsync_WithAsyncMapper_OnSuccess_TransformsValue()
    {
        var resultTask = Task.FromResult(Result<int>.Success(42));

        var mapped = await resultTask.MapAsync(async x =>
        {
            await Task.Delay(1);
            return x.ToString();
        });

        Assert.True(mapped.IsSuccess);
        Assert.Equal("42", mapped.Value);
    }

    [Fact]
    public async Task MapAsync_WithAsyncMapper_OnFailure_PropagatesError()
    {
        var error = new Error("TestError", "Test message");
        var resultTask = Task.FromResult(Result<int>.Failure(error));

        var mapped = await resultTask.MapAsync(async x =>
        {
            await Task.Delay(1);
            return x.ToString();
        });

        Assert.True(mapped.IsFailure);
        Assert.Equal(error, mapped.Error);
    }

    [Fact]
    public async Task BindAsync_OnSuccess_ExecutesBinder()
    {
        var resultTask = Task.FromResult(Result<int>.Success(42));

        var bound = await resultTask.BindAsync(x => Result<string>.Success(x.ToString()));

        Assert.True(bound.IsSuccess);
        Assert.Equal("42", bound.Value);
    }

    [Fact]
    public async Task BindAsync_OnFailure_PropagatesError()
    {
        var error = new Error("TestError", "Test message");
        var resultTask = Task.FromResult(Result<int>.Failure(error));

        var bound = await resultTask.BindAsync(x => Result<string>.Success(x.ToString()));

        Assert.True(bound.IsFailure);
        Assert.Equal(error, bound.Error);
    }

    [Fact]
    public async Task BindAsync_WithAsyncBinder_OnSuccess_ExecutesBinder()
    {
        var resultTask = Task.FromResult(Result<int>.Success(42));

        var bound = await resultTask.BindAsync(async x =>
        {
            await Task.Delay(1);
            return Result<string>.Success(x.ToString());
        });

        Assert.True(bound.IsSuccess);
        Assert.Equal("42", bound.Value);
    }

    [Fact]
    public async Task BindAsync_WithAsyncBinder_OnFailure_PropagatesError()
    {
        var error = new Error("TestError", "Test message");
        var resultTask = Task.FromResult(Result<int>.Failure(error));

        var bound = await resultTask.BindAsync(async x =>
        {
            await Task.Delay(1);
            return Result<string>.Success(x.ToString());
        });

        Assert.True(bound.IsFailure);
        Assert.Equal(error, bound.Error);
    }

    [Fact]
    public async Task TapAsync_OnSuccess_ExecutesAction()
    {
        var resultTask = Task.FromResult(Result<int>.Success(42));
        var capturedValue = 0;

        var result = await resultTask.TapAsync(async x =>
        {
            await Task.Delay(1);
            capturedValue = x;
        });

        Assert.Equal(42, capturedValue);
        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public async Task TapAsync_OnFailure_DoesNotExecuteAction()
    {
        var resultTask = Task.FromResult(Result<int>.Failure("error"));
        var executed = false;

        var result = await resultTask.TapAsync(async _ =>
        {
            await Task.Delay(1);
            executed = true;
        });

        Assert.False(executed);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task ChainedAsyncOperations_Success_ExecutesAllSteps()
    {
        var resultTask = Task.FromResult(Result<int>.Success(5));

        var result = await resultTask
            .MapAsync(x => x * 2)
            .MapAsync(async x =>
            {
                await Task.Delay(1);
                return x + 10;
            })
            .BindAsync(async x =>
            {
                await Task.Delay(1);
                return Result<string>.Success(x.ToString());
            })
            .TapAsync(async x =>
            {
                await Task.Delay(1);
            });

        Assert.True(result.IsSuccess);
        Assert.Equal("20", result.Value);
    }

    [Fact]
    public async Task ChainedAsyncOperations_WithFailure_StopsAtFirstFailure()
    {
        var resultTask = Task.FromResult(Result<int>.Success(5));
        var tapExecuted = false;

        var result = await resultTask
            .MapAsync(x => x * 2)
            .BindAsync(x => Result<int>.Failure("operation failed"))
            .MapAsync(x => x + 10)
            .TapAsync(async x =>
            {
                await Task.Delay(1);
                tapExecuted = true;
            });

        Assert.True(result.IsFailure);
        Assert.False(tapExecuted);
        Assert.Equal("operation failed", result.Error.Message);
    }

    [Fact]
    public async Task RealWorldExample_AsyncPipeline()
    {
        async Task<int> FetchUserIdAsync(string username)
        {
            await Task.Delay(1);
            return username == "admin" ? 1 : 0;
        }

        async Task<string> FetchUserEmailAsync(int userId)
        {
            await Task.Delay(1);
            return userId > 0 ? $"user{userId}@example.com" : throw new InvalidOperationException("User not found");
        }

        var result = await Results.TryAsync(() => FetchUserIdAsync("admin"))
            .BindAsync(async userId =>
            {
                if (userId <= 0)
                    return Result<int>.Failure("User not found");
                return Result<int>.Success(userId);
            })
            .BindAsync(async userId =>
            {
                var email = await FetchUserEmailAsync(userId);
                return Result<string>.Success(email);
            })
            .MapAsync(email => email.ToUpper());

        Assert.True(result.IsSuccess);
        Assert.Equal("USER1@EXAMPLE.COM", result.Value);
    }
}
