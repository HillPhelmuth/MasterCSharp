using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazor.ModalDialog;
using BlazorApp.Shared;
using BlazorApp.Shared.ArenaChallenge;
using BlazorApp.Shared.CodeModels;
using Microsoft.AspNetCore.Components;

namespace BlazorApp.Client.Pages.ChallengeArena
{
    public partial class ActiveArenas : IDisposable
    {
        [Inject]
        public ArenaService ArenaService { get; set; }
        [Inject]
        public AppState AppState { get; set; }
        [Inject]
        protected IModalDialogService ModalService { get; set; }
        private List<Arena> OpenArenas { get; set; }
        private List<Arena> FullArenas { get; set; } = new List<Arena>();
        [Parameter]
        public Arena SelectedArena { get; set; }
        [Parameter]
        public bool CreatedArena { get; set; }
        protected Challenge SelectedChallenge { get; set; }
        private Arena NewArena { get; set; } = new Arena();
        private bool isCreateArena;

        protected override Task OnInitializedAsync()
        {
            OpenArenas = ArenaService.OpenArenas.ToList();
            FullArenas = ArenaService.ActiveArenas.Where(x => x.IsFull).ToList();
            ArenaService.OnArenasUpdate += UpdateArenas;
            return base.OnInitializedAsync();
        }

        private void JoinArena(Arena arena)
        {
            var userName = AppState.UserName;
            //ArenaService.JoinArena(arena.Name, userName);
            SelectedArena = arena;
            CreatedArena = false;
            var parameters = new ModalDialogParameters { { "SelectedArena", SelectedArena }, { "CreatedArena", CreatedArena } };
            ModalService.Close(true, parameters);
        }
        private void JoinArena(object arena)
        {
            var arenaType = (Arena)arena;
            var userName = AppState.UserName;
            //ArenaService.JoinArena(arenaType.Name, userName);
            SelectedArena = arenaType;
            CreatedArena = false;
            var parameters = new ModalDialogParameters { { "SelectedArena", SelectedArena }, { "CreatedArena", CreatedArena } };
            ModalService.Close(true, parameters);
        }
        private void CreateArena()
        {
            var userName = AppState.UserName;
            NewArena.Creator = userName;
            SelectedArena = NewArena;
            //ArenaService.CreateArena(NewArena);
            CreatedArena = true;
            var parameters = new ModalDialogParameters { { "SelectedArena", SelectedArena }, { "CreatedArena", CreatedArena } };
            ModalService.Close(true, parameters);
        }
        private async void ShowChallenges()
        {
            var result = await ModalService.ShowDialogAsync<ArenaChallenges>("Select a code challenge");
            if (result.Success)
                SelectedChallenge = result.ReturnParameters.Get<Challenge>("SelectedChallenge");
            NewArena.CurrentChallenge = SelectedChallenge;
            await InvokeAsync(StateHasChanged);
        }
        private void UpdateArenaLists()
        {
            OpenArenas = ArenaService.OpenArenas.ToList();
            FullArenas = ArenaService.ActiveArenas.Where(x => x.IsFull).ToList();
            StateHasChanged();
        }

        private Task UpdateArenas()
        {
            OpenArenas = ArenaService.OpenArenas.ToList();
            FullArenas = ArenaService.ActiveArenas.Where(x => x.IsFull).ToList();
            return InvokeAsync(StateHasChanged);
        }
        public void Dispose()
        {
            ArenaService.OnArenasChanged -= UpdateArenaLists;
            Console.WriteLine("ActiveArenas.razor is disposed");
        }
    }
}
