using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http.Json;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Blazor.ModalDialog;
using BlazorApp.Client.ExtensionMethods;
using BlazorApp.Client.Pages.RazorProject;
using BlazorApp.Client.Pages.ShareCode;
using BlazorApp.Shared.CodeModels;
using BlazorApp.Shared.CodeServices;

using BlazorApp.Shared.RazorCompileService;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Assembly = System.Reflection.Assembly;
using JsonSerializer = System.Text.Json.JsonSerializer;

//using Newtonsoft.Json;

namespace BlazorApp.Client.Pages.Practice
{
    public partial class RazorCodeHome : IDisposable
    {
        [Inject]
        public CodeEditorService CodeEditorService { get; set; }
        [Inject]
        public PublicClient PublicClient { get; set; }
        [Inject]
        public IJSRuntime JsRuntime { get; set; }
        [Inject]
        public RazorCompile RazorCompile { get; set; }
        [Inject]
        public PublicGithubClient GithubClient { get; set; }
        [Inject]
        protected IModalDialogService ModalService { get; set; }

        private const string MainComponentCodePrefix = "@page \"/__razorOutput\"\n";
        private const string MainUserPagePath = "/__razorOutput";
        public const string MainComponentFilePath = "__RazorOutput.razor";
        public List<CodeFile> Files { get; set; } = new List<CodeFile>();
        private DotNetObjectReference<RazorCodeHome> dotNetInstance;
        public RenderFragment ResultFragment { get; set; }
        private string language = "razor";
        private bool isready;
        private static string sampleSnippet = CodeSnippets.RazorSnippets["RazorSample"];
        public string InputSnippet { get; set; }
        private string srcString;
        private IDictionary<string, CodeFile> CodeFiles { get; set; } = new Dictionary<string, CodeFile>();
        private string Output;
        private List<string> Diagnostics { get; set; } = new List<string>();
        private bool isCodeCompiling;
        protected override async Task OnInitializedAsync()
        {
            CodeEditorService.CodeSnippet = sampleSnippet;
            var mainCodeFile = new CodeFile { Path = MainComponentFilePath, Content = MainComponentCodePrefix + sampleSnippet };
            //CodeFiles.Add(MainComponentFilePath, mainCodeFile);
            CodeEditorService.ActiveCodeFile = mainCodeFile;
            CodeEditorService.SaveCode(mainCodeFile);
            CodeEditorService.PropertyChanged += HandlePropertyChanged;
            await Task.Delay(100);
            isready = true;
            await base.OnInitializedAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                CodeEditorService.CodeSnippet = sampleSnippet;
                dotNetInstance = DotNetObjectReference.Create(this);

                await JsRuntime.InvokeVoidAsync("App.Repl.init", dotNetInstance);
            }
            await base.OnAfterRenderAsync(firstRender);
        }

        protected async void HandleExecuteProject(string input)
        {
            isCodeCompiling = true;
            StateHasChanged();
            await Task.Delay(100);
            InputSnippet = input;
            HandleSave(input);
            await ExecuteProject();
            isCodeCompiling = false;
            await InvokeAsync(StateHasChanged);
        }

        protected async void HandleSave(string content)
        {
            CodeEditorService.ActiveCodeFile.Content = content;
            var currentFile = CodeEditorService.ActiveCodeFile;
            currentFile.Content = content;
            CodeEditorService.SaveCode(currentFile);

        }
        protected async Task AddCodeFile()
        {
            var inputForm = new ModalDataInputForm("Create new conponent", "what should we call this component?");
            var snippetField = inputForm.AddStringField("Name", "Component Name", "");
            string filename = string.Empty;
            var options = new ModalDialogOptions()
            {
                Style = "small-modal"
            };
            if (await inputForm.ShowAsync(ModalService, options))
            {
                filename = snippetField.Value;
            }

            if (string.IsNullOrEmpty(filename)) return;
            var newCodeFile = new CodeFile();
            if (!filename.Contains(".razor"))
            {
                newCodeFile.Path = $"{filename}.razor";
            }
            sampleSnippet = $"<h1>{filename}</h1>";
            CodeEditorService.CodeSnippet = sampleSnippet;
            newCodeFile.Content = sampleSnippet;
            CodeEditorService.ActiveCodeFile = newCodeFile;
            await InvokeAsync(StateHasChanged);
        }

        protected async void SelectActiveFile()
        {
            var result = await ModalService.ShowDialogAsync<CodeFileModal>("Select a code snippet");
            if (result.Success)
            {
                var snippet = result.ReturnParameters.Get<CodeFile>("ActiveCodeFile");
                CodeEditorService.ActiveCodeFile = snippet;
                CodeEditorService.CodeSnippet = snippet.Content;
                
            }

            await InvokeAsync(StateHasChanged);
        }

        protected async void SelectSampleSnippet()
        {
            var result = await ModalService.ShowDialogAsync<RazorSamplesModal>("Select a code snippet");
            if (result.Success)
            {
                var snippet = result.ReturnParameters.Get<CodeFile>("ActiveCodeFile");
                CodeEditorService.ActiveCodeFile = snippet;
                CodeEditorService.CodeSnippet = snippet.Content;
            }

            await InvokeAsync(StateHasChanged);
        }
        protected async Task ExecuteProject()
        {
            isCodeCompiling = true;
            StateHasChanged();
            CodeAssemblyModel compilationResult = null;
            CodeFile mainComponent = null;
            string originalMainComponentContent = null;
            originalMainComponentContent = CodeEditorService.CodeFiles
                .FirstOrDefault(x => x.Path == CoreConstants.MainComponentFilePath)
                ?.Content ?? CoreConstants.MainComponentDefaultFileContent;
            var codeFiles = CodeEditorService.CodeFiles.PagifyMainComponent();
            
            compilationResult = await RazorCompile.CompileToAssemblyAsync(codeFiles);
            Diagnostics.AddRange(compilationResult?.Diagnostics?.Select(x => x.ToString()) ?? new List<string> { "None" });
           
            if (compilationResult?.AssemblyBytes?.Length > 0)
            {
                await JsRuntime.InvokeVoidAsync("App.Repl.updateUserAssemblyInCacheStorage", compilationResult.AssemblyBytes);

                await JsRuntime.InvokeVoidAsync("App.reloadIFrame", "user-page-window", MainUserPagePath);
            }

            isCodeCompiling = false;
            CodeEditorService.CodeFiles = codeFiles.UnPagifyMainComponent(originalMainComponentContent);
            await InvokeAsync(StateHasChanged);
            
        }
        protected async Task UpdateFromPublicRepo()
        {
            var option = new ModalDialogOptions
            {
                Style = "modal-dialog-githubform"
            };
            var result = await ModalService.ShowDialogAsync<GitHubForm>("Get code from a public Github Repo", option);
            if (!result.Success) return;
            string code = result.ReturnParameters.Get<string>("FileCode");
            CodeEditorService.CodeSnippet = code;

        }
        private void HandlePropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName != "ActiveCodeFile") return;
            CodeEditorService.CodeSnippet = CodeEditorService.ActiveCodeFile.Content;
            StateHasChanged();

        }

        private void ValidateFilesNames()
        {
            foreach (var codeFile in CodeEditorService.CodeFiles.Where(x => !x.Path.Contains(".razor")))
            {
                codeFile.Path = $"{codeFile.Path}.razor";
            }
        }

        #region TextExecution
        private async Task ExecuteProjectTest()
        {

            CodeAssemblyModel compilationResult = null;
            var codeFiles = new List<CodeFile>
            {
                new CodeFile
                {
                    Path = MainComponentFilePath,
                    Content = MainComponentCodePrefix + CodeSnippets.RazorSnippets["RazorParent"]
                },
                new CodeFile
                {
                    Path = $"RazorChild.razor", Content = CodeSnippets.RazorSnippets["RazorChild"]
                }
            };
            foreach (var codeFile in codeFiles)
            {
                Console.WriteLine($"Name: {codeFile.Path}");
                Console.WriteLine($"Content: {codeFile.Content}");
            }
            compilationResult = await RazorCompile.CompileToAssemblyAsync(codeFiles);
            Diagnostics.AddRange(compilationResult?.Diagnostics?.Select(x => x.ToString()) ?? new List<string> { "None" });
            if (compilationResult?.AssemblyBytes?.Length > 0)
            {
                await JsRuntime.InvokeVoidAsync("App.Repl.updateUserAssemblyInCacheStorage", compilationResult.AssemblyBytes);

                await JsRuntime.InvokeVoidAsync("App.reloadIFrame", "user-page-window", MainUserPagePath);
                isCodeCompiling = false;
                await InvokeAsync(StateHasChanged);
            }

        }


        #endregion

        public void Dispose()
        {
            dotNetInstance?.Dispose();
            CodeEditorService.PropertyChanged -= HandlePropertyChanged;
            _ = JsRuntime.InvokeVoidAsync("App.Repl.dispose");
            Console.WriteLine("RazorCodeHome.razor disposed");
        }
    }
}
