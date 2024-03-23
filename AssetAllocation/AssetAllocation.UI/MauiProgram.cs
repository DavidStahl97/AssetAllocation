using AssetAllocation.DB;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;

namespace AssetAllocation.UI
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder.Configuration.AddAppConfiguration();

            builder.UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

            builder.Services.AddMudServices();

            builder.Services.AddDb();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }

        private static void AddDb(this IServiceCollection services)
        {
            services.AddSetting<AssetAllocationDbSettings>("DB");
            services.AddTransient<IAssetAllocationDbContext, AssetAllocationDbContext>();
        }
    }
}
