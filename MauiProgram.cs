using Mageki.Services;
using MarketAlly.TouchEffect.Maui.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using Mopups.Hosting;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace Mageki;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		App.InitNLog();
		VersionTracking.Track();
		AppDomain.CurrentDomain.UnhandledException += App.CurrentDomainOnUnhandledException;
		TaskScheduler.UnobservedTaskException += App.TaskSchedulerOnUnobservedTaskException;
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiTouchEffect()
			.ConfigureMopups()
			.UseSkiaSharp()
#if WINDOWS
			.ConfigureLifecycleEvents(events =>
			{
				events.AddWindows(windows => windows.OnWindowCreated(PlatformWindowState.Register));
			})
#endif
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
				fonts.AddFont("MaterialIconsRound-Regular.otf", "MaterialIconsRound");
			});

		builder.Services.AddSingleton<App>();
		builder.Services.AddTransient<MainPage>();
		builder.Services.AddSingleton<SettingsViewModel>();
		builder.Services.AddSingleton<SettingsPopup>();
		builder.Services.AddSingleton<ISettingsPopupService, SettingsPopupService>();
		builder.Services.AddPlatformServices();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
