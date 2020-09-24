using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace MasterSharpOpen.Shared.StaticAuth
{
    public static class AuthServiceExtension
    {
        public static IServiceCollection AddAuthentication(this IServiceCollection services)
        {
            return services
                .AddAuthorizationCore()
                .AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
        }
    }
}