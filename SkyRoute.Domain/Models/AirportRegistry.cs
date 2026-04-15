namespace SkyRoute.Domain.Models;

/// <summary>
/// Maps IATA airport codes to their country codes.
/// Replaces the fragile substring hack (Origin[..2]) used for domestic vs international detection.
/// Extend this dictionary as new airports are onboarded to the platform.
/// </summary>
public static class AirportRegistry
{
    private static readonly Dictionary<string, string> _countryByAirport = new(StringComparer.OrdinalIgnoreCase)
    {
        // United States
        { "JFK", "US" },
        { "LAX", "US" },
        { "ORD", "US" },
        { "MIA", "US" },
        // Argentina
        { "EZE", "AR" },
        { "AEP", "AR" },
        { "COR", "AR" },
        // United Kingdom
        { "LHR", "GB" },
        { "LGW", "GB" },
        // Spain
        { "MAD", "ES" },
        { "BCN", "ES" },
        // Brazil
        { "GRU", "BR" },
        { "GIG", "BR" },
        // Mexico
        { "MEX", "MX" },
        { "CUN", "MX" },
    };

    /// <summary>Returns the country code for a given IATA airport code, or null if unknown.</summary>
    public static string? GetCountry(string iataCode) =>
        _countryByAirport.TryGetValue(iataCode, out var country) ? country : null;

    /// <summary>Returns true if origin and destination are in different countries.</summary>
    public static bool IsInternational(string origin, string destination)
    {
        var originCountry = GetCountry(origin);
        var destCountry = GetCountry(destination);

        // If either airport is unknown we conservatively treat it as international
        // to require a passport (stricter is safer).
        if (originCountry is null || destCountry is null) return true;

        return !string.Equals(originCountry, destCountry, StringComparison.OrdinalIgnoreCase);
    }
}
