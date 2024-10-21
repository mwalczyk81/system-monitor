using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using SystemMonitor.Services;

namespace SystemMonitor
{
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

            builder.Services.AddMudServices();

#if WINDOWS
        builder.Services.AddScoped<ISystemPerformanceService, WindowsSystemPerformanceService>();
#elif MACCATALYST
        builder.Services.AddScoped<ISystemPerformanceService, MacOSSystemPerformanceService>();
#elif LINUX
        builder.Services.AddScoped<ISystemPerformanceService, LinuxSystemPerformanceService>();
#elif ANDROID
            builder.Services.AddScoped<ISystemPerformanceService, AndroidSystemPerformanceService>(sp => new AndroidSystemPerformanceService(Android.App.Application.Context));
#elif IOS
        builder.Services.AddScoped<ISystemPerformanceService, iOSSystemPerformanceService>();
#endif
            builder.Services.AddMauiBlazorWebView();

#if DEBUG
    		builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif
            return builder.Build();
        }
    }
}
