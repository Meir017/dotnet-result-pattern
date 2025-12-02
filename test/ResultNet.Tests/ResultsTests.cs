namespace ResultNet.Tests;

public class ResultsTests
{
    [Fact]
    public void Try_WithSuccessfulFunc_ReturnsSuccess()
    {
        var result = Results.Try(() => 42);

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void Try_WithThrowingFunc_ReturnsFailure()
    {
        var result = Results.Try<int>(() => throw new InvalidOperationException("Test error"));

        Assert.True(result.IsFailure);
        Assert.Equal("Exception", result.Error.Code);
        Assert.Equal("Test error", result.Error.Message);
    }

    [Fact]
    public void Try_WithCustomErrorMapper_UsesMapper()
    {
        var result = Results.Try<int>(
            () => throw new InvalidOperationException("Test error"),
            ex => new Error("CustomError", $"Custom: {ex.Message}"));

        Assert.True(result.IsFailure);
        Assert.Equal("CustomError", result.Error.Code);
        Assert.Equal("Custom: Test error", result.Error.Message);
    }

    [Fact]
    public void Try_WithAction_Success_ReturnsSuccess()
    {
        var executed = false;
        var result = Results.Try(() => executed = true);

        Assert.True(result.IsSuccess);
        Assert.True(executed);
    }

    [Fact]
    public void Try_WithAction_ThrowsException_ReturnsFailure()
    {
        var result = Results.Try(() => throw new InvalidOperationException("Test error"));

        Assert.True(result.IsFailure);
        Assert.Equal("Exception", result.Error.Code);
        Assert.Equal("Test error", result.Error.Message);
    }

    [Fact]
    public async Task TryAsync_WithSuccessfulFunc_ReturnsSuccess()
    {
        var result = await Results.TryAsync(async () =>
        {
            await Task.Delay(1);
            return 42;
        });

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public async Task TryAsync_WithThrowingFunc_ReturnsFailure()
    {
        var result = await Results.TryAsync<int>(async () =>
        {
            await Task.Delay(1);
            throw new InvalidOperationException("Test error");
        });

        Assert.True(result.IsFailure);
        Assert.Equal("Exception", result.Error.Code);
        Assert.Equal("Test error", result.Error.Message);
    }

    [Fact]
    public async Task TryAsync_WithAction_Success_ReturnsSuccess()
    {
        var executed = false;
        var result = await Results.TryAsync(async () =>
        {
            await Task.Delay(1);
            executed = true;
        });

        Assert.True(result.IsSuccess);
        Assert.True(executed);
    }

    [Fact]
    public async Task TryAsync_WithAction_ThrowsException_ReturnsFailure()
    {
        var result = await Results.TryAsync(async () =>
        {
            await Task.Delay(1);
            throw new InvalidOperationException("Test error");
        });

        Assert.True(result.IsFailure);
        Assert.Equal("Exception", result.Error.Code);
        Assert.Equal("Test error", result.Error.Message);
    }

    [Fact]
    public void Combine_AllSuccess_ReturnsLastResult()
    {
        var result1 = Result<int>.Success(1);
        var result2 = Result<int>.Success(2);
        var result3 = Result<int>.Success(3);

        var combined = Results.Combine(result1, result2, result3);

        Assert.True(combined.IsSuccess);
        Assert.Equal(3, combined.Value);
    }

    [Fact]
    public void Combine_WithFailure_ReturnsFirstFailure()
    {
        var result1 = Result<int>.Success(1);
        var result2 = Result<int>.Failure("error 2");
        var result3 = Result<int>.Success(3);

        var combined = Results.Combine(result1, result2, result3);

        Assert.True(combined.IsFailure);
        Assert.Equal("error 2", combined.Error.Message);
    }

    [Fact]
    public void Combine_WithNoResults_ReturnsFailure()
    {
        var combined = Results.Combine<int>();

        Assert.True(combined.IsFailure);
        Assert.Equal("No results to combine", combined.Error.Message);
    }

    [Fact]
    public void Combine_NonGeneric_AllSuccess_ReturnsSuccess()
    {
        var result1 = Result.Success();
        var result2 = Result.Success();
        var result3 = Result.Success();

        var combined = Results.Combine(result1, result2, result3);

        Assert.True(combined.IsSuccess);
    }

    [Fact]
    public void Combine_NonGeneric_WithFailure_ReturnsFirstFailure()
    {
        var result1 = Result.Success();
        var result2 = Result.Failure("error 2");
        var result3 = Result.Success();

        var combined = Results.Combine(result1, result2, result3);

        Assert.True(combined.IsFailure);
        Assert.Equal("error 2", combined.Error.Message);
    }

    [Fact]
    public void CombineAll_AllSuccess_ReturnsAllValues()
    {
        var result1 = Result<int>.Success(1);
        var result2 = Result<int>.Success(2);
        var result3 = Result<int>.Success(3);

        var combined = Results.CombineAll(result1, result2, result3);

        Assert.True(combined.IsSuccess);
        Assert.Equal(new[] { 1, 2, 3 }, combined.Value);
    }

    [Fact]
    public void CombineAll_WithFailure_ReturnsFirstFailure()
    {
        var result1 = Result<int>.Success(1);
        var result2 = Result<int>.Failure("error 2");
        var result3 = Result<int>.Success(3);

        var combined = Results.CombineAll(result1, result2, result3);

        Assert.True(combined.IsFailure);
        Assert.Equal("error 2", combined.Error.Message);
    }

    [Fact]
    public void RealWorldExample_ParsingAndValidation()
    {
        var result = Results.Try(() => int.Parse("42"))
            .Ensure(x => x > 0, "Must be positive")
            .Ensure(x => x < 100, "Must be less than 100")
            .Map(x => x * 2);

        Assert.True(result.IsSuccess);
        Assert.Equal(84, result.Value);
    }

    [Fact]
    public void RealWorldExample_ParsingFailure()
    {
        var result = Results.Try(() => int.Parse("invalid"))
            .Ensure(x => x > 0, "Must be positive")
            .Map(x => x * 2);

        Assert.True(result.IsFailure);
        Assert.Equal("Exception", result.Error.Code);
    }

    [Fact]
    public void RealWorldExample_ValidationFailure()
    {
        var result = Results.Try(() => int.Parse("150"))
            .Ensure(x => x > 0, "Must be positive")
            .Ensure(x => x < 100, "Must be less than 100")
            .Map(x => x * 2);

        Assert.True(result.IsFailure);
        Assert.Equal("Must be less than 100", result.Error.Message);
    }

    [Fact]
    public async Task RealWorldExample_AsyncDatabaseOperation()
    {
        async Task<string> FetchUserNameAsync(int userId)
        {
            await Task.Delay(1);
            if (userId <= 0)
                throw new ArgumentException("Invalid user ID");
            return $"User{userId}";
        }

        var result = await Results.TryAsync(() => FetchUserNameAsync(42))
            .MapAsync(name => name.ToUpper())
            .TapAsync(async name =>
            {
                await Task.Delay(1);
            });

        Assert.True(result.IsSuccess);
        Assert.Equal("USER42", result.Value);
    }

    [Fact]
    public async Task RealWorldExample_AsyncOperationFailure()
    {
        async Task<string> FetchUserNameAsync(int userId)
        {
            await Task.Delay(1);
            if (userId <= 0)
                throw new ArgumentException("Invalid user ID");
            return $"User{userId}";
        }

        var result = await Results.TryAsync(() => FetchUserNameAsync(-1));

        Assert.True(result.IsFailure);
        Assert.Equal("Exception", result.Error.Code);
        Assert.Contains("Invalid user ID", result.Error.Message);
    }

    [Fact]
    public void RealWorldExample_MultipleOperationsCombined()
    {
        var validateEmail = (string email) =>
            email.Contains('@')
                ? Result.Success()
                : Result.Failure("Invalid email");

        var validateAge = (int age) =>
            age >= 18
                ? Result.Success()
                : Result.Failure("Must be 18 or older");

        var emailResult = validateEmail("test@example.com");
        var ageResult = validateAge(25);

        var combined = Results.Combine(emailResult, ageResult);

        Assert.True(combined.IsSuccess);
    }

    [Fact]
    public void RealWorldExample_MultipleOperationsWithFailure()
    {
        var validateEmail = (string email) =>
            email.Contains('@')
                ? Result.Success()
                : Result.Failure("Invalid email");

        var validateAge = (int age) =>
            age >= 18
                ? Result.Success()
                : Result.Failure("Must be 18 or older");

        var emailResult = validateEmail("invalid-email");
        var ageResult = validateAge(25);

        var combined = Results.Combine(emailResult, ageResult);

        Assert.True(combined.IsFailure);
        Assert.Equal("Invalid email", combined.Error.Message);
    }
}
