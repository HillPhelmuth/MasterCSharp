using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BlazorApp.Shared.CodeServices;
using BlazorApp.Shared.StaticAuth.Interfaces;
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
        [Inject]
        private ICustomAuthenticationStateProvider AuthProvider { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }
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
        
        //private const string FunctionBaseUrl = "http://localhost:7071/api"; or https://csharprealtimefunction.azurewebsites.net/api
        private HubConnection hubConnection;
        private List<string> messages = new List<string>();
        private string messageInput;
        private bool isCodeCompiling;
        protected override async Task OnInitializedAsync()
        {
            var authInfo = await AuthProvider.GetAuthenticationStateAsync();
            var authUser = authInfo.User.Identity;
            UserName ??= authUser.Name;
            var hubUri = $"{NavigationManager.BaseUri}api";
            if (NavigationManager.BaseUri.Contains("localhost"))
                hubUri = "http://localhost:7071/api";
            //must use API port for local host: "http://localhost:7071/api";
            hubConnection = new HubConnectionBuilder()
                .WithUrl($"{hubUri}/", options =>
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
                CodeEditorService.CodeSnippet = code.ToString();
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

        private async Task Send() => await PublicClient.SendMessage(UserName, messageInput);

        private async void SendSnippet(string snippet) => await PublicClient.SendSnippet(snippet, OtherUser);

        private async Task JoinGroup() => await PublicClient.JoinGroup(GroupName, UserName);

        private async void MessageGroup() => await PublicClient.MessageGroup(GroupName, messageInput);

        private async void SubmitCode(string code)
        {
            isCodeCompiling = true;
            await InvokeAsync(StateHasChanged);
            var output = await PublicClient.SubmitCode(code);
            await PublicClient.SendCodeOutput(GroupName, output);
            isCodeCompiling = false;
            await InvokeAsync(StateHasChanged);

        }
        public bool IsConnected => hubConnection?.State == HubConnectionState.Connected;

        public void Dispose() => _ = hubConnection.DisposeAsync();
    }
}
