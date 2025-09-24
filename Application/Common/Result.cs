namespace MoneyTracker.Application.Common;

/// <summary>
/// Patrón Result para manejo explicito de errores sin excepciones
/// </summary>
public class Result
{
    public bool IsSuccess { get; protected set; }
    public bool IsFailure => !IsSuccess;
    public List<string> Errors { get; protected set; } = new();
    public string Error => Errors.FirstOrDefault() ?? string.Empty;

    protected Result(bool isSuccess, List<string> errors)
    {
        IsSuccess = isSuccess;
        Errors = errors ?? new List<string>();
    }

    public static Result Success() => new(true, new List<string>());
    public static Result Failure(string error) => new(false, new List<string> { error });
    public static Result Failure(List<string> errors) => new(false, errors);

    public static Result<T> Success<T>(T data) => new(true, data, new List<string>());
    public static Result<T> Failure<T>(string error) => new(false, default!, new List<string> { error });
    public static Result<T> Failure<T>(List<string> errors) => new(false, default!, errors);
}

/// <summary>
/// Result genérico que incluye datos
/// </summary>
public class Result<T> : Result
{
    public T Data { get; private set; }

    protected internal Result(bool isSuccess, T data, List<string> errors)
        : base(isSuccess, errors)
    {
        Data = data;
    }
}