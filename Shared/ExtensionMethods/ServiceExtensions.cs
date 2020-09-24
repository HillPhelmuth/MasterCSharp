using BlazorApp.Shared.ArenaChallenge;
using BlazorApp.Shared.CodeServices;
using BlazorApp.Shared.StaticAuth;
using BlazorApp.Shared.StaticAuth.Interfaces;
using MatBlazor;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorApp.Shared.ExtensionMethods
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddMasterSharpServices(this IServiceCollection service)
        {
            service.AddSingleton<CodeEditorService>();
            service.AddSingleton<AppStateService>();
            service.AddScoped<ICustomAuthenticationStateProvider, CustomAuthenticationStateProvider>();
            service.AddSingleton<ArenaService>();
            service.AddMatToaster(config =>
            {
                config.Position = MatToastPosition.BottomCenter;
                config.PreventDuplicates = true;
                config.NewestOnTop = true;
                config.ShowCloseButton = true;
                config.MaximumOpacity = 95;
                config.VisibleStateDuration = 5000;

            });
            return service;
        }
    }
}
