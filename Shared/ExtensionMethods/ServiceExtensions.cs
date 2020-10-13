using MasterCSharp.Shared.ArenaChallenge;
using MasterCSharp.Shared.CodeServices;
using MasterCSharp.Shared.StaticAuth;
using MasterCSharp.Shared.StaticAuth.Interfaces;
using MatBlazor;
using Microsoft.Extensions.DependencyInjection;

namespace MasterCSharp.Shared.ExtensionMethods
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
