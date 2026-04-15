namespace SkyRoute.Domain.Models;

/// <summary>
/// Supported currencies. Using an enum prevents magic strings like "USD" scattered across layers.
/// Add new currencies here as the platform expands internationally.
/// </summary>
public enum Currency
{
    USD
}
