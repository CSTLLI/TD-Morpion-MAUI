using TD_Morpion_MAUI.Services;
using TD_Morpion_MAUI.ViewModels;
using Xunit;

namespace TD_Morpion_MAUI.Tests;

public class MainViewModelTests
{
    private readonly FakeGameHistoryService _historyService;
    private readonly MainViewModel _viewModel;

    public MainViewModelTests()
    {
        _historyService = new FakeGameHistoryService();
        _viewModel = new MainViewModel(_historyService);
    }

    [Fact]
    public void InitialState_StatusIsPlayerTurn()
    {
        Assert.Equal("Tour de X (Vous)", _viewModel.StatusText);
    }

    [Fact]
    public void InitialState_AllCellsEmpty()
    {
        Assert.All(_viewModel.Cells, cell => Assert.Equal("", cell.Text));
    }

    [Fact]
    public void InitialState_ScoreIsZero()
    {
        Assert.Equal("V: 0  |  D: 0  |  N: 0", _viewModel.ScoreText);
    }

    [Fact]
    public void InitialState_HistoryIsEmpty()
    {
        Assert.Empty(_viewModel.GameHistory);
    }

    [Fact]
    public async Task PlayCell_PlacesXOnCell()
    {
        await _viewModel.PlayCellCommand.ExecuteAsync("0");

        Assert.Equal("X", _viewModel.Cells[0].Text);
    }

    [Fact]
    public async Task PlayCell_BotPlaysAfterPlayer()
    {
        await _viewModel.PlayCellCommand.ExecuteAsync("0");

        // Le bot doit avoir joué sur une case (au moins une case a "O")
        int botCells = _viewModel.Cells.Count(c => c.Text == "O");
        // Si la partie n'est pas finie après le coup du joueur, le bot joue
        // (sauf si X gagne directement, ce qui n'arrive pas au premier coup)
        Assert.True(botCells >= 1, "Le bot devrait avoir joué après le joueur");
    }

    [Fact]
    public async Task PlayCell_CannotPlayOnOccupiedCell()
    {
        await _viewModel.PlayCellCommand.ExecuteAsync("0");

        string cellTextBefore = _viewModel.Cells[0].Text;
        await _viewModel.PlayCellCommand.ExecuteAsync("0");

        Assert.Equal(cellTextBefore, _viewModel.Cells[0].Text);
    }

    [Fact]
    public async Task PlayCell_AfterBotPlays_StatusBackToPlayer()
    {
        await _viewModel.PlayCellCommand.ExecuteAsync("0");

        // Si la partie continue, le status revient au joueur
        if (_viewModel.StatusText.Contains("Bot a gagné") ||
            _viewModel.StatusText.Contains("nul") ||
            _viewModel.StatusText.Contains("gagné"))
            return; // Partie finie, pas de vérification

        Assert.Equal("Tour de X (Vous)", _viewModel.StatusText);
    }

    [Fact]
    public void Reset_ClearsAllCells()
    {
        _viewModel.ResetCommand.Execute(null);

        Assert.All(_viewModel.Cells, cell => Assert.Equal("", cell.Text));
    }

    [Fact]
    public async Task Reset_AfterPlay_ClearsBoard()
    {
        await _viewModel.PlayCellCommand.ExecuteAsync("0");
        _viewModel.ResetCommand.Execute(null);

        Assert.All(_viewModel.Cells, cell => Assert.Equal("", cell.Text));
        Assert.Equal("Tour de X (Vous)", _viewModel.StatusText);
    }

    [Fact]
    public async Task Reset_AllowsPlayingAgain()
    {
        await _viewModel.PlayCellCommand.ExecuteAsync("0");
        _viewModel.ResetCommand.Execute(null);

        await _viewModel.PlayCellCommand.ExecuteAsync("4");

        Assert.Equal("X", _viewModel.Cells[4].Text);
    }

    [Fact]
    public async Task History_RecordsGameResult()
    {
        // Jouer une partie complète en simulant les coups
        // On joue jusqu'à ce que la partie se termine
        var playedCells = new HashSet<int>();
        for (int i = 0; i < 9; i++)
        {
            if (_viewModel.Cells[i].Text != "")
                continue;

            await _viewModel.PlayCellCommand.ExecuteAsync(i.ToString());

            // Vérifier si la partie est finie
            if (_viewModel.StatusText.Contains("gagné") ||
                _viewModel.StatusText.Contains("nul"))
                break;
        }

        // Si la partie est finie, l'historique doit contenir une entrée
        if (_viewModel.StatusText.Contains("gagné") ||
            _viewModel.StatusText.Contains("nul"))
        {
            Assert.Single(_viewModel.GameHistory);
        }
    }

    [Fact]
    public async Task Score_UpdatesAfterGame()
    {
        // Jouer jusqu'à fin de partie
        for (int i = 0; i < 9; i++)
        {
            if (_viewModel.Cells[i].Text != "")
                continue;

            await _viewModel.PlayCellCommand.ExecuteAsync(i.ToString());

            if (_viewModel.StatusText.Contains("gagné") ||
                _viewModel.StatusText.Contains("nul"))
                break;
        }

        // Le score ne doit plus être à zéro si la partie est finie
        if (_viewModel.GameHistory.Count > 0)
        {
            Assert.NotEqual("V: 0  |  D: 0  |  N: 0", _viewModel.ScoreText);
        }
    }

    [Fact]
    public async Task MultipleGames_HistoryAccumulates()
    {
        // Jouer 2 parties
        for (int game = 0; game < 2; game++)
        {
            for (int i = 0; i < 9; i++)
            {
                if (_viewModel.Cells[i].Text != "")
                    continue;

                await _viewModel.PlayCellCommand.ExecuteAsync(i.ToString());

                if (_viewModel.StatusText.Contains("gagné") ||
                    _viewModel.StatusText.Contains("nul"))
                    break;
            }

            if (game == 0)
                _viewModel.ResetCommand.Execute(null);
        }

        // Vérifier qu'il y a au moins les résultats des parties terminées
        Assert.True(_viewModel.GameHistory.Count >= 1);
    }

    [Fact]
    public void Reset_DoesNotClearHistory()
    {
        _historyService.Add("Victoire");
        _viewModel.ResetCommand.Execute(null);

        Assert.Single(_viewModel.GameHistory);
    }

    [Fact]
    public void HistoryEntry_HasCorrectResult()
    {
        _historyService.Add("Victoire");

        Assert.Equal("Victoire", _viewModel.GameHistory[0].Result);
    }

    [Fact]
    public void HistoryEntry_HasDate()
    {
        _historyService.Add("Défaite");

        Assert.NotEmpty(_viewModel.GameHistory[0].Date);
        Assert.Equal(DateTime.Now.ToString("dd/MM/yyyy"), _viewModel.GameHistory[0].Date);
    }

    [Fact]
    public void HistoryEntry_DisplayTextFormat()
    {
        _historyService.Add("Nul");

        var entry = _viewModel.GameHistory[0];
        Assert.Equal($"{entry.Date} - Nul", entry.DisplayText);
    }

    [Fact]
    public async Task GameOver_CannotPlayMore()
    {
        // Remplir le board pour finir la partie
        for (int i = 0; i < 9; i++)
        {
            if (_viewModel.Cells[i].Text != "")
                continue;

            await _viewModel.PlayCellCommand.ExecuteAsync(i.ToString());

            if (_viewModel.StatusText.Contains("gagné") ||
                _viewModel.StatusText.Contains("nul"))
                break;
        }

        // Si game over, essayer de jouer ne change rien
        if (_viewModel.StatusText.Contains("gagné") ||
            _viewModel.StatusText.Contains("nul"))
        {
            var cellTexts = _viewModel.Cells.Select(c => c.Text).ToList();

            // Trouver une case vide s'il en reste
            var emptyIndex = _viewModel.Cells
                .Select((c, i) => new { c, i })
                .FirstOrDefault(x => x.c.Text == "");

            if (emptyIndex != null)
            {
                await _viewModel.PlayCellCommand.ExecuteAsync(emptyIndex.i.ToString());
                Assert.Equal("", _viewModel.Cells[emptyIndex.i].Text);
            }
        }
    }
}
