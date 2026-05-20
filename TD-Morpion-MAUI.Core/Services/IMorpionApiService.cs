using TD_Morpion_MAUI.Api;
using TD_Morpion_MAUI.Models;

namespace TD_Morpion_MAUI.Services;

/// <summary>Client de l'API Morpion (MorpionAPI-csharp).</summary>
public interface IMorpionApiService
{
    /// <summary>POST /games — crée une partie et renvoie son état initial.</summary>
    Task<GameDto> CreateGameAsync(GameMode mode, CancellationToken ct = default);

    /// <summary>GET /games/{id} — renvoie l'état d'une partie, ou null si introuvable (404).</summary>
    Task<GameDto?> GetGameAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// POST /games/{id}/moves — joue le coup à <paramref name="position"/> (1-9).
    /// Renvoie l'état mis à jour, ou un message d'erreur si le coup est refusé (400).
    /// </summary>
    Task<(GameDto? Game, string? Error)> PlayMoveAsync(Guid id, int position, CancellationToken ct = default);

    /// <summary>GET /history — renvoie l'historique des parties terminées.</summary>
    Task<IReadOnlyList<GameHistoryEntry>> GetHistoryAsync(CancellationToken ct = default);
}
