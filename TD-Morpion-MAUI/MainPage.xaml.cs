using TD_Morpion_MAUI.Models;

namespace TD_Morpion_MAUI;

public partial class MainPage : ContentPage
{
    private Board _board;
    private char _currentPlayer;
    private bool _gameOver;
    private Button[] _buttons;

    public MainPage()
    {
        InitializeComponent();
        _board = new Board();
        _currentPlayer = 'X';
        _gameOver = false;
        _buttons = new Button[] { Btn0, Btn1, Btn2, Btn3, Btn4, Btn5, Btn6, Btn7, Btn8 };
    }

    private async void OnCellClicked(object? sender, EventArgs e)
    {
        if (_gameOver || sender is not Button button)
            return;

        int index = Array.IndexOf(_buttons, button);
        if (index == -1) return;

        int position = index + 1;
        var move = _board.IsValidMove(position);
        if (move == null) return;

        _board.PlayMove(move.Value.line, move.Value.column, _currentPlayer);
        button.Text = _currentPlayer.ToString();
        button.TextColor = _currentPlayer == 'X' ? Color.FromArgb("#1565C0") : Color.FromArgb("#C62828");

        if (_board.CheckWin(_currentPlayer))
        {
            _gameOver = true;
            StatusLabel.Text = $"Joueur {_currentPlayer} a gagn\u00e9 !";
            StatusLabel.TextColor = _currentPlayer == 'X' ? Color.FromArgb("#1565C0") : Color.FromArgb("#C62828");
            await DisplayAlertAsync("Victoire", $"Le joueur {_currentPlayer} a gagn\u00e9 !", "OK");
            return;
        }

        if (_board.IsFull())
        {
            _gameOver = true;
            StatusLabel.Text = "Match nul !";
            StatusLabel.TextColor = Colors.Orange;
            await DisplayAlertAsync("Match nul", "Aucun joueur n'a gagn\u00e9.", "OK");
            return;
        }

        _currentPlayer = _currentPlayer == 'X' ? 'O' : 'X';
        StatusLabel.Text = $"Au tour de {_currentPlayer}";
        StatusLabel.TextColor = _currentPlayer == 'X' ? Color.FromArgb("#1565C0") : Color.FromArgb("#C62828");
    }

    private void OnResetClicked(object? sender, EventArgs e)
    {
        _board = new Board();
        _currentPlayer = 'X';
        _gameOver = false;

        foreach (var button in _buttons)
        {
            button.Text = "";
            button.TextColor = Color.FromArgb("#333333");
        }

        StatusLabel.Text = "Au tour de X";
        StatusLabel.TextColor = Color.FromArgb("#512BD4");
    }
}
