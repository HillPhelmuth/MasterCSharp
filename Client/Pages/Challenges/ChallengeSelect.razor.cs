using System;
using System.Threading.Tasks;
using MasterCSharp.Shared.CodeModels;
using Microsoft.AspNetCore.Components;

namespace MasterCSharp.Client.Pages.Challenges
{
    public partial class ChallengeSelect
    {
        private string description;
        private string examples;
        private bool isChallengeSelected;
        public Challenge selectedChallenge { get; set; }
        [Parameter]
        public CodeChallenges CodeChallenges { get; set; }
        [Parameter]
        public EventCallback<Challenge> OnChallengeChanged { get; set; }

        protected Task SelectChallenge(Challenge challenge)
        {
            selectedChallenge = challenge;
            var challengeName = challenge.Name;
            description = selectedChallenge.Description;
            examples = selectedChallenge.Examples;
            isChallengeSelected = true;
            OnChallengeChanged.InvokeAsync(selectedChallenge);
            Console.WriteLine($"Puzzle selected: {selectedChallenge.Name}");
            StateHasChanged();
            return Task.CompletedTask;
        }
    }
}
