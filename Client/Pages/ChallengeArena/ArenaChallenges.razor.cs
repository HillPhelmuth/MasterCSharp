using System.Threading.Tasks;
using Blazor.ModalDialog;
using BlazorApp.Shared;
using BlazorApp.Shared.CodeModels;
using Microsoft.AspNetCore.Components;

namespace BlazorApp.Client.Pages.ChallengeArena
{
    public partial class ArenaChallenges
    {
        [Inject]
        protected AppStateService AppStateService { get; set; }
        [Inject]
        protected PublicClient PublicClient { get; set; }
        [Inject]
        protected IModalDialogService ModalService { get; set; }
        [Parameter]
        public Challenge SelectedChallenge { get; set; }
        
        protected CodeChallenges CodeChallenges { get; set; }


        protected override async Task OnInitializedAsync()
        {
            CodeChallenges = AppStateService?.CodeChallenges ?? await PublicClient.GetChallenges();
        }

        protected void SelectChallenge(Challenge challenge)
        {
            var parameters = new ModalDialogParameters();
            SelectedChallenge = challenge;
            parameters.Add("SelectedChallenge", challenge); 
            ModalService.Close(true, parameters);
        }
    }
}
