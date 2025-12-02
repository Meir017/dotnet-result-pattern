namespace ResultNet.Tests;

public class UsageExamplesTests
{
    #region Basic Usage
    
    [Fact]
    public void Example_SimpleValidation()
    {
        Result<int> ValidateAge(int age)
        {
            if (age < 0)
                return Result<int>.Failure("Age cannot be negative");
            if (age > 150)
                return Result<int>.Failure("Age cannot exceed 150");
            return Result<int>.Success(age);
        }

        var validAge = ValidateAge(25);
        Assert.True(validAge.IsSuccess);
        Assert.Equal(25, validAge.Value);

        var invalidAge = ValidateAge(-5);
        Assert.True(invalidAge.IsFailure);
        Assert.Equal("Age cannot be negative", invalidAge.Error.Message);
    }

    [Fact]
    public void Example_ImplicitConversions()
    {
        Result<int> GetUserId(string username)
        {
            if (string.IsNullOrEmpty(username))
                return new Error("Validation", "Username is required");
            
            if (username == "admin")
                return 1;
            
            return new Error("NotFound", "User not found");
        }

        var adminResult = GetUserId("admin");
        Assert.True(adminResult.IsSuccess);
        Assert.Equal(1, adminResult.Value);

        var notFoundResult = GetUserId("unknown");
        Assert.True(notFoundResult.IsFailure);
        Assert.Equal("NotFound", notFoundResult.Error.Code);
    }

    [Fact]
    public void Example_MatchingPatterns()
    {
        Result<int> Divide(int numerator, int denominator)
        {
            if (denominator == 0)
                return Result<int>.Failure("Division by zero");
            return Result<int>.Success(numerator / denominator);
        }

        var successMessage = Divide(10, 2).Match(
            onSuccess: value => $"Result: {value}",
            onFailure: error => $"Error: {error.Message}");

        var failureMessage = Divide(10, 0).Match(
            onSuccess: value => $"Result: {value}",
            onFailure: error => $"Error: {error.Message}");

        Assert.Equal("Result: 5", successMessage);
        Assert.Equal("Error: Division by zero", failureMessage);
    }

    #endregion

    #region Functional Composition

    [Fact]
    public void Example_ChainingOperations()
    {
        var result = Result<string>.Success("  hello world  ")
            .Map(s => s.Trim())
            .Map(s => s.ToUpper())
            .Ensure(s => s.Length > 0, "String cannot be empty")
            .Map(s => $"Message: {s}");

        Assert.True(result.IsSuccess);
        Assert.Equal("Message: HELLO WORLD", result.Value);
    }

    [Fact]
    public void Example_ErrorPropagation()
    {
        var result = Result<string>.Success("test")
            .Map(s => s.ToUpper())
            .Bind(s => Result<int>.Failure("Something went wrong"))
            .Map(i => i * 2);

        Assert.True(result.IsFailure);
        Assert.Equal("Something went wrong", result.Error.Message);
    }

    [Fact]
    public void Example_RailwayOrientedProgramming()
    {
        Result<string> ValidateInput(string input) =>
            string.IsNullOrWhiteSpace(input)
                ? Result<string>.Failure("Input is required")
                : Result<string>.Success(input);

        Result<string> Normalize(string input) =>
            Result<string>.Success(input.Trim().ToLower());

        Result<int> ConvertToNumber(string input) =>
            Results.Try(() => int.Parse(input));

        Result<int> EnsurePositive(int number) =>
            number > 0
                ? Result<int>.Success(number)
                : Result<int>.Failure("Number must be positive");

        var successPipeline = ValidateInput("  42  ")
            .Bind(Normalize)
            .Bind(ConvertToNumber)
            .Bind(EnsurePositive);

        Assert.True(successPipeline.IsSuccess);
        Assert.Equal(42, successPipeline.Value);

        var failurePipeline = ValidateInput("  -5  ")
            .Bind(Normalize)
            .Bind(ConvertToNumber)
            .Bind(EnsurePositive);

        Assert.True(failurePipeline.IsFailure);
        Assert.Equal("Number must be positive", failurePipeline.Error.Message);
    }

    #endregion

    #region Exception Handling

    [Fact]
    public void Example_ExceptionHandling()
    {
        var successResult = Results.Try(() => int.Parse("42"));
        Assert.True(successResult.IsSuccess);
        Assert.Equal(42, successResult.Value);

        var failureResult = Results.Try(() => int.Parse("not a number"));
        Assert.True(failureResult.IsFailure);
        Assert.Equal("Exception", failureResult.Error.Code);
    }

    [Fact]
    public void Example_CustomErrorMapping()
    {
        var result = Results.Try(
            () => int.Parse("invalid"),
            ex => new Error("ParseError", $"Failed to parse: {ex.Message}"));

        Assert.True(result.IsFailure);
        Assert.Equal("ParseError", result.Error.Code);
        Assert.Contains("Failed to parse", result.Error.Message);
    }

    #endregion

    #region Side Effects

    [Fact]
    public void Example_DefaultValues()
    {
        var successResult = Result<int>.Success(42);
        var failureResult = Result<int>.Failure("error");

        var value1 = successResult.ValueOr(0);
        var value2 = failureResult.ValueOr(0);

        Assert.Equal(42, value1);
        Assert.Equal(0, value2);
    }

    [Fact]
    public void Example_SideEffects()
    {
        var logMessages = new List<string>();

        Result<int>.Success(42)
            .Tap(value => logMessages.Add($"Processing: {value}"))
            .Map(x => x * 2)
            .Tap(value => logMessages.Add($"Result: {value}"));

        Assert.Equal(2, logMessages.Count);
        Assert.Equal("Processing: 42", logMessages[0]);
        Assert.Equal("Result: 84", logMessages[1]);
    }

    #endregion

    #region Combining Results

    [Fact]
    public void Example_CombiningValidations()
    {
        Result ValidateUsername(string username) =>
            string.IsNullOrWhiteSpace(username)
                ? Result.Failure("Username is required")
                : Result.Success();

        Result ValidatePassword(string password) =>
            password.Length < 8
                ? Result.Failure("Password must be at least 8 characters")
                : Result.Success();

        Result ValidateEmail(string email) =>
            !email.Contains('@')
                ? Result.Failure("Invalid email format")
                : Result.Success();

        var allValid = Results.Combine(
            ValidateUsername("john"),
            ValidatePassword("password123"),
            ValidateEmail("john@example.com"));

        Assert.True(allValid.IsSuccess);

        var hasFailure = Results.Combine(
            ValidateUsername("john"),
            ValidatePassword("short"),
            ValidateEmail("john@example.com"));

        Assert.True(hasFailure.IsFailure);
        Assert.Equal("Password must be at least 8 characters", hasFailure.Error.Message);
    }

    [Fact]
    public void Example_CollectingMultipleValues()
    {
        Result<int> GetUserId() => 1;
        Result<int> GetDepartmentId() => 2;
        Result<int> GetRoleId() => 3;

        var combined = Results.CombineAll(
            GetUserId(),
            GetDepartmentId(),
            GetRoleId());

        Assert.True(combined.IsSuccess);
        Assert.Equal(new[] { 1, 2, 3 }, combined.Value);
    }

    #endregion

    #region Async Support

    [Fact]
    public async Task Example_AsyncOperations()
    {
        async Task<int> FetchUserIdAsync(string username)
        {
            await Task.Delay(1);
            return username == "admin" ? 1 : throw new Exception("User not found");
        }

        var result = await Results.TryAsync(() => FetchUserIdAsync("admin"))
            .MapAsync(id => id * 10)
            .TapAsync(async id => await Task.Delay(1));

        Assert.True(result.IsSuccess);
        Assert.Equal(10, result.Value);
    }

    [Fact]
    public async Task Example_AsyncPipeline()
    {
        async Task<string> LoadDataAsync() => await Task.FromResult("raw data");
        async Task<string> ProcessDataAsync(string data) => await Task.FromResult(data.ToUpper());
        async Task SaveDataAsync(string data) => await Task.Delay(1);

        var result = await Results.TryAsync(LoadDataAsync)
            .BindAsync(async data => 
            {
                var processed = await ProcessDataAsync(data);
                return Result<string>.Success(processed);
            })
            .TapAsync(SaveDataAsync);

        Assert.True(result.IsSuccess);
        Assert.Equal("RAW DATA", result.Value);
    }

    #endregion

    #region Typed Error Codes

    public enum UserErrorCode
    {
        None = 0,
        NotFound,
        InvalidCredentials,
        Unauthorized,
        ValidationFailed
    }

    [Fact]
    public void Example_UsingEnumErrorCodes()
    {
        Result<string, UserErrorCode> AuthenticateUser(string username, string password)
        {
            if (string.IsNullOrEmpty(username))
                return Result<string, UserErrorCode>.Failure(
                    UserErrorCode.ValidationFailed, 
                    "Username is required");
            
            if (username != "admin")
                return Result<string, UserErrorCode>.Failure(
                    UserErrorCode.NotFound, 
                    "User not found");
            
            if (password != "secret")
                return Result<string, UserErrorCode>.Failure(
                    UserErrorCode.InvalidCredentials, 
                    "Invalid password");
            
            return Result<string, UserErrorCode>.Success("token-12345");
        }

        var successResult = AuthenticateUser("admin", "secret");
        Assert.True(successResult.IsSuccess);
        Assert.Equal("token-12345", successResult.Value);

        var notFoundResult = AuthenticateUser("john", "secret");
        Assert.True(notFoundResult.IsFailure);
        Assert.Equal(UserErrorCode.NotFound, notFoundResult.Error.Code);
        Assert.Equal("User not found", notFoundResult.Error.Message);
    }

    [Fact]
    public void Example_TypedErrorCodesWithImplicitConversion()
    {
        Result<int, UserErrorCode> GetUserAge(string username)
        {
            if (string.IsNullOrEmpty(username))
                return Result<int, UserErrorCode>.Failure("Username is required");
            
            if (username == "john")
                return 30;
            
            var error = new Error<UserErrorCode>(UserErrorCode.NotFound, "User not found");
            return error;
        }

        var successResult = GetUserAge("john");
        Assert.True(successResult.IsSuccess);
        Assert.Equal(30, successResult.Value);

        var failureResult = GetUserAge("unknown");
        Assert.True(failureResult.IsFailure);
        Assert.Equal(UserErrorCode.NotFound, failureResult.Error.Code);
    }

    [Fact]
    public void Example_TypedErrorCodesWithExceptionHandling()
    {
        var successResult = Results.Try<int, UserErrorCode>(
            () => int.Parse("42"),
            ex => new Error<UserErrorCode>(
                UserErrorCode.ValidationFailed, 
                $"Parse error: {ex.Message}"));
        
        Assert.True(successResult.IsSuccess);
        Assert.Equal(42, successResult.Value);

        var failureResult = Results.Try<int, UserErrorCode>(
            () => int.Parse("invalid"),
            ex => new Error<UserErrorCode>(
                UserErrorCode.ValidationFailed, 
                $"Parse error: {ex.Message}"));
        
        Assert.True(failureResult.IsFailure);
        Assert.Equal(UserErrorCode.ValidationFailed, failureResult.Error.Code);
    }

    #endregion

    #region Advanced Patterns

    [Fact]
    public void Example_EarlyReturnPattern()
    {
        Result<string> ProcessOrder(int orderId)
        {
            var orderResult = GetOrder(orderId);
            if (orderResult.IsFailure)
                return orderResult.Error;

            var validationResult = ValidateOrder(orderResult.Value);
            if (validationResult.IsFailure)
                return validationResult.Error;

            return Result<string>.Success($"Order {orderId} processed");
        }

        Result<string> GetOrder(int id) =>
            id > 0 ? Result<string>.Success($"Order{id}") : Result<string>.Failure("Invalid order ID");

        Result ValidateOrder(string order) =>
            order.Contains("Order") ? Result.Success() : Result.Failure("Invalid order format");

        var result = ProcessOrder(123);
        Assert.True(result.IsSuccess);
        Assert.Equal("Order 123 processed", result.Value);

        var invalidResult = ProcessOrder(-1);
        Assert.True(invalidResult.IsFailure);
        Assert.Equal("Invalid order ID", invalidResult.Error.Message);
    }

    #endregion
}
