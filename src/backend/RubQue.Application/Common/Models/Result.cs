namespace RubQue.Application.Common.Models;

public class Result<T>
{
    public bool Succeeded { get; private set; }
    public T? Data { get; private set; }
    public string? Message { get; private set; }
    public string[] Errors { get; private set; } = Array.Empty<string>();

    public static Result<T> Success(T data, string? message = null) =>
        new() { Succeeded = true, Data = data, Message = message };

    public static Result<T> Failure(string error) =>
        new() { Succeeded = false, Errors = new[] { error } };

    public static Result<T> Failure(IEnumerable<string> errors, string? message = null) =>
        new() { Succeeded = false, Errors = errors.ToArray(), Message = message };
}

public class Result
{
    public bool Succeeded { get; private set; }
    public string? Message { get; private set; }
    public string[] Errors { get; private set; } = Array.Empty<string>();

    public static Result Success(string? message = null) =>
        new() { Succeeded = true, Message = message };

    public static Result Failure(string error) =>
        new() { Succeeded = false, Errors = new[] { error } };

    public static Result Failure(IEnumerable<string> errors, string? message = null) =>
        new() { Succeeded = false, Errors = errors.ToArray(), Message = message };
}
