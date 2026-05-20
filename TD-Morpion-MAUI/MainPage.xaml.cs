using TD_Morpion_MAUI.ViewModels;

namespace TD_Morpion_MAUI;

public partial class MainPage : ContentPage
{
    private readonly MainViewModel _viewModel;
    private bool _initialized;

    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;

        viewModel.AlertRequested += async (title, message) =>
        {
            await DisplayAlertAsync(title, message, "OK");
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Crée la partie côté API une seule fois, à la première apparition.
        if (_initialized) return;
        _initialized = true;
        await _viewModel.InitializeCommand.ExecuteAsync(null);
    }
}
