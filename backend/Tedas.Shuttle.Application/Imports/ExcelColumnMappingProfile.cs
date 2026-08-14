namespace Tedas.Shuttle.Application.Imports;

public sealed record ExcelColumnMappingProfile(
    string TargetField,
    IReadOnlySet<string> Aliases);
