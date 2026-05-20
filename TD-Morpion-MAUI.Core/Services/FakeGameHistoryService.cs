using System.Collections.ObjectModel;
using TD_Morpion_MAUI.Models;

namespace TD_Morpion_MAUI.Services;

public class FakeGameHistoryService : IGameHistoryService
{
    public ObservableCollection<GameHistoryEntry> History { get; } = new();

    public void Add(string result)
    {
        History.Insert(0, new GameHistoryEntry
        {
            Result = result,
            Date = DateTime.Now.ToString("dd/MM/yyyy")
        });
    }
}
