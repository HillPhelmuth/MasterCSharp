using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Blazor.ModalDialog;
using BlazorApp.Shared;
using BlazorApp.Shared.CodeModels;
using BlazorApp.Shared.CodeServices;
using BlazorApp.Shared.UserModels;
using BlazorMonaco;
using BlazorMonaco.Bridge;
using Microsoft.AspNetCore.Components;
using TextCopy;

namespace BlazorApp.Client.Pages.Challenges
{
    public partial class CodeChallengeHome : ComponentBase, IDisposable
    {
        [Inject]
        public CodeEditorService CodeEditorService { get; set; }
        [CascadingParameter(Name = nameof(AppStateService))]
        protected AppStateService AppStateService { get; set; }
        [Inject]
        public PublicClient PublicClient { get; set; }
        [Inject]
        public IModalDialogService ModalService { get; set; }

        public CodeChallenges CodeChallenges { get; set; }
        public Challenge SelectedChallenge { get; set; }
        public UserAppData UserAppData { get; set; }
        
        private string codeSnippet;
        private bool takeChallenge = false;
        private bool isCodeCompiling;
        private bool isChallengeSucceed;
        private bool isChallengeFail;
        private bool isChallengeReady;


        [Parameter]
        public EventCallback<int> OnNotReady { get; set; }

        protected override async Task OnInitializedAsync()
        {
            CodeChallenges = AppStateService.CodeChallenges ?? await PublicClient.GetChallenges();
            UserAppData = AppStateService.UserAppData;
            foreach (var challenge in CodeChallenges.Challenges)
            {
                Console.WriteLine($"user challenges found: {UserAppData?.ChallengeSuccessData}");
                if (UserAppData?.ChallengeSuccessIds?.Any(x => x == challenge.ID) ?? false)
                {
                    challenge.UserCompleted = true;
                }
            }
            AppStateService.CodeChallenges ??= CodeChallenges;
            AppStateService.PropertyChanged += UpdateUserChallenges;
            isChallengeReady = true;
        }

        private async void HandleSaveSnippet(string code)
        {
            await ModalService.ShowMessageBoxAsync("Save Fail", "If you want to save a code snippet, navigate to the 'Practice C#' tab");
        }
        private void SolveChallenge() => takeChallenge = !takeChallenge;

        public async Task SubmitCode(string code)
        {
            isCodeCompiling = true;
            StateHasChanged();
            await HandleCodeSubmit(code);
        }
        public async Task HandleCodeSubmit(string code)
        {
            var submitChallenge = new Challenge
            {
                Solution = code,
                Tests = SelectedChallenge.Tests
            };
            var output = await PublicClient.SubmitChallenge(submitChallenge);
            AppStateService.CodeOutput = output;
            foreach (var result in output.Outputs)
            {
                Console.WriteLine($"test: {result.TestIndex}, result: {result.TestResult}, output: {result.Codeout}");
            }
            isChallengeSucceed = output.Outputs.All(x => x.TestResult);
            var debugString = isChallengeSucceed ? "True" : "False";
            Console.WriteLine($"isChallengeSucceed = {debugString}");
            isChallengeFail = !isChallengeSucceed;
            Console.WriteLine($"isChallengeFail = {isChallengeFail}");
            isCodeCompiling = false;
            await InvokeAsync(StateHasChanged);
            if (isChallengeSucceed)
            {
                SelectedChallenge.UserCompleted = true;
                UserAppData.ChallengeSuccessIds.Add(SelectedChallenge.ID);
                await PublicClient.AddSuccessfulChallenge(AppStateService.UserName, SelectedChallenge.ID);
                AppStateService.UpdateUserAppData(UserAppData);
            }
            StateHasChanged();
        }

        protected async Task HandleChallengeChanged(Challenge challenge)
        {
            Console.WriteLine($"Challenge from handler: {challenge.Name}");
            SelectedChallenge = challenge;
            await UpdateSnippet();
            takeChallenge = false;
            StateHasChanged();
        }
        protected Task UpdateSnippet()
        {
            var challenge = SelectedChallenge;
            codeSnippet = challenge.Snippet;
            CodeEditorService.CodeSnippet = codeSnippet;
            return Task.CompletedTask;
        }

        protected Task ShowAnswer()
        {
            codeSnippet = SelectedChallenge.Solution;
            CodeEditorService.CodeSnippet = codeSnippet;
            return Task.CompletedTask;
        }

        protected void UpdateUserChallenges(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName != "CodeChallenges" && args.PropertyName != "UserAppData")
                return;
            CodeChallenges = AppStateService.CodeChallenges;
            UserAppData = AppStateService.UserAppData;
            StateHasChanged();
        }

        public void Dispose()
        {
            Console.WriteLine("CodeChallengeHome.razor Disposed");
            AppStateService.PropertyChanged -= UpdateUserChallenges;
        }
    }
}
