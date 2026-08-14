using System.Globalization;
using System.Text;
using Tedas.Shuttle.Application.DTOs.Imports;

namespace Tedas.Shuttle.Application.Imports;

public static class ExcelColumnMappingSuggester
{
    public static IReadOnlyList<ColumnMappingSuggestionDto> Suggest(
        IReadOnlyList<string> headers,
        IReadOnlyList<ExcelColumnMappingProfile> profiles)
    {
        return headers
            .Select(header => SuggestHeader(header, profiles))
            .Where(suggestion => suggestion is not null)
            .Select(suggestion => suggestion!)
            .ToArray();
    }

    private static ColumnMappingSuggestionDto? SuggestHeader(
        string header,
        IReadOnlyList<ExcelColumnMappingProfile> profiles)
    {
        var normalizedHeader = NormalizeKey(header);
        if (normalizedHeader.Length == 0)
        {
            return null;
        }

        foreach (var profile in profiles)
        {
            if (profile.Aliases.Any(alias => NormalizeKey(alias) == normalizedHeader))
            {
                return new ColumnMappingSuggestionDto(header, profile.TargetField, 1);
            }
        }

        foreach (var profile in profiles)
        {
            if (profile.Aliases.Any(alias => normalizedHeader.Contains(NormalizeKey(alias), StringComparison.Ordinal)))
            {
                return new ColumnMappingSuggestionDto(header, profile.TargetField, 0.8);
            }
        }

        return null;
    }

    public static string NormalizeKey(string value)
    {
        var normalized = value.Trim().ToLower(new CultureInfo("tr-TR")).Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var character in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(character);
            if (category == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsLetterOrDigit(character))
            {
                builder.Append(character);
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }
}
