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

## {BASIC_USAGE}

## {FUNCTIONAL_COMPOSITION}

## {EXCEPTION_HANDLING}

## {SIDE_EFFECTS}

## {COMBINING_RESULTS}

## {ASYNC_SUPPORT}

## {TYPED_ERROR_CODES}

## {ADVANCED_PATTERNS}

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
