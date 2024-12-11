using Microsoft.Extensions.Logging;
using VoiceVault.Maui.Services;
using VoiceVault.Maui.ViewModels; // Add this line

namespace VoiceVault.Maui;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
		=> MauiApp.CreateBuilder()
		.UseMauiApp<App>()
		.ConfigureFonts(fonts =>
		{
			fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
		})
		.RegisterServices()
		.RegisterViewModels()
		.RegisterViews()
		.Build();

	 public static MauiAppBuilder RegisterServices(this MauiAppBuilder mauiAppBuilder)
    {
        //mauiAppBuilder.Services.AddTransient<ILoggingService, LoggingService>();
        mauiAppBuilder.Services.AddSingleton<HttpClient>();
        mauiAppBuilder.Services.AddSingleton<IUploadService, UploadService>();

		#if DEBUG
			mauiAppBuilder.Logging.AddDebug();
		#endif

        // More services registered here.

        return mauiAppBuilder;        
    }

    public static MauiAppBuilder RegisterViewModels(this MauiAppBuilder mauiAppBuilder)
    {
        mauiAppBuilder.Services.AddSingleton<MainViewModel>();

        // More view-models registered here.

        return mauiAppBuilder;        
    }

    public static MauiAppBuilder RegisterViews(this MauiAppBuilder mauiAppBuilder)
    {
        mauiAppBuilder.Services.AddSingleton<MainPage>();

        // More views registered here.

        return mauiAppBuilder;        
    }
}
