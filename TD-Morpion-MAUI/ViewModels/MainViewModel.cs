using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TD_Morpion_MAUI.Models;

namespace TD_Morpion_MAUI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private Board _board = new();
    private char _currentPlayer = 'X';
    private bool _gameOver;

    [ObservableProperty]
    private string _statusText = "Au tour de X";

    [ObservableProperty]
    private Color _statusColor = Color.FromArgb("#512BD4");

    public ObservableCollection<CellViewModel> Cells { get; } = new(
        Enumerable.Range(0, 9).Select(_ => new CellViewModel())
    );

    public event Action<string, string>? AlertRequested;

    [RelayCommand]
    private void PlayCell(string parameter)
    {
        if (_gameOver) return;

        int index = int.Parse(parameter);
        int position = index + 1;

        var move = _board.IsValidMove(position);
        if (move == null) return;

        _board.PlayMove(move.Value.line, move.Value.column, _currentPlayer);

        Cells[index].Text = _currentPlayer.ToString();
        Cells[index].TextColor = _currentPlayer == 'X'
            ? Color.FromArgb("#1565C0")
            : Color.FromArgb("#C62828");

        if (_board.CheckWin(_currentPlayer))
        {
            _gameOver = true;
            StatusText = $"Joueur {_currentPlayer} a gagné !";
            StatusColor = _currentPlayer == 'X'
                ? Color.FromArgb("#1565C0")
                : Color.FromArgb("#C62828");
            AlertRequested?.Invoke("Victoire", $"Le joueur {_currentPlayer} a gagné !");
            return;
        }

        if (_board.IsFull())
        {
            _gameOver = true;
            StatusText = "Match nul !";
            StatusColor = Colors.Orange;
            AlertRequested?.Invoke("Match nul", "Aucun joueur n'a gagné.");
            return;
        }

        _currentPlayer = _currentPlayer == 'X' ? 'O' : 'X';
        StatusText = $"Au tour de {_currentPlayer}";
        StatusColor = _currentPlayer == 'X'
            ? Color.FromArgb("#1565C0")
            : Color.FromArgb("#C62828");
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

        StatusText = "Au tour de X";
        StatusColor = Color.FromArgb("#512BD4");
    }
}
