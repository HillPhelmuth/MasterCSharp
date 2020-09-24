using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MasterSharpOpen.Shared;
using MasterSharpOpen.Shared.CodeServices;
using MasterSharpOpen.Shared.StaticAuth.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;

namespace MasterSharpOpen.Client.Pages.Interactive
{
    public partial class HubSource : IDisposable
    {
        [Inject]
        public AppStateService AppStateService { get; set; }
        [Inject]
        public PublicClient PublicClient { get; set; }
        [Inject]
        public CodeEditorService CodeEditorService { get; set; }
        [Inject]
        private ICustomAuthenticationStateProvider AuthProvider { get; set; }

        [Parameter]
        public string UserName { get; set; }
        [Parameter]
        public string OtherUser { get; set; }
        [Parameter]
        public string GroupName { get; set; }
        private const string FunctionBaseUrl = "https://csharprealtimefunction.azurewebsites.net/api";
        //private const string FunctionBaseUrl = "http://localhost:7071/api";
        private HubConnection hubConnection;
        private List<string> messages = new List<string>();
        private string userInput;
        private string messageInput;

        protected override async Task OnInitializedAsync()
        {
            //var authInfo = await AuthProvider.GetAuthenticationStateAsync();
            //var authUser = authInfo.User.Identity;

            hubConnection = new HubConnectionBuilder()
                .WithUrl($"{FunctionBaseUrl}/", options =>
                {
                    options.Headers.Add("x-ms-client-principal-id", UserName);
                })
                .Build();

            hubConnection.On<string>("newMessage", (message) =>
            {
                var encodedMsg = $"{message}";
                Console.WriteLine($"received message: {encodedMsg}");
                messages.Add(encodedMsg);
                StateHasChanged();
            });
            hubConnection.On<object>("groupMessage", (message) =>
            {
                var encodedMsg = $"{message}";
                Console.WriteLine($"received group message: {encodedMsg}");
                messages.Add(encodedMsg);

                StateHasChanged();
            });
            hubConnection.On<object>("newCode", code =>
            {
                Console.WriteLine($"code received: {code}");
                CodeEditorService.UpdateShardSnippet(code.ToString());
            });
            hubConnection.On<object>("privateMessage", message =>
            {
                Console.WriteLine($"private received: {message}");
                messages.Add(message.ToString());
                StateHasChanged();
            });
            await hubConnection.StartAsync();
            await Task.Delay(500);
            await PublicClient.Client.PostAsJsonAsync($"{FunctionBaseUrl}/messages", $"user {UserName} has joined");
            await JoinGroup();
        }
        private async Task Send()
        {
            var newObject = new { userInput, messageInput };
            var objString = JsonConvert.SerializeObject(newObject);
            await PublicClient.Client.PostAsJsonAsync($"{FunctionBaseUrl}/messages", objString);
        }

        private async void SendSnippet(string snippet)
        {
            await PublicClient.Client.PostAsJsonAsync($"{FunctionBaseUrl}/sendCode/{OtherUser}", snippet);
        }

        private async Task JoinGroup()
        {

            await PublicClient.Client.PostAsJsonAsync($"{FunctionBaseUrl}/joinGroup/{GroupName}/{UserName}", UserName);
        }

        private async Task MessageGroup()
        {
            await PublicClient.Client.PostAsJsonAsync($"{FunctionBaseUrl}/groupMessage/{GroupName}", messageInput);
        }

        private async void HandleKeyUp(KeyboardEventArgs args)
        {
            if (args.Key.ToUpper() == "ENTER" && args.AltKey) await MessageGroup();
            if (args.Key.ToUpper() == "ENTER") await Send();
        }
        public bool IsConnected => hubConnection?.State == HubConnectionState.Connected;

        public void Dispose()
        {
            _ = hubConnection.DisposeAsync();
        }
    }
}
