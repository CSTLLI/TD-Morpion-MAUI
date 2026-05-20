using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TD_Morpion_MAUI.Api;
using TD_Morpion_MAUI.Models;
using TD_Morpion_MAUI.Services;

namespace TD_Morpion_MAUI.ViewModels;

/// <summary>
/// ViewModel du plateau. Toute la logique de jeu vit côté API : on crée une partie,
/// on envoie les coups, et on affiche le plateau / le statut renvoyés par le serveur
/// (en mode HumanVsBot, le serveur joue le bot automatiquement).
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private static readonly Color EmptyColor = Color.FromArgb("#333333");
    private static readonly Color PlayerColor = Color.FromArgb("#1565C0"); // X (vous)
    private static readonly Color BotColor = Color.FromArgb("#C62828");    // O (bot)
    private static readonly Color IdleColor = Color.FromArgb("#512BD4");

    private readonly IMorpionApiService _api;

    private Guid _gameId;
    private GameStatus _status = GameStatus.InProgress;
    private bool _busy;

    [ObservableProperty]
    private string _statusText = "Connexion à l'API…";

    [ObservableProperty]
    private Color _statusColor = Color.FromArgb("#512BD4");

    [ObservableProperty]
    private string _scoreText = "V: 0  |  D: 0  |  N: 0";

    public ObservableCollection<CellViewModel> Cells { get; } = new(
        Enumerable.Range(0, 9).Select(_ => new CellViewModel())
    );

    public ObservableCollection<GameHistoryEntry> GameHistory { get; } = new();

    public event Action<string, string>? AlertRequested;

    public MainViewModel(IMorpionApiService api)
    {
        _api = api;
    }

    /// <summary>Crée une partie HumanVsBot et charge l'historique. À appeler à l'ouverture de la page.</summary>
    [RelayCommand]
    private async Task InitializeAsync()
    {
        await StartNewGameAsync();
        await RefreshHistoryAsync();
    }

    [RelayCommand]
    private async Task Reset()
    {
        await StartNewGameAsync();
    }

    [RelayCommand]
    private async Task PlayCell(string parameter)
    {
        if (_busy || _status != GameStatus.InProgress || _gameId == Guid.Empty) return;
        if (!int.TryParse(parameter, out int index)) return;

        _busy = true;
        try
        {
            // Position côté API : 1-9 (le plateau est indexé 0-8 dans l'UI).
            var (game, error) = await _api.PlayMoveAsync(_gameId, index + 1);

            // Coup refusé (case occupée, partie finie…) : on ignore sans casser l'UI.
            if (error is not null || game is null) return;

            Render(game);

            if (_status != GameStatus.InProgress)
            {
                await RefreshHistoryAsync();
                RaiseEndOfGameAlert();
            }
        }
        catch (Exception ex)
        {
            ShowApiError(ex);
        }
        finally
        {
            _busy = false;
        }
    }

    private async Task StartNewGameAsync()
    {
        _busy = true;
        try
        {
            var game = await _api.CreateGameAsync(GameMode.HumanVsBot);
            _gameId = game.Id;
            Render(game);
        }
        catch (Exception ex)
        {
            ShowApiError(ex);
        }
        finally
        {
            _busy = false;
        }
    }

    /// <summary>Met l'UI à jour à partir de l'état renvoyé par l'API.</summary>
    private void Render(GameDto game)
    {
        _status = game.Status;
        RenderBoard(game.Board);
        ApplyStatus(game.Status);
    }

    private void RenderBoard(string board)
    {
        for (int i = 0; i < Cells.Count; i++)
        {
            char c = i < board.Length ? board[i] : '.';
            Cells[i].Text = c == '.' ? "" : c.ToString();
            Cells[i].TextColor = c == 'X' ? PlayerColor : c == 'O' ? BotColor : EmptyColor;
        }
    }

    private void ApplyStatus(GameStatus status)
    {
        (StatusText, StatusColor) = status switch
        {
            GameStatus.XWon => ("Vous avez gagné !", PlayerColor),
            GameStatus.OWon => ("Le Bot a gagné !", BotColor),
            GameStatus.Draw => ("Match nul !", Colors.Orange),
            _ => ("Tour de X (Vous)", IdleColor),
        };
    }

    private void RaiseEndOfGameAlert()
    {
        switch (_status)
        {
            case GameStatus.XWon:
                AlertRequested?.Invoke("Victoire", "Vous avez gagné !");
                break;
            case GameStatus.OWon:
                AlertRequested?.Invoke("Défaite", "Le Bot a gagné !");
                break;
            case GameStatus.Draw:
                AlertRequested?.Invoke("Match nul", "Aucun joueur n'a gagné.");
                break;
        }
    }

    private async Task RefreshHistoryAsync()
    {
        try
        {
            var history = await _api.GetHistoryAsync();
            GameHistory.Clear();
            foreach (var entry in history)
                GameHistory.Add(entry);
            UpdateScore();
        }
        catch (Exception ex)
        {
            ShowApiError(ex);
        }
    }

    private void UpdateScore()
    {
        int wins = GameHistory.Count(h => h.Result == "Victoire");
        int losses = GameHistory.Count(h => h.Result == "Défaite");
        int draws = GameHistory.Count(h => h.Result == "Nul");
        ScoreText = $"V: {wins}  |  D: {losses}  |  N: {draws}";
    }

    private void ShowApiError(Exception ex)
    {
        StatusText = "Erreur API — l'API est-elle lancée ?";
        StatusColor = BotColor;
        AlertRequested?.Invoke("Erreur réseau",
            $"Impossible de joindre l'API ({MorpionApiService.DefaultBaseUrl}).\n{ex.Message}");
    }
}
