using System;
using System.Linq;
using System.Threading.Tasks;
using BlazorMonaco;
using BlazorMonaco.Bridge;
using MasterSharpOpen.Shared;
using MasterSharpOpen.Shared.CodeModels;
using MasterSharpOpen.Shared.CodeServices;
using MasterSharpOpen.Shared.UserModels;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using TextCopy;

namespace MasterSharpOpen.Client.Pages.Challenges
{
    public partial class CodeChallengeHome : ComponentBase, IDisposable
    {
        [Inject]
        public CodeEditorService CodeEditorService { get; set; }
        [Inject]
        public AppStateService AppStateService { get; set; }
        [Inject]
        public IClipboard Clipboard { get; set; }
        [Inject]
        public PublicClient PublicClient { get; set; }

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
            CodeChallenges ??= await PublicClient.GetChallenges();
            UserAppData = AppStateService.UserAppData;
            foreach (var challenge in CodeChallenges.Challenges)
            {
                Console.WriteLine($"user challenges found: {UserAppData?.ChallengeSuccessData}");
                if (UserAppData?.ChallengeSuccessIds?.Any(x => x == challenge.ID) ?? false)
                {
                    challenge.UserCompleted = true;
                }
            }

            AppStateService.SetCodeChallenges(CodeChallenges);
            AppStateService.OnChange += UpdateUserChallenges;
           
            isChallengeReady = true;
        }

        private void SolveChallenge() => takeChallenge = !takeChallenge;

        public async Task SubmitCode()
        {
            isCodeCompiling = true;
            StateHasChanged();
            await Task.Run(() =>
            {
                _ = HandleCodeSubmit();
            });
            
        }
        public async Task HandleCodeSubmit()
        {
            var code = await Editor.GetValue();

            var submitChallenge = new Challenge
            {
                Solution = code,
                Tests = SelectedChallenge.Tests
            };
            var output = await PublicClient.SubmitChallenge(submitChallenge);
            AppStateService.UpdateCodeOutput(output);
            foreach (var result in output.Outputs)
            {
                Console.WriteLine($"test: {result.TestIndex}, result: {result.TestResult}, output: {result.Codeout}");
            }
            isChallengeSucceed = output.Outputs.All(x => x.TestResult);
            isChallengeFail = !isChallengeSucceed;
            isCodeCompiling = false;
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
            CodeEditorService.UpdateSnippet(codeSnippet);
            Editor.SetValue(codeSnippet);
            return Task.CompletedTask;
        }

        protected Task ShowAnswer()
        {
            codeSnippet = SelectedChallenge.Solution;
            Editor.SetValue(codeSnippet);
            CodeEditorService.UpdateSnippet(codeSnippet);
            return Task.CompletedTask;
        }

        public async Task CopyCodeToClipboard()
        {
            var snippetClip = await Editor.GetValue();
            await Clipboard.SetTextAsync(snippetClip);
        }

        public async Task ReadCodeFromClipboard()
        {
            var content = await Clipboard.GetTextAsync();
            await Editor.SetValue(content);
            StateHasChanged();
        }
        protected void UpdateUserChallenges()
        {
            CodeChallenges = AppStateService.CodeChallenges;
            UserAppData = AppStateService.UserAppData;
            StateHasChanged();
        }

        #region Monaco Editor

        // Monaco Editor Settings
        protected MonacoEditor Editor { get; set; }
        protected StandaloneEditorConstructionOptions EditorOptionsPuzzle(MonacoEditor editor)
        {
            return new StandaloneEditorConstructionOptions
            {
                AutomaticLayout = true,
                AutoIndent = true,
                HighlightActiveIndentGuide = true,
                Language = "csharp",
                Value = codeSnippet ?? "private string MyProgram() \n" +
                    "{\n" +
                    "    string input = \"this does not\"; \n" +
                    "    string modify = input + \" suck!\"; \n" +
                    "    return modify;\n" +
                    "}\n" +
                    "return MyProgram();"
            };
        }

        protected async Task EditorOnDidInit(MonacoEditorBase editor)
        {
            await Editor.AddCommand((int)KeyMode.CtrlCmd | (int)KeyCode.KEY_H, (editor, keyCode) =>
            {
                Console.WriteLine("Ctrl+H : Initial editor command is triggered.");
            });
        }

        protected void OnContextMenu(EditorMouseEvent eventArg)
        {
            Console.WriteLine("OnContextMenu : " + System.Text.Json.JsonSerializer.Serialize(eventArg));
        }

        #endregion
        public void Dispose()
        {
            Console.WriteLine("CodeChallengeHome.razor Disposed");
            AppStateService.OnChange -= UpdateUserChallenges;
        }
    }
}
