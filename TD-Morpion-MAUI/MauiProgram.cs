using Microsoft.Extensions.Logging;
using TD_Morpion_MAUI.Models;
using TD_Morpion_MAUI.Services;
using TD_Morpion_MAUI.ViewModels;

namespace TD_Morpion_MAUI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Services — client de l'API Morpion (le jeu est géré côté serveur)
        builder.Services.AddSingleton<IMorpionApiService>(
            _ => new MorpionApiService(new HttpClient { BaseAddress = new Uri(MorpionApiService.DefaultBaseUrl) }));

        // ViewModels
        builder.Services.AddTransient<MainViewModel>();

        // Pages
        builder.Services.AddTransient<MainPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
