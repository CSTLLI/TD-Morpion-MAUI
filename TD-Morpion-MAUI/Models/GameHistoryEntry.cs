namespace TD_Morpion_MAUI.Models;

public class GameHistoryEntry
{
    public string Result { get; set; } = "";
    public string Date { get; set; } = "";
    public string DisplayText => $"{Date} - {Result}";
}
