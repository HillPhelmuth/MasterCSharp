using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;
using MasterSharpOpen.Shared.CodeServices;
using MasterSharpOpen.Shared.CodeShareModels;
using MasterSharpOpen.Shared.StaticAuth.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http.Connections.Client;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;

namespace MasterSharpOpen.Client.Pages
{
    public partial class TestChat : IDisposable
    {
        [Inject]
        public PublicClient PublicClient { get; set; }
        [Inject]
        public CodeEditorService CodeEditorService { get; set; }
        //[Inject]
        //private ICustomAuthenticationStateProvider AuthProvider { get; set; }

        [Parameter]
        public string UserName { get; set; }
        [Parameter]
        public string OtherUser { get; set; }
        [Parameter]
        public string GroupName { get; set; }
        [Parameter]
        public string Snippet { get; set; }

        private HubConnection hubConnection;
        private List<string> messages = new List<string>();
        private string userInput;
        private string messageInput;
        private bool isCodeReady;
        private bool isCodeCompiling;
        protected override async Task OnInitializedAsync()
        {
            //var authInfo = await AuthProvider.GetAuthenticationStateAsync();
            //var authUser = authInfo.User.Identity;

            hubConnection = new HubConnectionBuilder()
                .WithUrl("http://localhost:7071/api/", options =>
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
            });
            await hubConnection.StartAsync();

            await JoinGroup();
        }

        private async Task Send()
        {

            var newObject = new { userInput, messageInput };
            var objString = JsonConvert.SerializeObject(newObject);
            await PublicClient.Client.PostAsJsonAsync("http://localhost:7071/api/messages", objString);
        }

        private async void SendSnippet(string snippet)
        {
            await PublicClient.Client.PostAsJsonAsync($"http://localhost:7071/api/sendCode/{OtherUser}", snippet);
        }

        private async Task JoinGroup()
        {

            await PublicClient.Client.PostAsJsonAsync($"http://localhost:7071/api/joinGroup/{GroupName}/{UserName}", UserName);
        }

        private async void MessageGroup()
        {
            await PublicClient.Client.PostAsJsonAsync($"http://localhost:7071/api/groupMessage/{GroupName}", messageInput);
        }
        private async void SubmitCode(string code)
        {
            isCodeCompiling = true;
            await InvokeAsync(StateHasChanged);
            var output = await PublicClient.SubmitCode(code);
            var outputkey = "Code Output:";
            var newObject = new { outputkey, output };
            var objString = JsonConvert.SerializeObject(newObject);
            await PublicClient.Client.PostAsJsonAsync("http://localhost:7071/api/messages", objString);
            isCodeCompiling = false;
            await InvokeAsync(StateHasChanged);

        }
        public bool IsConnected =>
            hubConnection?.State == HubConnectionState.Connected;

        public void Dispose()
        {
            _ = hubConnection.DisposeAsync();
        }
    }
}
