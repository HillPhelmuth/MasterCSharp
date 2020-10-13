using System;
using System.Net.Http;
using System.Threading.Tasks;
using Blazor.ModalDialog;
using MasterCSharp.Shared.CodeServices;
using MasterCSharp.Shared.ExtensionMethods;
using MasterCSharp.Shared.StaticAuth;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using TextCopy;

namespace MasterCSharp.Client
{
   
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");
            var baseAddress = builder.Configuration["BaseAddress"] ?? builder.HostEnvironment.BaseAddress;
            var clientAddress = builder.Configuration["ClientAddress"] ?? builder.HostEnvironment.BaseAddress;
            builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(baseAddress) });
            builder.Services.AddTransient(sp => new HttpClient { BaseAddress = new Uri(baseAddress) }); 
            builder.Services.AddHttpClient<PublicClient>(clnt => clnt.BaseAddress = new Uri(baseAddress));
            builder.Services.AddHttpClient<PublicGithubClient>(clnt =>
                clnt.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));
            builder.Services.AddHttpClient<BrowserClient>(clnt => clnt.BaseAddress = new Uri(clientAddress));
            builder.Services.AddAuthentication();
            builder.Services.AddMasterSharpServices();
            builder.Services.InjectClipboard();
            builder.Services.AddModalDialog();
            builder.Services.AddScoped<RazorCompile>();
            //builder.Services.AddScoped<CompilationService>();
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
