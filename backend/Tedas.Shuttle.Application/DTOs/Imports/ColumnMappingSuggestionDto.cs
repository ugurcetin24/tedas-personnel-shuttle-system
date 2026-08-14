namespace Tedas.Shuttle.Application.DTOs.Imports;

public sealed record ColumnMappingSuggestionDto(
    string SourceHeader,
    string TargetField,
    double Confidence);
