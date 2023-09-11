using Maui.Phone.Interfaces;
using Maui.Phone.Services;
using Maui.Phone.ViewModels;
using Microsoft.Extensions.Logging;
using Serilog;
using Maui.WatchCommunication.Interfaces;
using Maui.WatchCommunication;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using CommunityToolkit.Maui;

namespace Maui.Phone;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		Directory.CreateDirectory(Constants.StoragePath);
		var outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} <{SourceContext}> [{Level:u3}] {Message:lj}{NewLine}{Exception}";
		Log.Logger = new LoggerConfiguration()
			.Enrich.FromLogContext()
			.WriteTo.Console(outputTemplate: outputTemplate)
			.WriteTo.File(path: Constants.LogPath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7, outputTemplate: outputTemplate)
			.CreateLogger();
		var startupLog = Log.ForContext<MauiApp>();
        startupLog.Information("Bootstrapping application");
        var builder = MauiApp.CreateBuilder();
		var openSans = Path.Combine(AppContext.BaseDirectory, "OpenSans-Regular.ttf");
		var openSansBold = Path.Combine(AppContext.BaseDirectory, "OpenSans-Semibold.ttf");
        try
		{
            builder
                .UseMauiApp<App>()
				.UseMauiCommunityToolkit()
				.ConfigureFonts(fonts =>
				{
					fonts.AddFont(openSans, "OpenSansRegular");
					fonts.AddFont(openSansBold, "OpenSansSemibold");
				});

#if DEBUG
			builder.Logging.AddDebug();
#endif
			builder.Logging.AddSerilog();

			builder.Services.AddSingleton<IHandler, Handler>();
			builder.Services.AddSingleton<WearableInteractionService>();
			builder.Services.AddSingleton<PageService>();
			builder.Services.AddSingleton<DeviceOrientationService>();

            builder.Services.AddTransient<MainPageViewModel>();
            builder.Services.AddTransient<MainPage>();
#if IOS
		ScrollViewHandler.Mapper.AppendToMapping("ContentSize", (handler, view) =>
		{
			handler.PlatformView.UpdateContentSize(handler.VirtualView.ContentSize);
			handler.PlatformArrange(handler.PlatformView.Frame.ToRectangle());
		});
#endif
            startupLog.Information("Bootstrapping completed, launching app");
		}
		catch (Exception ex)
		{
            startupLog.Fatal(ex, "Uncaught low level exception occurred, app is closing");
            //throw;
        }
		finally
		{
			//Log.CloseAndFlush();
        }
        return builder.Build();
    }
}