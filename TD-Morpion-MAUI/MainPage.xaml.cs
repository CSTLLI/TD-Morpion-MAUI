using TD_Morpion_MAUI.ViewModels;

namespace TD_Morpion_MAUI;

public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;

        viewModel.AlertRequested += async (title, message) =>
        {
            await DisplayAlertAsync(title, message, "OK");
        };
    }
}
