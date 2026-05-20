namespace TD_Morpion_MAUI.Api;

/// <summary>Mode de jeu, identique à l'enum côté API.</summary>
public enum GameMode
{
    HumanVsHuman,
    HumanVsBot
}

/// <summary>État de la partie, identique à l'enum côté API.</summary>
public enum GameStatus
{
    InProgress,
    XWon,
    OWon,
    Draw
}

/// <summary>Corps de POST /games.</summary>
public record CreateGameRequest(GameMode Mode);

/// <summary>Corps de POST /games/{id}/moves (position 1-9).</summary>
public record MoveRequest(int Position);

/// <summary>
/// Réponse de l'API pour une partie.
/// <c>Board</c> est une chaîne de 9 caractères ('.' = vide, 'X' / 'O'), en ligne par ligne.
/// </summary>
public record GameDto(
    Guid Id,
    GameMode Mode,
    string Board,
    char CurrentPlayer,
    GameStatus Status);
