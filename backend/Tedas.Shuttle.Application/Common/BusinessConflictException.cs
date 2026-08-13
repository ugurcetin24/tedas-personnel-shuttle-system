namespace Tedas.Shuttle.Application.Common;

public sealed class BusinessConflictException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
