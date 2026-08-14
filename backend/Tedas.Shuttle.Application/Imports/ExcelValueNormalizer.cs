using System.Globalization;
using System.Text.RegularExpressions;

namespace Tedas.Shuttle.Application.Imports;

public static partial class ExcelValueNormalizer
{
    private static readonly CultureInfo TurkishCulture = new("tr-TR");

    public static string? NullIfWhiteSpace(string? value)
    {
        var normalized = NormalizeWhitespace(value);
        return normalized.Length == 0 ? null : normalized;
    }

    public static string NormalizeWhitespace(string? value)
    {
        return string.Join(' ', (value ?? string.Empty).Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    public static string NormalizeRegistrationNumber(string? value)
    {
        return NormalizeWhitespace(value).ToUpperInvariant();
    }

    public static string NormalizeCode(string? value)
    {
        return NormalizeWhitespace(value).ToUpperInvariant();
    }

    public static string? NormalizePhone(string? value)
    {
        var normalized = NullIfWhiteSpace(value);
        if (normalized is null)
        {
            return null;
        }

        return NonPhoneCharacterRegex().Replace(normalized, string.Empty);
    }

    public static bool TryParseInteger(string? value, out int result)
    {
        return int.TryParse(
            NormalizeWhitespace(value),
            NumberStyles.Integer,
            TurkishCulture,
            out result);
    }

    public static bool TryParseDecimal(string? value, out decimal result)
    {
        var normalized = NormalizeWhitespace(value);

        return decimal.TryParse(normalized, NumberStyles.Number, TurkishCulture, out result)
            || decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out result);
    }

    public static bool TryParseTime(string? value, out TimeOnly result)
    {
        var normalized = NormalizeWhitespace(value);

        return TimeOnly.TryParse(normalized, TurkishCulture, out result)
            || TimeOnly.TryParse(normalized, CultureInfo.InvariantCulture, out result);
    }

    [GeneratedRegex("[^0-9+]")]
    private static partial Regex NonPhoneCharacterRegex();
}
