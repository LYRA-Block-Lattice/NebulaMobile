using Blazored.LocalStorage;
using Fluxor;
using Lyra.Core.API;
using Lyra.Data.API;
using Lyra.Data.Crypto;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using MudBlazor.Services;
using UserLibrary.Data;

namespace NebulaMobile;

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
			});

        builder.Configuration.AddJsonFile(new EmbeddedFileProvider(typeof(App).Assembly),
                "appsettings.json", optional: false, false);
        builder.Services.AddMauiBlazorWebView();
#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
#endif
        #region lyra init
        Signatures.Switch(true);

        builder.Services.AddBlazoredLocalStorage();
        var networkid = builder.Configuration["network"];
        builder.Services.AddScoped<ILyraAPI>(a => LyraRestClient.Create(networkid, Environment.OSVersion.ToString(), "HotTest", "1.0"));
        builder.Services.AddScoped<DealerClient>(a => new DealerClient(networkid));

        var currentAssembly = typeof(MauiProgram).Assembly;
        var libAssembly = typeof(UserLibrary.Data.WalletView).Assembly;
        builder.Services.AddFluxor(options => options.ScanAssemblies(libAssembly, currentAssembly));

        builder.Services.AddMudServices(config =>
        {
            config.SnackbarConfiguration.PositionClass = MudBlazor.Defaults.Classes.Position.BottomCenter;

            config.SnackbarConfiguration.PreventDuplicates = false;
            config.SnackbarConfiguration.NewestOnTop = false;
            config.SnackbarConfiguration.ShowCloseIcon = true;
            config.SnackbarConfiguration.VisibleStateDuration = 10000;
            config.SnackbarConfiguration.HideTransitionDuration = 500;
            config.SnackbarConfiguration.ShowTransitionDuration = 500;
            config.SnackbarConfiguration.SnackbarVariant = MudBlazor.Variant.Filled;
        });

        builder.Services.AddTransient<NebulaConsts>();

        // Register a preconfigure SignalR hub connection.
        // Note the connection isnt yet started, this will be done as part of the App.razor component
        // to avoid blocking the application startup in case the connection cannot be established
        builder.Services.AddSingleton<HubConnection>(sp => {
            var eventUrl = "https://dealer.devnet.lyra.live:7070/hub";
            if (networkid == "testnet")
                eventUrl = "https://dealertestnet.lyra.live/hub";
            else if (networkid == "mainnet")
                eventUrl = "https://dealer.lyra.live/hub";
            var hub = ConnectionFactoryHelper.CreateConnection(new Uri(eventUrl));

            return hub;
        });

        builder.Services.AddSingleton<ConnectionMethodsWrapper>();
        #endregion

        return builder.Build();
	}
}
