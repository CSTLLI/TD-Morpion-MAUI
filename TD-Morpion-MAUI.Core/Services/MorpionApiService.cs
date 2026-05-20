using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using TD_Morpion_MAUI.Api;
using TD_Morpion_MAUI.Models;

namespace TD_Morpion_MAUI.Services;

/// <summary>
/// Implémentation HTTP de <see cref="IMorpionApiService"/> au-dessus d'un <see cref="HttpClient"/>.
/// Le <see cref="HttpClient.BaseAddress"/> doit pointer sur l'API (ex. http://localhost:5069).
/// </summary>
public class MorpionApiService : IMorpionApiService
{
    /// <summary>URL par défaut de l'API en local (profil "http" de launchSettings).</summary>
    public const string DefaultBaseUrl = "http://localhost:5069";

    private readonly HttpClient _http;

    // L'API ASP.NET sérialise en camelCase et les enums en chaînes : on s'aligne.
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public MorpionApiService(HttpClient http) => _http = http;

    public async Task<GameDto> CreateGameAsync(GameMode mode, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("/games", new CreateGameRequest(mode), JsonOptions, ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<GameDto>(JsonOptions, ct))!;
    }

    public async Task<GameDto?> GetGameAsync(Guid id, CancellationToken ct = default)
    {
        var response = await _http.GetAsync($"/games/{id}", ct);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<GameDto>(JsonOptions, ct);
    }

    public async Task<(GameDto? Game, string? Error)> PlayMoveAsync(Guid id, int position, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync($"/games/{id}/moves", new MoveRequest(position), JsonOptions, ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return (null, "Partie introuvable");

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var problem = await response.Content.ReadFromJsonAsync<ErrorResponse>(JsonOptions, ct);
            return (null, problem?.Error ?? "Coup invalide");
        }

        response.EnsureSuccessStatusCode();
        var game = await response.Content.ReadFromJsonAsync<GameDto>(JsonOptions, ct);
        return (game, null);
    }

    public async Task<IReadOnlyList<GameHistoryEntry>> GetHistoryAsync(CancellationToken ct = default)
    {
        var history = await _http.GetFromJsonAsync<List<GameHistoryEntry>>("/history", JsonOptions, ct);
        return history ?? new List<GameHistoryEntry>();
    }

    private record ErrorResponse(string Error);
}
