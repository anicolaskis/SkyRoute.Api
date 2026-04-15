using System.Text.Json;

namespace SkyRoute.Infrastructure.Mocks;

// Default implementation of IMockFlightDataSource:
// reads templates from a JSON file on disk.
// Advantage: changing test data does not require recompilation.
// Used in the exercise production to feed GlobalAir and BudgetWings.
public class JsonMockFlightDataSource : IMockFlightDataSource
{
    private readonly List<FlightTemplate> _templates;

    public string ProviderName { get; }

    public JsonMockFlightDataSource(string providerName, string jsonFilePath)
    {
        ProviderName = providerName;

        if (!File.Exists(jsonFilePath))
            throw new FileNotFoundException($"Mock data file not found: {jsonFilePath}");

        var json = File.ReadAllText(jsonFilePath);
        _templates = JsonSerializer.Deserialize<List<FlightTemplate>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new List<FlightTemplate>();
    }

    public IEnumerable<FlightTemplate> GetTemplates() => _templates;
}
