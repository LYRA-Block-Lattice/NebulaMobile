﻿using Microsoft.AspNetCore.Components.WebView.Maui;
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
				})
				.Host
				.ConfigureAppConfiguration((app, config) =>
				{
					var assembly = typeof(App).GetTypeInfo().Assembly;
					config.AddJsonFile(new EmbeddedFileProvider(assembly), "appsettings.json", optional: false, false);
				});

			builder.Services.AddBlazorWebView();

			// my
			Signatures.Switch(true);
			builder.Services.AddHttpClient();
			builder.Services.AddBlazoredLocalStorage();
			var networkid = builder.Configuration["network"];

			builder.Services.AddTransient<NebulaConsts>();
			builder.Services.AddSingleton<PeriodicExecutor>(x =>
				new PeriodicExecutor(networkid)
			);

			// use dedicate host to avoid "random" result from api.lyra.live which is dns round-robbined. <-- not fail safe
			//services.AddTransient<LyraRestClient>(a => LyraRestClient.Create(networkid, Environment.OSVersion.ToString(), "Nebula", "1.0"/*, $"http://nebula.{networkid}.lyra.live:{Neo.Settings.Default.P2P.WebAPI}/api/Node/"*/));

			builder.Services.AddScoped<ILyraAPI>(provider =>
			{
				var lc = LyraRestClient.Create(networkid, Environment.OSVersion.ToString(), "MAUI", "1.0");
				return lc;
				//var client = new LyraAggregatedClient(networkid, true, null);
				//var t = Task.Run(async () => { await client.InitAsync(); });
				//Task.WaitAll(t);
				//return client;
			});

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

			return builder.Build();
        }
    }
}