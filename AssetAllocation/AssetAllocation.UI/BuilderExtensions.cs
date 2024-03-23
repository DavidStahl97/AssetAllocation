using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AssetAllocation.UI
{
    public static class BuilderExtensions
    {
        public static void AddAppConfiguration(this ConfigurationManager configurationManager)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("AssetAllocation.UI.appsettings.json");

            Guard.IsNotNull(stream);

            var config = new ConfigurationBuilder()
                        .AddJsonStream(stream)
                        .Build();

            configurationManager.AddConfiguration(config);
        }

        public static void AddSetting<T>(this IServiceCollection services, string settingsName)
            where T : class
        {
            services.AddOptions<T>()
                .BindConfiguration(settingsName)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddSingleton(resolver =>
                resolver.GetRequiredService<IOptions<T>>().Value);
        }
    }
}
