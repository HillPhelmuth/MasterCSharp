using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BlazorApp.Shared.CodeServices;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace BlazorApp.Client.Pages.ShareCode
{
    public partial class DiffShareHub : IDisposable
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
        [Parameter]
        public EventCallback<string> OnNewMessage { get; set; }

        private const string FunctionBaseUrl = "api";
        //private const string FunctionBaseUrl = "http://localhost:7071/api";
        private HubConnection hubConnection;
        private List<string> messages = new List<string>();
        private string messageInput;
        private bool isCodeCompiling;
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

            hubConnection.On<object>("newMessage", (message) =>
            {
                var encodedMsg = $"{message}";
                Console.WriteLine($"received message: {encodedMsg}");
                messages.Add(encodedMsg);
                OnNewMessage.InvokeAsync(encodedMsg);
            });
            hubConnection.On<object>("groupMessage", (message) =>
            {
                var encodedMsg = $"{message}";
                Console.WriteLine($"received group message: {encodedMsg}");
                messages.Add(encodedMsg);
                OnNewMessage.InvokeAsync(encodedMsg);
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
                OnNewMessage.InvokeAsync($"{message}");
            });
            hubConnection.On<object>("newOut", output =>
            {
                Console.WriteLine($"Output: {output}");
                OnNewMessage.InvokeAsync($"CODE OUTPUT::{output}");
            });
            await hubConnection.StartAsync();

            await JoinGroup();
        }

        private async Task Send()
        {
            await PublicClient.Client.PostAsJsonAsync($"{FunctionBaseUrl}/messages/{UserName}", messageInput);
        }

        private async void SendSnippet(string snippet)
        {
            await PublicClient.Client.PostAsJsonAsync($"{FunctionBaseUrl}/sendCode/{OtherUser}", snippet);
        }

        private async Task JoinGroup()
        {

            await PublicClient.Client.PostAsJsonAsync($"{FunctionBaseUrl}/joinGroup/{GroupName}/{UserName}", UserName);
        }

        private async void MessageGroup()
        {
            await PublicClient.Client.PostAsJsonAsync($"{FunctionBaseUrl}/groupMessage/{GroupName}", messageInput);
        }
        private async void SubmitCode(string code)
        {
            isCodeCompiling = true;
            await InvokeAsync(StateHasChanged);
            var output = await PublicClient.SubmitCode(code);
            await PublicClient.Client.PostAsJsonAsync($"{FunctionBaseUrl}/sendOut/{GroupName}", output);
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
