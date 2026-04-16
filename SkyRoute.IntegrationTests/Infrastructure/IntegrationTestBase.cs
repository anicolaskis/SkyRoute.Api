using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SkyRoute.IntegrationTests.Infrastructure;

/// <summary>
/// Base class for integration tests.
/// Provides a pre-configured HttpClient and shared JSON options
/// (enum-as-string, same as the real API) so all deserialization works correctly.
/// </summary>
public abstract class IntegrationTestBase : IClassFixture<SkyRouteWebAppFactory>
{
    protected readonly HttpClient Client;

    protected static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    protected IntegrationTestBase(SkyRouteWebAppFactory factory)
    {
        Client = factory.CreateClient();
    }

    protected static StringContent Json(object payload) =>
        new(JsonSerializer.Serialize(payload, JsonOptions), System.Text.Encoding.UTF8, "application/json");
}
