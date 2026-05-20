using TD_Morpion_MAUI.Api;
using TD_Morpion_MAUI.Models;
using TD_Morpion_MAUI.Tests.Fakes;
using TD_Morpion_MAUI.ViewModels;
using Xunit;

namespace TD_Morpion_MAUI.Tests;

public class MainViewModelTests
{
    private readonly FakeMorpionApiService _api;
    private readonly MainViewModel _viewModel;

    public MainViewModelTests()
    {
        _api = new FakeMorpionApiService();
        _viewModel = new MainViewModel(_api);
    }

    private GameDto Game(string board, char current, GameStatus status)
        => new(_api.GameId, GameMode.HumanVsBot, board, current, status);

    private async Task InitAsync() => await _viewModel.InitializeCommand.ExecuteAsync(null);

    [Fact]
    public async Task Initialize_CreatesHumanVsBotGame()
    {
        await InitAsync();
        Assert.Equal(GameMode.HumanVsBot, _api.LastCreatedMode);
    }

    [Fact]
    public async Task Initialize_StatusIsPlayerTurn()
    {
        await InitAsync();
        Assert.Equal("Tour de X (Vous)", _viewModel.StatusText);
    }

    [Fact]
    public async Task Initialize_AllCellsEmpty()
    {
        await InitAsync();
        Assert.All(_viewModel.Cells, cell => Assert.Equal("", cell.Text));
    }

    [Fact]
    public async Task Initialize_ScoreIsZero()
    {
        await InitAsync();
        Assert.Equal("V: 0  |  D: 0  |  N: 0", _viewModel.ScoreText);
    }

    [Fact]
    public async Task Initialize_HistoryIsEmpty()
    {
        await InitAsync();
        Assert.Empty(_viewModel.GameHistory);
    }

    [Fact]
    public async Task PlayCell_SendsPositionIndexPlusOne()
    {
        await InitAsync();
        await _viewModel.PlayCellCommand.ExecuteAsync("4");
        Assert.Equal(5, _api.LastPlayedPosition);
    }

    [Fact]
    public async Task PlayCell_RendersBoardReturnedByApi()
    {
        await InitAsync();
        // Le joueur joue en 0 ; le serveur a aussi placé le bot en 4.
        _api.MoveHandler = _ => (Game("X...O....", 'X', GameStatus.InProgress), null);

        await _viewModel.PlayCellCommand.ExecuteAsync("0");

        Assert.Equal("X", _viewModel.Cells[0].Text);
        Assert.Equal("O", _viewModel.Cells[4].Text);
        Assert.Equal("", _viewModel.Cells[1].Text);
    }

    [Fact]
    public async Task PlayCell_Win_ShowsVictoryAndUpdatesScore()
    {
        await InitAsync();
        _api.MoveHandler = _ => (Game("XXX......", 'X', GameStatus.XWon), null);
        _api.HistoryList.Add(new GameHistoryEntry { Result = "Victoire", Date = "20/05/2026" });

        await _viewModel.PlayCellCommand.ExecuteAsync("2");

        Assert.Equal("Vous avez gagné !", _viewModel.StatusText);
        Assert.Equal("V: 1  |  D: 0  |  N: 0", _viewModel.ScoreText);
        Assert.Single(_viewModel.GameHistory);
    }

    [Fact]
    public async Task PlayCell_BotWin_ShowsDefeat()
    {
        await InitAsync();
        _api.MoveHandler = _ => (Game("OOO.X.X..", 'O', GameStatus.OWon), null);
        _api.HistoryList.Add(new GameHistoryEntry { Result = "Défaite", Date = "20/05/2026" });

        await _viewModel.PlayCellCommand.ExecuteAsync("4");

        Assert.Equal("Le Bot a gagné !", _viewModel.StatusText);
        Assert.Equal("V: 0  |  D: 1  |  N: 0", _viewModel.ScoreText);
    }

    [Fact]
    public async Task PlayCell_Draw_ShowsDraw()
    {
        await InitAsync();
        _api.MoveHandler = _ => (Game("XOXXOOOXX", 'X', GameStatus.Draw), null);
        _api.HistoryList.Add(new GameHistoryEntry { Result = "Nul", Date = "20/05/2026" });

        await _viewModel.PlayCellCommand.ExecuteAsync("8");

        Assert.Equal("Match nul !", _viewModel.StatusText);
        Assert.Equal("V: 0  |  D: 0  |  N: 1", _viewModel.ScoreText);
    }

    [Fact]
    public async Task PlayCell_InvalidMove_LeavesBoardUnchanged()
    {
        await InitAsync();
        _api.MoveHandler = _ => (null, "Coup invalide");

        await _viewModel.PlayCellCommand.ExecuteAsync("0");

        Assert.All(_viewModel.Cells, cell => Assert.Equal("", cell.Text));
        Assert.Equal("Tour de X (Vous)", _viewModel.StatusText);
    }

    [Fact]
    public async Task PlayCell_AfterGameOver_IsIgnored()
    {
        await InitAsync();
        _api.MoveHandler = _ => (Game("XXX......", 'X', GameStatus.XWon), null);
        await _viewModel.PlayCellCommand.ExecuteAsync("2"); // partie gagnée
        int callsAfterWin = _api.PlayMoveCalls;

        await _viewModel.PlayCellCommand.ExecuteAsync("5"); // doit être ignoré

        Assert.Equal(callsAfterWin, _api.PlayMoveCalls);
    }

    [Fact]
    public async Task Win_RaisesAlert()
    {
        await InitAsync();
        _api.MoveHandler = _ => (Game("XXX......", 'X', GameStatus.XWon), null);

        string? alertTitle = null;
        _viewModel.AlertRequested += (title, _) => alertTitle = title;

        await _viewModel.PlayCellCommand.ExecuteAsync("2");

        Assert.Equal("Victoire", alertTitle);
    }

    [Fact]
    public async Task Reset_CreatesNewGameAndClearsBoard()
    {
        await InitAsync();
        _api.MoveHandler = _ => (Game("X...O....", 'X', GameStatus.InProgress), null);
        await _viewModel.PlayCellCommand.ExecuteAsync("0");

        // Au reset, le serveur renvoie un plateau vide.
        _api.CreateResult = Game(".........", 'X', GameStatus.InProgress);
        await _viewModel.ResetCommand.ExecuteAsync(null);

        Assert.All(_viewModel.Cells, cell => Assert.Equal("", cell.Text));
        Assert.Equal("Tour de X (Vous)", _viewModel.StatusText);
    }
}
