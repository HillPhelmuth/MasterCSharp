using System;
using System.Collections.Generic;
using System.Text;
using MasterSharpOpen.Shared.ArenaChallenge;
using MasterSharpOpen.Shared.CodeServices;
using MasterSharpOpen.Shared.StaticAuth;
using MasterSharpOpen.Shared.StaticAuth.Interfaces;
using MatBlazor;
using Microsoft.Extensions.DependencyInjection;

namespace MasterSharpOpen.Shared.ExtensionMethods
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
