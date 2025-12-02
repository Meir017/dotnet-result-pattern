namespace ResultNet.Tests;

public class AsyncEnhancementsTests
{
    [Fact]
    public async Task Result_MapAsync_WithAsyncMapper_TransformsValue()
    {
        var result = Result<int>.Success(42);

        var mapped = await result.MapAsync(async x =>
        {
            await Task.Delay(1);
            return x.ToString();
        });

        Assert.True(mapped.IsSuccess);
        Assert.Equal("42", mapped.Value);
    }

    [Fact]
    public async Task Result_MapAsync_OnFailure_PropagatesError()
    {
        var result = Result<int>.Failure("error");

        var mapped = await result.MapAsync(async x =>
        {
            await Task.Delay(1);
            return x.ToString();
        });

        Assert.True(mapped.IsFailure);
        Assert.Equal("error", mapped.Error.Message);
    }

    [Fact]
    public async Task Result_BindAsync_WithAsyncBinder_ExecutesBinder()
    {
        var result = Result<int>.Success(42);

        var bound = await result.BindAsync(async x =>
        {
            await Task.Delay(1);
            return Result<string>.Success(x.ToString());
        });

        Assert.True(bound.IsSuccess);
        Assert.Equal("42", bound.Value);
    }

    [Fact]
    public async Task Result_BindAsync_OnFailure_PropagatesError()
    {
        var result = Result<int>.Failure("error");

        var bound = await result.BindAsync(async x =>
        {
            await Task.Delay(1);
            return Result<string>.Success(x.ToString());
        });

        Assert.True(bound.IsFailure);
        Assert.Equal("error", bound.Error.Message);
    }

    [Fact]
    public async Task Result_EnsureAsync_WithValidAsyncPredicate_ReturnsSuccess()
    {
        var result = Result<int>.Success(42);

        var ensured = await result.EnsureAsync(async x =>
        {
            await Task.Delay(1);
            return x > 0;
        }, "Must be positive");

        Assert.True(ensured.IsSuccess);
        Assert.Equal(42, ensured.Value);
    }

    [Fact]
    public async Task Result_EnsureAsync_WithInvalidAsyncPredicate_ReturnsFailure()
    {
        var result = Result<int>.Success(-5);

        var ensured = await result.EnsureAsync(async x =>
        {
            await Task.Delay(1);
            return x > 0;
        }, "Must be positive");

        Assert.True(ensured.IsFailure);
        Assert.Equal("Must be positive", ensured.Error.Message);
    }

    [Fact]
    public async Task Result_TapAsync_OnSuccess_ExecutesAsyncAction()
    {
        var result = Result<int>.Success(42);
        var capturedValue = 0;

        var newResult = await result.TapAsync(async x =>
        {
            await Task.Delay(1);
            capturedValue = x;
        });

        Assert.Equal(42, capturedValue);
        Assert.True(newResult.IsSuccess);
        Assert.Equal(42, newResult.Value);
    }

    [Fact]
    public async Task Result_TapAsync_OnFailure_DoesNotExecuteAction()
    {
        var result = Result<int>.Failure("error");
        var executed = false;

        var newResult = await result.TapAsync(async _ =>
        {
            await Task.Delay(1);
            executed = true;
        });

        Assert.False(executed);
        Assert.True(newResult.IsFailure);
    }

    [Fact]
    public async Task Result_NonGeneric_TapAsync_OnSuccess_ExecutesAction()
    {
        var result = Result.Success();
        var executed = false;

        var newResult = await result.TapAsync(async () =>
        {
            await Task.Delay(1);
            executed = true;
        });

        Assert.True(executed);
        Assert.True(newResult.IsSuccess);
    }

    [Fact]
    public async Task Result_TapErrorAsync_OnFailure_ExecutesAsyncAction()
    {
        var result = Result<int>.Failure("error");
        Error? capturedError = null;

        var newResult = await result.TapErrorAsync(async error =>
        {
            await Task.Delay(1);
            capturedError = error;
        });

        Assert.NotNull(capturedError);
        Assert.Equal("error", capturedError.Value.Message);
        Assert.True(newResult.IsFailure);
    }

    [Fact]
    public async Task Result_TapErrorAsync_OnSuccess_DoesNotExecuteAction()
    {
        var result = Result<int>.Success(42);
        var executed = false;

        var newResult = await result.TapErrorAsync(async _ =>
        {
            await Task.Delay(1);
            executed = true;
        });

        Assert.False(executed);
        Assert.True(newResult.IsSuccess);
    }

    [Fact]
    public async Task Result_OnSuccessAsync_OnSuccess_ExecutesAsyncAction()
    {
        var result = Result<int>.Success(42);
        var capturedValue = 0;

        var newResult = await result.OnSuccessAsync(async x =>
        {
            await Task.Delay(1);
            capturedValue = x;
        });

        Assert.Equal(42, capturedValue);
        Assert.True(newResult.IsSuccess);
    }

    [Fact]
    public async Task Result_OnSuccessAsync_OnFailure_DoesNotExecuteAction()
    {
        var result = Result<int>.Failure("error");
        var executed = false;

        var newResult = await result.OnSuccessAsync(async _ =>
        {
            await Task.Delay(1);
            executed = true;
        });

        Assert.False(executed);
        Assert.True(newResult.IsFailure);
    }

    [Fact]
    public async Task Result_OnFailureAsync_OnFailure_ExecutesAsyncAction()
    {
        var result = Result<int>.Failure("error");
        Error? capturedError = null;

        var newResult = await result.OnFailureAsync(async error =>
        {
            await Task.Delay(1);
            capturedError = error;
        });

        Assert.NotNull(capturedError);
        Assert.Equal("error", capturedError.Value.Message);
        Assert.True(newResult.IsFailure);
    }

    [Fact]
    public async Task Result_OnFailureAsync_OnSuccess_DoesNotExecuteAction()
    {
        var result = Result<int>.Success(42);
        var executed = false;

        var newResult = await result.OnFailureAsync(async _ =>
        {
            await Task.Delay(1);
            executed = true;
        });

        Assert.False(executed);
        Assert.True(newResult.IsSuccess);
    }

    [Fact]
    public async Task TaskResult_EnsureAsync_WithSyncPredicate_Works()
    {
        var resultTask = Task.FromResult(Result<int>.Success(42));

        var ensured = await resultTask.EnsureAsync(x => x > 0, "Must be positive");

        Assert.True(ensured.IsSuccess);
        Assert.Equal(42, ensured.Value);
    }

    [Fact]
    public async Task TaskResult_EnsureAsync_WithAsyncPredicate_Works()
    {
        var resultTask = Task.FromResult(Result<int>.Success(42));

        var ensured = await resultTask.EnsureAsync(async x =>
        {
            await Task.Delay(1);
            return x > 0;
        }, "Must be positive");

        Assert.True(ensured.IsSuccess);
        Assert.Equal(42, ensured.Value);
    }

    [Fact]
    public async Task TaskResult_TapErrorAsync_OnFailure_ExecutesAction()
    {
        var resultTask = Task.FromResult(Result<int>.Failure("error"));
        Error? capturedError = null;

        var result = await resultTask.TapErrorAsync(async error =>
        {
            await Task.Delay(1);
            capturedError = error;
        });

        Assert.NotNull(capturedError);
        Assert.Equal("error", capturedError.Value.Message);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task TaskResult_OnSuccessAsync_OnSuccess_ExecutesAction()
    {
        var resultTask = Task.FromResult(Result<int>.Success(42));
        var capturedValue = 0;

        var result = await resultTask.OnSuccessAsync(async x =>
        {
            await Task.Delay(1);
            capturedValue = x;
        });

        Assert.Equal(42, capturedValue);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task TaskResult_OnFailureAsync_OnFailure_ExecutesAction()
    {
        var resultTask = Task.FromResult(Result<int>.Failure("error"));
        Error? capturedError = null;

        var result = await resultTask.OnFailureAsync(async error =>
        {
            await Task.Delay(1);
            capturedError = error;
        });

        Assert.NotNull(capturedError);
        Assert.Equal("error", capturedError.Value.Message);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task ComplexAsyncChaining_FromResult_ExecutesAllSteps()
    {
        var log = new List<string>();

        var result = await Result<int>.Success(5)
            .TapAsync(async x =>
            {
                await Task.Delay(1);
                log.Add($"Start: {x}");
            })
            .MapAsync(async x =>
            {
                await Task.Delay(1);
                return x * 2;
            })
            .EnsureAsync(async x =>
            {
                await Task.Delay(1);
                return x > 5;
            }, "Must be greater than 5")
            .BindAsync(async x =>
            {
                await Task.Delay(1);
                return Result<string>.Success(x.ToString());
            })
            .OnSuccessAsync(async value =>
            {
                await Task.Delay(1);
                log.Add($"End: {value}");
            });

        Assert.True(result.IsSuccess);
        Assert.Equal("10", result.Value);
        Assert.Equal(2, log.Count);
        Assert.Equal("Start: 5", log[0]);
        Assert.Equal("End: 10", log[1]);
    }

    [Fact]
    public async Task ComplexAsyncChaining_WithFailure_StopsAtFirstFailure()
    {
        var log = new List<string>();

        var result = await Result<int>.Success(5)
            .MapAsync(async x =>
            {
                await Task.Delay(1);
                log.Add("Map executed");
                return x * 2;
            })
            .EnsureAsync(async x =>
            {
                await Task.Delay(1);
                return x > 20;
            }, "Must be greater than 20")
            .BindAsync(async x =>
            {
                await Task.Delay(1);
                log.Add("Bind executed");
                return Result<string>.Success(x.ToString());
            });

        Assert.True(result.IsFailure);
        Assert.Equal("Must be greater than 20", result.Error.Message);
        Assert.Single(log);
        Assert.Equal("Map executed", log[0]);
    }

    [Fact]
    public async Task RealWorldExample_AsyncDatabaseValidation()
    {
        async Task<bool> UsernameExistsAsync(string username)
        {
            await Task.Delay(1);
            return username == "existing_user";
        }

        async Task<bool> EmailExistsAsync(string email)
        {
            await Task.Delay(1);
            return email == "existing@example.com";
        }

        var result = await Result<string>.Success("new_user")
            .EnsureAsync(async username =>
            {
                var exists = await UsernameExistsAsync(username);
                return !exists;
            }, "Username already exists")
            .BindAsync(async username =>
            {
                var email = $"{username}@example.com";
                var emailExists = await EmailExistsAsync(email);
                return emailExists
                    ? Result<string>.Failure("Email already exists")
                    : Result<string>.Success(email);
            })
            .OnSuccessAsync(async email =>
            {
                await Task.Delay(1); // Simulate sending email
            });

        Assert.True(result.IsSuccess);
        Assert.Equal("new_user@example.com", result.Value);
    }

    [Fact]
    public async Task RealWorldExample_AsyncFileProcessing()
    {
        async Task<string> ReadFileAsync(string path)
        {
            await Task.Delay(1);
            return path == "valid.txt" ? "file content" : throw new FileNotFoundException();
        }

        async Task<string> ProcessContentAsync(string content)
        {
            await Task.Delay(1);
            return content.ToUpper();
        }

        async Task SaveFileAsync(string content)
        {
            await Task.Delay(1);
        }

        var result = await Results.Try(() => "valid.txt")
            .BindAsync(async path =>
            {
                var content = await ReadFileAsync(path);
                return Result<string>.Success(content);
            })
            .MapAsync(ProcessContentAsync)
            .TapAsync(SaveFileAsync);

        Assert.True(result.IsSuccess);
        Assert.Equal("FILE CONTENT", result.Value);
    }

    [Fact]
    public async Task MixedSyncAsyncChaining_WorksSeamlessly()
    {
        var result = await Result<int>.Success(5)
            .Map(x => x * 2)                           // Sync -> Result<int>
            .MapAsync(async x =>                        // Async -> Task<Result<int>>
            {
                await Task.Delay(1);
                return x + 5;
            })
            .EnsureAsync(x => x > 10, "Too small")     // Task -> Task<Result<int>>
            .BindAsync(async x =>                       // Task -> Task<Result<string>>
            {
                await Task.Delay(1);
                return Result<string>.Success(x.ToString());
            })
            .MapAsync(s => $"Result: {s}");            // Task -> Task<Result<string>>

        Assert.True(result.IsSuccess);
        Assert.Equal("Result: 15", result.Value);
    }
}
