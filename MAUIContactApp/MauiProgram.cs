using CommunityToolkit.Maui.Markup;
using MAUIContactApp.Controls;
using Microsoft.Extensions.Logging;

#if ANDROID
using MAUIContactApp.Platforms.Droid.Handlers.Renderers;
#endif

#if IOS
using MAUIContactApp.Platforms.iOS.Handlers.Renderers;
#endif

namespace MAUIContactApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>().UseMauiCommunityToolkitMarkup()
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            })
            .ConfigureMauiHandlers(handlers =>
            {
                handlers.AddHandler(typeof(HybridWebView), typeof(HybridWebViewHandler));
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
