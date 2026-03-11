using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TD_Morpion_MAUI.Models;
using TD_Morpion_MAUI.Services;

namespace TD_Morpion_MAUI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private Board _board = new();
    private char _currentPlayer = 'X';
    private bool _gameOver;
    private readonly IGameHistoryService _historyService;

    [ObservableProperty]
    private string _statusText = "Tour de X (Vous)";

    [ObservableProperty]
    private Color _statusColor = Color.FromArgb("#512BD4");

    [ObservableProperty]
    private string _scoreText = "";

    public ObservableCollection<CellViewModel> Cells { get; } = new(
        Enumerable.Range(0, 9).Select(_ => new CellViewModel())
    );

    public ObservableCollection<GameHistoryEntry> GameHistory => _historyService.History;

    public event Action<string, string>? AlertRequested;

    public MainViewModel(IGameHistoryService historyService)
    {
        _historyService = historyService;
        UpdateScore();
    }

    [RelayCommand]
    private async Task PlayCell(string parameter)
    {
        if (_gameOver || _currentPlayer != 'X') return;

        int index = int.Parse(parameter);
        int position = index + 1;

        var move = _board.IsValidMove(position);
        if (move == null) return;

        _board.PlayMove(move.Value.line, move.Value.column, _currentPlayer);

        Cells[index].Text = _currentPlayer.ToString();
        Cells[index].TextColor = Color.FromArgb("#1565C0");

        if (_board.CheckWin(_currentPlayer))
        {
            _gameOver = true;
            StatusText = "Vous avez gagné !";
            StatusColor = Color.FromArgb("#1565C0");
            _historyService.Add("Victoire");
            UpdateScore();
            AlertRequested?.Invoke("Victoire", "Vous avez gagné !");
            return;
        }

        if (_board.IsFull())
        {
            _gameOver = true;
            StatusText = "Match nul !";
            StatusColor = Colors.Orange;
            _historyService.Add("Nul");
            UpdateScore();
            AlertRequested?.Invoke("Match nul", "Aucun joueur n'a gagné.");
            return;
        }

        _currentPlayer = 'O';
        StatusText = "Tour de O (Bot)...";
        StatusColor = Color.FromArgb("#C62828");

        await Task.Delay(500);
        BotPlay();
    }

    private void BotPlay()
    {
        int botIndex = _board.GetBotMove();
        int position = botIndex + 1;

        var move = _board.IsValidMove(position);
        if (move == null) return;

        _board.PlayMove(move.Value.line, move.Value.column, 'O');

        Cells[botIndex].Text = "O";
        Cells[botIndex].TextColor = Color.FromArgb("#C62828");

        if (_board.CheckWin('O'))
        {
            _gameOver = true;
            StatusText = "Le Bot a gagné !";
            StatusColor = Color.FromArgb("#C62828");
            _historyService.Add("Défaite");
            UpdateScore();
            AlertRequested?.Invoke("Défaite", "Le Bot a gagné !");
            return;
        }

        if (_board.IsFull())
        {
            _gameOver = true;
            StatusText = "Match nul !";
            StatusColor = Colors.Orange;
            _historyService.Add("Nul");
            UpdateScore();
            AlertRequested?.Invoke("Match nul", "Aucun joueur n'a gagné.");
            return;
        }

        _currentPlayer = 'X';
        StatusText = "Tour de X (Vous)";
        StatusColor = Color.FromArgb("#1565C0");
    }

    private void UpdateScore()
    {
        var history = _historyService.History;
        int wins = history.Count(h => h.Result == "Victoire");
        int losses = history.Count(h => h.Result == "Défaite");
        int draws = history.Count(h => h.Result == "Nul");
        ScoreText = $"V: {wins}  |  D: {losses}  |  N: {draws}";
    }

    [RelayCommand]
    private void Reset()
    {
        _board = new Board();
        _currentPlayer = 'X';
        _gameOver = false;

        foreach (var cell in Cells)
        {
            cell.Text = "";
            cell.TextColor = Color.FromArgb("#333333");
        }

        StatusText = "Tour de X (Vous)";
        StatusColor = Color.FromArgb("#512BD4");
    }
}
