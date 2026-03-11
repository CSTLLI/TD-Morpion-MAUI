using CommunityToolkit.Mvvm.ComponentModel;

namespace TD_Morpion_MAUI.ViewModels;

public partial class CellViewModel : ObservableObject
{
    [ObservableProperty]
    private string _text = "";

    [ObservableProperty]
    private Color _textColor = Color.FromArgb("#333333");
}
