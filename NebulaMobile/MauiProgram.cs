using Blazored.LocalStorage;
using Fluxor;
using Lyra.Core.API;
using Lyra.Data.API;
using Lyra.Data.Crypto;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using MudBlazor;
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

		builder.Services.AddMauiBlazorWebView();
#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
#endif

		// My code
		builder.Configuration.AddJsonFile("appsettings.json");
		var networkid = builder.Configuration["network"];

		Signatures.Switch(true);
		builder.Services.AddHttpClient();
		builder.Services.AddBlazoredLocalStorage();

		builder.Services.AddTransient<NebulaConsts>();
		builder.Services.AddTransient<ILyraAPI>(a => LyraRestClient.Create(networkid, Environment.OSVersion.ToString(), "MAUI", "1.0"/*, $"http://nebula.{networkid}.lyra.live:{Neo.Settings.Default.P2P.WebAPI}/api/Node/"*/));

		var currentAssembly = typeof(MauiProgram).Assembly;
		var userlib = typeof(UserLibrary.Data.WalletView).Assembly;
		builder.Services.AddFluxor(options => options.ScanAssemblies(currentAssembly, userlib));

		builder.Services.AddMudServices(config =>
		{
			config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomCenter;

			config.SnackbarConfiguration.PreventDuplicates = false;
			config.SnackbarConfiguration.NewestOnTop = false;
			config.SnackbarConfiguration.ShowCloseIcon = true;
			config.SnackbarConfiguration.VisibleStateDuration = 10000;
			config.SnackbarConfiguration.HideTransitionDuration = 500;
			config.SnackbarConfiguration.ShowTransitionDuration = 500;
			config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
		});

		// Register a preconfigure SignalR hub connection.
		// Note the connection isnt yet started, this will be done as part of the App.razor component
		// to avoid blocking the application startup in case the connection cannot be established
		builder.Services.AddSingleton<HubConnection>(sp => {
			var eventUrl = "https://192.168.3.91:7070/hub";
			if (networkid == "testnet")
				eventUrl = "https://dealertestnet.lyra.live/hub";
			else if (networkid == "mainnet")
				eventUrl = "https://dealer.lyra.live/hub";
			var hub = ConnectionFactoryHelper.CreateConnection(new Uri(eventUrl));

			return hub;
		});

		builder.Services.AddSingleton<ConnectionMethodsWrapper>();

		return builder.Build();
	}
}
