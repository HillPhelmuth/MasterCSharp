using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BlazorApp.Shared;
using BlazorApp.Shared.ArenaChallenge;
using BlazorApp.Shared.StaticAuth.Interfaces;
using BlazorApp.Shared.UserModels;
using MatBlazor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json.Linq;

namespace BlazorApp.Client.Pages.ChallengeArena
{
    public partial class ArenaHub : IDisposable
    {
        [Inject]
        public PublicClient PublicClient { get; set; }
        [Inject]
        public ArenaService ArenaService { get; set; }
        [Inject]
        public AppStateService AppStateService { get; set; }
        [Inject]
        protected IMatToaster Toaster { get; set; }
        [Inject]
        public ICustomAuthenticationStateProvider AuthenticationState { get; set; }
        [Parameter]
        public EventCallback<string> OnNewMessage { get; set; }
        [Parameter]
        public string UserName { get; set; }
        public bool IsConnected => hubConnection?.State == HubConnectionState.Connected;
        //local http://localhost:7071/api
        private const string FunctionBaseUrl = "https://csharpduelshubfunction.azurewebsites.net/api";
        private HubConnection hubConnection;
        private List<string> messages = new List<string>();
        private string messageInput;
        private bool showChat;
        
        protected override async Task OnInitializedAsync()
        {
            UserName = AppStateService.UserName;
            if (string.IsNullOrEmpty(UserName))
            {
                var auth = await AuthenticationState.GetAuthenticationStateAsync();
                UserName = auth?.User?.Identity?.Name;
            }
            //Temp for testing
            //var random = new Random();
            //UserName = $"{userName}{random.Next(1, 999)}";
            hubConnection = new HubConnectionBuilder()
                .WithUrl($"{FunctionBaseUrl}/", options =>
                {
                    options.Headers.Add("x-ms-client-principal-id", UserName);
                })
                .Build();

            hubConnection.On<object>("getAlert", (message) =>
            {
                var encodedMsg = $"{message}";
                Console.WriteLine($"received message: {encodedMsg}");
                Toaster.Add(message.ToString(), MatToastType.Primary);
                messages.Add(message.ToString());
                StateHasChanged();
            });
            hubConnection.On<object>("alertArena", async (message) =>
            {
                var encodedMsg = $"{message}";
                Console.WriteLine($"received Alert: {encodedMsg}");
                var activeArenas = await PublicClient.GetActiveArenas();
                ArenaService.UpdateArenas(activeArenas);
                StateHasChanged();

            });
            hubConnection.On<object>("leaveArena", (message) =>
            {
                var encodedMsg = $"{message}";
                Console.WriteLine($"received message: {encodedMsg}");
                messages.Add(encodedMsg);
                OnNewMessage.InvokeAsync(encodedMsg);
            });
            hubConnection.On<object>("resultAlert", (message) =>
            {
                Console.WriteLine($"'resultAlert' Triggered message: {message}");
                var result = ArenaResultMessage.FromJson(message.ToString());
                var arenaName = result.Group;
                ArenaService.ArenaComplete(arenaName);
                Console.WriteLine($"'ArenaComplete' values:name: {arenaName} winner {result.DuelWinner} loser: {result.DuelLoser}");
                Toaster.Add(result.Message, MatToastType.Primary);
                messages.Add(result.Message);
            });
            hubConnection.On<object>("resultActual", (arenaObj) =>
            {
                Console.WriteLine("'resultActual' Triggered");
                var encodedMsg = $"{arenaObj}";
                var arenaAlert = ArenaResultMessage.FromJson(arenaObj.ToString());
                messages.Add($"{arenaAlert.DuelWinner} defeated {arenaAlert.DuelLoser} in arena {arenaAlert.Group}");
                Console.WriteLine($"received group message: {encodedMsg}");
                StateHasChanged();
               
            });
            hubConnection.On<object>("joinAlert", (message) =>
            {
                var encodedMsg = $"{message}";
               
                var jObj = JObject.Parse(message.ToString());
                var arenaName = jObj["group"]?.ToString();
                var userJoin = jObj["user"]?.ToString();
                var arenaText = jObj["message"]?.ToString();
                Console.WriteLine($"'joinAlert' Triggered: {encodedMsg}");
                ArenaService.JoinArena(arenaName,userJoin);
                Toaster.Add(arenaText, MatToastType.Primary);
                StateHasChanged();
            });
            hubConnection.On<object>("showArena", group =>
            {
                Console.WriteLine($"Create arena message received: {group}");
                var jObj = JObject.Parse(group.ToString());

                var user = jObj["user"]?.ToString();
                var groupobj = jObj["group"]?.ToString();
                var chalObj = jObj["challenge"]?.ToString();
               Toaster.Add($"User {user} joined Arena: {groupobj} with Challenge: {chalObj}", MatToastType.Info);
               messages.Add($"User {user} joined Arena: {groupobj} with Challenge: {chalObj}");
                StateHasChanged();
            });
            
            await hubConnection.StartAsync();
            var arenasInit = await PublicClient.GetActiveArenas();
            ArenaService.UpdateArenas(arenasInit);
            
        }
        
       
      
        private async Task SendPublic()
        {
            await PublicClient.Client.PostAsJsonAsync($"{FunctionBaseUrl}/alerts/{UserName}", messageInput);
        }
        private async Task CreateArena(string addArena, string challngeName)
        {
            await PublicClient.Client.PostAsJsonAsync($"{FunctionBaseUrl}/create/{addArena}/{UserName}/{challngeName}", messageInput);
            await PublicClient.Client.PostAsJsonAsync($"{FunctionBaseUrl}/joinDuel/{addArena}/{UserName}", messageInput);
        }

        private async Task JoinArena(string arenaName)
        {
            await PublicClient.Client.PostAsJsonAsync($"{FunctionBaseUrl}/joinDuel/{arenaName}/{UserName}", messageInput);
            await PublicClient.Client.PostAsJsonAsync($"{FunctionBaseUrl}/joinAlert/{arenaName}/{UserName}", messageInput);
            
        }

        private async void HandleArenaSubmit((bool, string) dataTuple)
        {
            (bool isSuccess, string arenaName) = dataTuple;
            var message = isSuccess
                ? $"{UserName} sucessfully completed the challenge!"
                : $"{UserName} submitted an incorrect solution!";
            await PublicClient.Client.PostAsJsonAsync($"{FunctionBaseUrl}/duelAttempt/{arenaName}", message);
            if (!isSuccess) return;
            var arenaRemove = ArenaService.ActiveArenas.FirstOrDefault(x => x.Name == arenaName);
            if (arenaRemove != null) await PublicClient.DeleteActiveArena(arenaRemove);
        }

        private async void HandleArenaCreate(Arena arena)
        {
            var arenaObj = JObject.FromObject(arena);
            Console.WriteLine($"Arena from Modal: {arenaObj}");
            await CreateArena(arena.Name, arena.CurrentChallenge.Name);
            await PublicClient.CreateActiveArena(arena);
        }
        private async void HandleArenaJoin(Arena arena)
        {
            await JoinArena(arena.Name);
            arena.Opponent = UserName;
            await PublicClient.UpdateActiveArena(arena);
        }

        private async void RemoveArenas()
        {
            foreach (var arena in ArenaService.ActiveArenas.Where(x => x.Creator == UserName))
            {
                await PublicClient.DeleteActiveArena(arena);
            }
        }

        private void HandleLeaveArena(Arena arena)
        {
            _ = PublicClient.Client.PostAsJsonAsync($"{FunctionBaseUrl}/leaveDuel/{arena.Name}/{UserName}", arena);
            if (!arena.IsFull || arena.Creator == UserName)
                _ = PublicClient.DeleteActiveArena(arena);
            else
            {
                if (arena.Opponent != UserName) return;
                arena.Opponent = null;
                _ = PublicClient.UpdateActiveArena(arena);
            }
        }
        public void Dispose()
        {
            _ = hubConnection.DisposeAsync();
           
            Console.WriteLine("ArenaHub.razor disposed");
        }
    }
}
