namespace ResultNet;

public readonly record struct Error(string Code, string Message)
{
    public static implicit operator Error(string message) => new("Error", message);
}

public readonly record struct Error<TCode>(TCode Code, string Message) where TCode : struct, Enum
{
    public static implicit operator Error<TCode>(string message)
        => new(default, message);
}
