using Blazored.LocalStorage;
using Fluxor;
using Lyra.Core.API;
using Lyra.Data.API;
using Lyra.Data.Crypto;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Localization;
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

        builder.Services.AddLocalization(options =>
        {
            options.ResourcesPath = "Resources";
        });

        builder.Services.AddBlazoredLocalStorage();
        var networkid = builder.Configuration["network"];
        builder.Services.AddScoped<ILyraAPI>(a => LyraRestClient.Create(networkid, Environment.OSVersion.ToString(), "HotTest", "1.0"));
        //builder.Services.AddScoped<DealerClient>(a => new DealerClient(networkid));

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
        builder.Services.AddSingleton<DealerConnMgr>();
        #endregion

        return builder.Build();
	}
}
 