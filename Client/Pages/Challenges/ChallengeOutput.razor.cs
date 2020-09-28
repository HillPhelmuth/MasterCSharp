using System;
using System.ComponentModel;
using System.Threading.Tasks;
using BlazorApp.Shared;
using BlazorApp.Shared.CodeModels;
using Microsoft.AspNetCore.Components;

namespace BlazorApp.Client.Pages.Challenges
{
    public partial class ChallengeOutput : IDisposable
    {
        [CascadingParameter(Name = nameof(AppStateService))]
        protected AppStateService AppStateService { get; set; }

        protected CodeOutputModel CodeOutput { get; set; } = new CodeOutputModel();

        protected override Task OnInitializedAsync()
        {
            CodeOutput = AppStateService.CodeOutput;
            AppStateService.PropertyChanged += UpdatePropertyState;
            return base.OnInitializedAsync();
        }

        private void UpdatePropertyState(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName != "CodeOutput") return;
            CodeOutput = AppStateService.CodeOutput;
        }
        public void Dispose() => AppStateService.PropertyChanged -= UpdatePropertyState;
    }
}
