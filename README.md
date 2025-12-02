# ResultNet

An opinionated, lightweight Result pattern library for .NET that embraces functional programming principles and provides comprehensive async/await support.

## Features

- 🎯 **Simple & Intuitive**: Clean API that's easy to understand and use
- ⚡ **Async-First**: Full support for async/await patterns
- 🔗 **Functional Composition**: Map, Bind, Ensure, and more
- 🛡️ **Type-Safe**: Strongly typed errors with Result<T> and Result
- 🚀 **Zero Dependencies**: Lightweight library with no external dependencies
- 📦 **Well Tested**: Comprehensive test suite with 155+ tests

## Installation

```bash
dotnet add package ResultNet
```

## Quick Start

```csharp
using ResultNet;

// Create results
var success = Result<int>.Success(42);
var failure = Result<int>.Failure("Something went wrong");

// Use implicit conversions
Result<int> result = 42;  // Success
Result<int> error = new Error("Code", "Message");  // Failure

// Pattern matching
var message = result.Match(
    onSuccess: value => $"Got: {value}",
    onFailure: error => $"Error: {error.Message}"
);
```

## Core Concepts

### Result<T>

Represents an operation that returns a value of type `T` or an error.

```csharp
public readonly struct Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure { get; }
    public T Value { get; }  // Throws if IsFailure
    public Error Error { get; }  // Throws if IsSuccess
}
```

### Result

Represents an operation that succeeds or fails without returning a value.

```csharp
public readonly struct Result
{
    public bool IsSuccess { get; }
    public bool IsFailure { get; }
    public Error Error { get; }  // Throws if IsSuccess
}
```

### Error

Represents an error with a code and message.

```csharp
public readonly record struct Error(string Code, string Message);
```

### Result<T, TCode> and Error<TCode>

For type-safe error codes using enums:

```csharp
public enum ErrorCode
{
    None = 0,
    NotFound,
    ValidationFailed,
    Unauthorized
}

public readonly record struct Error<TCode>(TCode Code, string Message) 
    where TCode : struct, Enum;

public readonly struct Result<T, TCode> where TCode : struct, Enum
{
    public bool IsSuccess { get; }
    public bool IsFailure { get; }
    public T Value { get; }  // Throws if IsFailure
    public Error<TCode> Error { get; }  // Throws if IsSuccess
}
```

## Usage Examples

## ### Simple Validation

```csharp
Result<int> ValidateAge(int age)
{
    if (age < 0)
        return Result<int>.Failure("Age cannot be negative");
    if (age > 150)
        return Result<int>.Failure("Age cannot exceed 150");
    return Result<int>.Success(age);
}
var validAge = ValidateAge(25);
var invalidAge = ValidateAge(-5);
```

### Implicit Conversions

```csharp
Result<int> GetUserId(string username)
{
    if (string.IsNullOrEmpty(username))
        return new Error("Validation", "Username is required");
    if (username == "admin")
        return 1;
    return new Error("NotFound", "User not found");
}
var adminResult = GetUserId("admin");
var notFoundResult = GetUserId("unknown");
```

### Matching Patterns

```csharp
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
```



## ### Chaining Operations

```csharp
var result = Result<string>.Success("  hello world  ")
    .Map(s => s.Trim())
    .Map(s => s.ToUpper())
    .Ensure(s => s.Length > 0, "String cannot be empty")
    .Map(s => $"Message: {s}");
```

### Error Propagation

```csharp
var result = Result<string>.Success("test")
    .Map(s => s.ToUpper())
    .Bind(s => Result<int>.Failure("Something went wrong"))
    .Map(i => i * 2);
```

### Railway Oriented Programming

```csharp
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
var failurePipeline = ValidateInput("  -5  ")
    .Bind(Normalize)
    .Bind(ConvertToNumber)
    .Bind(EnsurePositive);
```



## ### Exception Handling

```csharp
var successResult = Results.Try(() => int.Parse("42"));
var failureResult = Results.Try(() => int.Parse("not a number"));
```

### Custom Error Mapping

```csharp
var result = Results.Try(
    () => int.Parse("invalid"),
    ex => new Error("ParseError", $"Failed to parse: {ex.Message}"));
```



## ### Default Values

```csharp
var successResult = Result<int>.Success(42);
var failureResult = Result<int>.Failure("error");
var value1 = successResult.ValueOr(0);
var value2 = failureResult.ValueOr(0);
```

### Side Effects

```csharp
var logMessages = new List<string>();
Result<int>.Success(42)
    .Tap(value => logMessages.Add($"Processing: {value}"))
    .Map(x => x * 2)
    .Tap(value => logMessages.Add($"Result: {value}"));
```



## ### Combining Validations

```csharp
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
var hasFailure = Results.Combine(
    ValidateUsername("john"),
    ValidatePassword("short"),
    ValidateEmail("john@example.com"));
```

### Collecting Multiple Values

```csharp
Result<int> GetUserId() => 1;
Result<int> GetDepartmentId() => 2;
Result<int> GetRoleId() => 3;
var combined = Results.CombineAll(
    GetUserId(),
    GetDepartmentId(),
    GetRoleId());
```



## ### Async Operations

```csharp
async Task<int> FetchUserIdAsync(string username)
{
    await Task.Delay(1);
    return username == "admin" ? 1 : throw new Exception("User not found");
}
var result = await Results.TryAsync(() => FetchUserIdAsync("admin"))
    .MapAsync(id => id * 10)
    .TapAsync(async id => await Task.Delay(1));
```

### Async Pipeline

```csharp
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
```



## ### Using Enum Error Codes

```csharp
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
var notFoundResult = AuthenticateUser("john", "secret");
```

### Typed Error Codes With Implicit Conversion

```csharp
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
var failureResult = GetUserAge("unknown");
```

### Typed Error Codes With Exception Handling

```csharp
var successResult = Results.Try<int, UserErrorCode>(
    () => int.Parse("42"),
    ex => new Error<UserErrorCode>(
        UserErrorCode.ValidationFailed, 
        $"Parse error: {ex.Message}"));
var failureResult = Results.Try<int, UserErrorCode>(
    () => int.Parse("invalid"),
    ex => new Error<UserErrorCode>(
        UserErrorCode.ValidationFailed, 
        $"Parse error: {ex.Message}"));
```



## ### Early Return Pattern

```csharp
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
var invalidResult = ProcessOrder(-1);
```



## API Reference

### Result<T> Methods

- `Success(T value)` - Creates a successful result
- `Failure(Error error)` - Creates a failed result
- `Failure(string message)` - Creates a failed result with default error code
- `Match<TResult>(Func<T, TResult> onSuccess, Func<Error, TResult> onFailure)` - Pattern matching

### Result<T, TCode> Methods

- `Success(T value)` - Creates a successful result
- `Failure(Error<TCode> error)` - Creates a failed result
- `Failure(TCode code, string message)` - Creates a failed result with specific error code
- `Failure(string message)` - Creates a failed result with default error code
- `Match<TResult>(Func<T, TResult> onSuccess, Func<Error<TCode>, TResult> onFailure)` - Pattern matching

### Extension Methods

#### Synchronous

- `Map<TOut>(Func<T, TOut> mapper)` - Transform the success value
- `Bind<TOut>(Func<T, Result<TOut>> binder)` - Chain result-returning operations
- `Ensure(Func<T, bool> predicate, Error error)` - Validate the value
- `Tap(Action<T> action)` - Execute side effects on success
- `TapError(Action<Error> action)` - Execute side effects on failure
- `OnSuccess(Action<T> action)` - Execute action on success
- `OnFailure(Action<Error> action)` - Execute action on failure
- `ValueOr(T defaultValue)` - Get value or default

#### Asynchronous

- `MapAsync<TOut>(Func<T, Task<TOut>> mapper)` - Transform with async function
- `BindAsync<TOut>(Func<T, Task<Result<TOut>>> binder)` - Chain async operations
- `EnsureAsync(Func<T, Task<bool>> predicate, Error error)` - Async validation
- `TapAsync(Func<T, Task> action)` - Async side effects
- `TapErrorAsync(Func<Error, Task> action)` - Async error handling
- `OnSuccessAsync(Func<T, Task> action)` - Async success callback
- `OnFailureAsync(Func<Error, Task> action)` - Async failure callback

All async methods also have overloads that work on `Task<Result<T>>`.

### Results Static Methods

- `Try<T>(Func<T> func, Func<Exception, Error>? errorMapper = null)` - Wrap exception-throwing code
- `TryAsync<T>(Func<Task<T>> func, Func<Exception, Error>? errorMapper = null)` - Async exception handling
- `Combine(params Result[] results)` - Combine multiple results (returns first failure)
- `CombineAll<T>(params Result<T>[] results)` - Combine and collect all values

#### Typed Error Code Overloads

- `Try<T, TCode>(Func<T> func, Func<Exception, Error<TCode>>? errorMapper = null)` - Wrap exception-throwing code with typed errors
- `TryAsync<T, TCode>(Func<Task<T>> func, Func<Exception, Error<TCode>>? errorMapper = null)` - Async exception handling with typed errors
- `Combine<T, TCode>(params Result<T, TCode>[] results)` - Combine multiple results with typed errors
- `CombineAll<T, TCode>(params Result<T, TCode>[] results)` - Combine and collect all values with typed errors

## Philosophy

ResultNet is **opinionated** because it:

1. **Prefers explicit over implicit**: Errors are values, not exceptions
2. **Encourages railway-oriented programming**: Operations naturally chain and short-circuit
3. **Embraces async/await**: First-class support for modern async patterns
4. **Values simplicity**: Minimal API surface, maximum expressiveness
5. **Discourages exception-driven control flow**: Use `Results.Try()` to convert exceptions to results

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

MIT License - see LICENSE file for details

## Acknowledgments

Inspired by functional programming patterns and the railway-oriented programming concept by Scott Wlaschin.
