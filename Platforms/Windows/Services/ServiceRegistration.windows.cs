using Mageki.Services;

namespace Mageki;

public static partial class ServiceRegistration
{
    public static partial IServiceCollection AddPlatformServices(this IServiceCollection services)
    {
        services.AddSingleton<INfcService, Platforms.Windows.Services.NfcService>();
        return services;
    }
}
