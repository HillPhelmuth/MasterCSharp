using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazor.ModalDialog;
using BlazorMonaco;
using BlazorMonaco.Bridge;
using MasterSharpOpen.Client.Pages.Challenges;
using MasterSharpOpen.Shared;
using MasterSharpOpen.Shared.ArenaChallenge;
using MasterSharpOpen.Shared.CodeModels;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;

namespace MasterSharpOpen.Client.Pages.ChallengeArena
{
    public partial class TheArena: IDisposable
    {
        [Inject]
        public PublicClient PublicClient { get; set; }
        [Inject]
        protected IModalDialogService ModalService { get; set; }
        [Inject]
        protected ArenaService ArenaService { get; set; }
        [Inject]
        protected AppStateService AppStateService { get; set; }
        [Inject]
        protected NavigationManager NavigationManager { get; set; }
        public Arena CurrentArena { get; set; }
        protected Challenge SelectedChallenge { get; set; }
        [Parameter]
        public EventCallback<(bool, string)> OnChallengeSubmit { get; set; }
        [Parameter]
        public string UserName { get; set; }
        [Parameter]
        public EventCallback<Arena> OnArenaCreate { get; set; }
        [Parameter]
        public EventCallback<Arena> OnArenaJoin { get; set; }
        [Parameter]
        public EventCallback<Arena> OnLeaveArena { get; set; }
        public bool isArenaComplete { get; set; }
        private bool isChallengeSucceed;
        private bool isChallengeFail;
        private bool isChallengeReady;
        private bool isCodeCompiling;
        private string lostToUser;
      

        protected override Task OnInitializedAsync()
        {
            ArenaService.OnArenaCompleted += HandleArenaComplete;
            ArenaService.OnArenasUpdate += HandleUpdateArena;
            return base.OnInitializedAsync();
        }
        private Task HandleUpdateArena()
        {
            var arenasInit = ArenaService.ActiveArenas;
            var matchedArena = arenasInit.FirstOrDefault(x => x.Name == CurrentArena?.Name);
            
            CurrentArena = matchedArena;
            return InvokeAsync(StateHasChanged);

        }
        private async Task HandleSubmit(string code)
        {
            Console.WriteLine($"code handled: {code}");
            isCodeCompiling = true;
            StateHasChanged();
            var selectedChallenge = CurrentArena.CurrentChallenge;
            var debugChallenge = JsonConvert.SerializeObject(selectedChallenge);
            Console.WriteLine($"debug challenge: {selectedChallenge}");
            var submitChallenge = new Challenge
            {
                Solution = code,
                Tests = selectedChallenge.Tests
            };
            var output = await PublicClient.SubmitChallenge(submitChallenge);
            isChallengeSucceed = output.Outputs.All(x => x.TestResult);
            await OnChallengeSubmit.InvokeAsync((isChallengeSucceed, CurrentArena.Name));
            if (isChallengeSucceed)
            {
               var result = await PublicClient.AddCompleteDuel(CurrentArena, true);
            }
            isChallengeFail = !isChallengeSucceed;
            isCodeCompiling = false;
        }

        private async void ShowArenas()
        {
            bool isCreate;
            var options = new ModalDialogOptions
            {
                Style = "modal-dialog-arena"
            };
            var result = await ModalService.ShowDialogAsync<ActiveArenas>("Select an Arena", options);
            if (result.Success)
            {
                CurrentArena = result.ReturnParameters.Get<Arena>("SelectedArena");
                isCreate = result.ReturnParameters.Get<bool>("CreatedArena");
                if (isCreate)
                    await OnArenaCreate.InvokeAsync(CurrentArena);
                else
                    await OnArenaJoin.InvokeAsync(CurrentArena);
            }

            CurrentArena.CurrentChallenge ??= SelectedChallenge;
            await InvokeAsync(StateHasChanged);
        }

        private async void ShowChallengeInfo()
        {
            var challenge = CurrentArena?.CurrentChallenge ?? SelectedChallenge;
            var options = new ModalDialogOptions
            {
                Style = "modal-dialog-challengeInfo"
            };
            var parameters = new ModalDialogParameters
            {
                {"description",challenge.Description},{"examples", challenge.Examples}
            };
            var result = await ModalService.ShowDialogAsync<ChallengeInfo>("Challenge Info", options, parameters);
        }

        private async Task HandleArenaComplete(string name)
        {
            Console.WriteLine($"HandleArenaComplete({name}) triggered");
            if (CurrentArena.Name != name) return;
            if (!isChallengeSucceed)
            {
                isChallengeFail = true;
                lostToUser = UserName == CurrentArena.Creator ? CurrentArena.Opponent : CurrentArena.Creator;
            }

            await InvokeAsync(StateHasChanged);
            await Task.Delay(2000);
            CurrentArena = null;
            Console.WriteLine($"HandleArenaComplete({name}) completed");
            await InvokeAsync(StateHasChanged);

        }
        
        private void GoToChallenges()
        {
            AppStateService.UpdateTabNavigation(1);
            NavigationManager.NavigateTo("/");
        }

        private async void GoHome()
        {
            CurrentArena ??= new Arena();
            await OnLeaveArena.InvokeAsync(CurrentArena);
            await Task.Delay(1000);
            NavigationManager.NavigateTo("/");
        }

        public void Dispose()
        {
            ArenaService.OnArenasUpdate -= HandleUpdateArena;
            ArenaService.OnArenaCompleted -= HandleArenaComplete;
        }
    }
}
