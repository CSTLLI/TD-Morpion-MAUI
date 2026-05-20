using TD_Morpion_MAUI.Api;
using TD_Morpion_MAUI.Models;
using TD_Morpion_MAUI.Services;

namespace TD_Morpion_MAUI.Tests.Fakes;

/// <summary>
/// Faux client d'API, entièrement contrôlable par les tests : on règle ce que renvoient
/// CreateGame / PlayMove / History, et on inspecte ce que le ViewModel a envoyé.
/// </summary>
public class FakeMorpionApiService : IMorpionApiService
{
    public Guid GameId { get; } = Guid.NewGuid();

    // Inspection
    public GameMode? LastCreatedMode { get; private set; }
    public int? LastPlayedPosition { get; private set; }
    public int PlayMoveCalls { get; private set; }

    // Contrôle des réponses
    public GameDto CreateResult { get; set; }
    public Func<int, (GameDto? Game, string? Error)>? MoveHandler { get; set; }
    public List<GameHistoryEntry> HistoryList { get; } = new();

    public FakeMorpionApiService()
    {
        CreateResult = new GameDto(GameId, GameMode.HumanVsBot, ".........", 'X', GameStatus.InProgress);
    }

    public Task<GameDto> CreateGameAsync(GameMode mode, CancellationToken ct = default)
    {
        LastCreatedMode = mode;
        return Task.FromResult(CreateResult);
    }

    public Task<GameDto?> GetGameAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult<GameDto?>(CreateResult);

    public Task<(GameDto? Game, string? Error)> PlayMoveAsync(Guid id, int position, CancellationToken ct = default)
    {
        PlayMoveCalls++;
        LastPlayedPosition = position;
        var result = MoveHandler?.Invoke(position) ?? (CreateResult, null);
        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<GameHistoryEntry>> GetHistoryAsync(CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<GameHistoryEntry>>(HistoryList);
}
