using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Blazor.ModalDialog;
using MasterSharpOpen.Shared;
using MasterSharpOpen.Shared.CodeServices;
using MasterSharpOpen.Shared.ExtensionMethods;
using MasterSharpOpen.Shared.StaticAuth;
using MasterSharpOpen.Shared.StaticAuth.Interfaces;
using MatBlazor;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TextCopy;

namespace MasterSharpOpen.Client
{
   
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            builder.Services.AddTransient(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) }); 
            builder.Services.AddHttpClient<PublicClient>(client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));
            builder.Services.AddHttpClient<PublicGithubClient>(client =>
                client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));
            builder.Services.AddAuthentication();
            builder.Services.AddMasterSharpServices();
            builder.Services.InjectClipboard();
            builder.Services.AddModalDialog();
            //builder.Services.AddMatToaster(config =>
            //{
            //    config.Position = MatToastPosition.BottomRight;
            //    config.PreventDuplicates = true;
            //    config.NewestOnTop = true;
            //    config.ShowCloseButton = true;
            //    config.MaximumOpacity = 95;
            //    config.VisibleStateDuration = 5000;
                
            //});
            
            await builder.Build().RunAsync();
        }
    }
}
