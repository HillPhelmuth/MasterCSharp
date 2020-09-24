using System;
using System.Threading.Tasks;
using BlazorApp.Shared;
using BlazorApp.Shared.CodeModels;
using Microsoft.AspNetCore.Components;

namespace BlazorApp.Client.Pages.Challenges
{
    public partial class ChallengeOutput : IDisposable
    {
        [Inject]
        public AppStateService AppStateService { get; set; }

        protected CodeOutputModel CodeOutput => AppStateService.CodeOutput;

        protected override Task OnInitializedAsync()
        {
            AppStateService.OnChange += StateHasChanged;
            return base.OnInitializedAsync();
        }

        public void Dispose()
        {
            AppStateService.OnChange -= StateHasChanged;
        }
    }
}
