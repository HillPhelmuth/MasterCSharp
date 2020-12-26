using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Blazor.ModalDialog;
using BlazorApp.Client.ExtensionMethods;
using BlazorApp.Client.Pages.ShareCode;
using BlazorApp.Client.Shared;
using BlazorApp.Shared;
using BlazorApp.Shared.CodeServices;
using BlazorApp.Shared.UserModels;
using Microsoft.AspNetCore.Components;

namespace BlazorApp.Client.Pages.Practice
{
    public partial class CodeHome : IDisposable
    {
        [Inject]
        public CodeEditorService CodeEditorService { get; set; }
        [Inject]
        protected IModalDialogService ModalService { get; set; }
        [Inject]
        public PublicClient PublicClient { get; set; }
        [Inject]
        private PublicGithubClient GithubClient { get; set; }
        [CascadingParameter(Name = nameof(AppState))]
        protected AppState AppState { get; set; }
    
        private bool isCodeCompiling;
        private bool isConsoleOpen;
        private bool isMonacoOpen;
        private bool isSnippetSaving;
        private string codeOutput = "";
        private string codeSnippet;
        private string message;
        private string ReadlinePattern { get; } = "Console.ReadLine()";
        private string buttonCss = "";
        protected async Task UpdateCodeSnippet(string snippet, bool isConsole = false)
        {
            isConsoleOpen = isConsole;
            CodeEditorService.CodeSnippet = snippet;
            StateHasChanged();
            await Task.Delay(50);
            isMonacoOpen = true;
            codeSnippet = snippet;
            StateHasChanged();
        }

        private void HandleSaveSnippet(string snippet)
        {
            Console.WriteLine("Handle Save Snippet");
            isSnippetSaving = true;
            StateHasChanged();
            _ = SaveUserSnippet(snippet);
        }

        private async Task SaveUserSnippet(string snippet)
        {
            if (!AppState.HasUser)
            {
                var result = await ModalService.ShowDialogAsync<LoginProvider>("Sign-in to Save");
                if (!result.Success)
                    return;
            }
            var inputForm = new ModalDataInputForm("Save User Snippet", "what should we call this code snippet?");
            var snippetField = inputForm.AddStringField("Name", "Snippet Name", "");
            string snippetName = "";
            var options = new ModalDialogOptions()
            {
                Style = "small-modal"
            };
            if (await inputForm.ShowAsync(ModalService, options))
            {
                snippetName = snippetField.Value;
            }
            var newSnippet = new UserSnippet
            {
                Name = snippetName,
                Snippet = snippet
            };
            var userData = AppState.UserAppData;
            userData.Snippets.Add(newSnippet);
            AppState.UpdateUserAppData(userData);
            var requestResult = await PublicClient.AddUserSnippet(AppState.UserName, newSnippet);
            isSnippetSaving = false;
            message = requestResult ? $"Successfully saved snippet: {snippetName}" : "Save snippet failed";
            StateHasChanged();
        }
        protected void HandeSubmit(string input)
        {
            Console.WriteLine("Handle Submit");
            isCodeCompiling = true;
            StateHasChanged();
            _ = OnSubmit(input);
        }
        protected async Task OnSubmit(string codeInput)
        {
            Console.WriteLine("On Submit");

            string result;
            var sw = new Stopwatch();
            if (codeInput.Contains(ReadlinePattern))
            {
                string code = await ReplaceConsoleInput(codeInput);
                sw.Start();
                result = await PublicClient.SubmitConsole(code);

                codeOutput += $"<p>{result}</p>";
                CodeEditorService.CurrentOutput = codeOutput;
                sw.Stop();
                Console.WriteLine($"console function: {sw.ElapsedMilliseconds}ms");
                isCodeCompiling = false;
                buttonCss = "alert_output";
                StateHasChanged();
                return;
            }
            if (!isConsoleOpen)
            {
                sw.Start();
                result = await PublicClient.SubmitCode(codeInput);
                codeOutput += $"<p>{result}</p>";
                CodeEditorService.CurrentOutput = codeOutput;
                sw.Stop();
                Console.WriteLine($"console function: {sw.ElapsedMilliseconds}ms");
                isCodeCompiling = false;
                buttonCss = "alert_output";
                StateHasChanged();
                return;
            }
            sw.Start();
            result = await PublicClient.SubmitConsole(codeInput);
            codeOutput += $"<p>{result}</p>";
            CodeEditorService.CurrentOutput = codeOutput;
            sw.Stop();
            Console.WriteLine($"console function: {sw.ElapsedMilliseconds}ms");
            isCodeCompiling = false;
            buttonCss = "alert_output";
            StateHasChanged();
        }

        private async Task<string> ReplaceConsoleInput(string codeInput)
        {
            var tempCode = codeInput;
            var inputDictionary = new Dictionary<int, DataInputFormStringField>();
            var readLineIndexes = tempCode.AllIndexesOf(ReadlinePattern);
            var regex = new Regex(Regex.Escape(ReadlinePattern));
            var inputForm = new ModalDataInputForm("User Inputs", "User console input");

            for (int i = 1; i <= readLineIndexes.Count(); i++)
            {
                string userInput = "";
                var inputField1 =
                    inputForm.AddStringField($"Input{i}", $"{ReadlinePattern} {i}", userInput, "The user's input.");
                inputDictionary.Add(i, inputField1);
            }

            if (await inputForm.ShowAsync(ModalService))
            {
                int j = 1;
                tempCode = regex.Replace(tempCode, m =>
                {
                    var input = inputDictionary[j].Value;
                    Console.WriteLine($"Console.ReadLine() replaced with \"{input}\"");
                    j++;
                    return $"\"{input}\"";
                });
            }

            var code = tempCode;
            return code;
        }

        private void ClearOutput()
        {
            codeOutput = "";
            StateHasChanged();
        }

        public async Task GetCodeFromGitHubFile(string filename = "", bool isFromPublic = false)
        {
            var sw = new Stopwatch();
            sw.Start();
            string code = isFromPublic ? await UpdateFromPublicRepo() : await GithubClient.CodeFromGithub(filename);
            sw.Stop();
            Console.WriteLine($"GitHub file content retrieved in {sw.ElapsedMilliseconds}ms \r\n Returned: {code}");
            await UpdateCodeSnippet(code);
        }
        protected async Task<string> UpdateFromPublicRepo()
        {
            var option = new ModalDialogOptions
            {
                Style = "modal-dialog-githubform"
            };
            var result = await ModalService.ShowDialogAsync<GitHubForm>("Get code from a public Github Repo", option);
            if (!result.Success) return null;
            string code = result.ReturnParameters.Get<string>("FileCode");
            return code;

        }
        public async Task DisplayCodeDescription(string content, Dictionary<string, string> resourceUrls)
        {
            var parameters = new ModalDialogParameters
            {
                {"Description", content},{"ResourceUrls", resourceUrls}
            };
            await ModalService.ShowDialogAsync<CodeDescription>("More about this section", parameters: parameters);
        }

        public async Task DisplayCodeOutput()
        {
            buttonCss = "";
            await InvokeAsync(StateHasChanged);
            var content = CodeEditorService.CurrentOutput;
            var parameters = new ModalDialogParameters
            {
                {"CodeOutput", content}
            };
            await ModalService.ShowDialogAsync<CodeOutModal>("Current code output is displayed here.",parameters: parameters);
        }
        public void Dispose()
        {
            Console.WriteLine("CodeHome.razor Disposed");
            //CodeEditorService.OnChange -= StateHasChanged;
        }
    }
    
}
