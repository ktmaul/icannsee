using System;
using System.Collections.Generic;
using System.Text;
using Icannsee;
using Icannsee.Services;
using Icannsee.Services.Lookups;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IcannseeServiceCollectionExtensions
    {
        public static IServiceCollection AddLocalLookupService(this IServiceCollection services)
        {
            return services.AddSingleton<ILookupService, LocalLookupService>();
        }

        public static IServiceCollection AddRemoteLookupService(this IServiceCollection services, string remoteUri)
        {
            services.AddHttpClient("remote-lookups", client =>
            {
                client.BaseAddress = new Uri(remoteUri);
            });

            return services.AddSingleton<ILookupService, RemoteLookupService>();
        }

        public static IServiceCollection AddBasicLookupProviders(this IServiceCollection services)
        {
            return services
                .AddHttpClient()
                .AddSingleton<ILookupProvider, GeoIPLookupProvider>()
                .AddSingleton<ILookupProvider, PingLookupProvider>()
                .AddSingleton<ILookupProvider, RdapLookupProvider>();
        }
    }
}
