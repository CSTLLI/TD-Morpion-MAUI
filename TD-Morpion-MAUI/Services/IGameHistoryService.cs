using System.Collections.ObjectModel;
using TD_Morpion_MAUI.Models;

namespace TD_Morpion_MAUI.Services;

public interface IGameHistoryService
{
    ObservableCollection<GameHistoryEntry> History { get; }
    void Add(string result);
}
