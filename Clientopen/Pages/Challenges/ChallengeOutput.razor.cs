using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MasterSharpOpen.Shared;
using MasterSharpOpen.Shared.CodeModels;
using Microsoft.AspNetCore.Components;

namespace MasterSharpOpen.Client.Pages.Challenges
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
