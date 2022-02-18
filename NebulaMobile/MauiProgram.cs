using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Hosting;
using Fluxor;
using System.Net.Http;
using Lyra.Data.API;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Blazored.LocalStorage;
using Lyra.Core.API;
using System;
using Lyra.Data.Crypto;
using MudBlazor.Services;
using MudBlazor;
using UserLibrary.Data;
using Microsoft.AspNetCore.SignalR.Client;

namespace NebulaMobile
{
	public static class MauiProgram
	{
		public static MauiApp CreateMauiApp()
		{
			var builder = MauiApp.CreateBuilder();
			builder
				.RegisterBlazorMauiWebView()
				.UseMauiApp<App>()
				.ConfigureFonts(fonts =>
				{
					fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				});

			builder.Configuration.AddJsonFile("appsettings.json");
            builder.Services.AddBlazorWebView();

			// my
			Signatures.Switch(true);
			builder.Services.AddHttpClient();
			builder.Services.AddBlazoredLocalStorage();
			var networkid = builder.Configuration["network"];

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
}