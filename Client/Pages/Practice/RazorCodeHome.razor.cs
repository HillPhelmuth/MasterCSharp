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
using BlazorApp.Shared.ExtensionMethods;
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
        protected IModalDialogService ModalService { get; set; }

        private const string MainComponentCodePrefix = "@page \"/__razorOutput\"\n";
        private const string MainUserPagePath = "/__razorOutput";
        public const string MainComponentFilePath = "__RazorOutput.razor";
        public List<ProjectFile> Files { get; set; } = new List<ProjectFile>();
        private DotNetObjectReference<RazorCodeHome> dotNetInstance;
        private string language = "razor";
        private bool isready;
        private static string sampleSnippet = CodeSnippets.RazorSnippets["RazorSample"];
       
        private List<string> Diagnostics { get; set; } = new List<string>();
        private bool isCodeCompiling;
        private bool isCSharp;
        protected override async Task OnInitializedAsync()
        {
            CodeEditorService.CodeSnippet = sampleSnippet;
            var mainCodeFile = new ProjectFile { Path = MainComponentFilePath, Content = sampleSnippet, FileType = FileType.Razor};
            CodeEditorService.ActiveProjectFile = mainCodeFile;
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

                await JsRuntime.InvokeVoidAsync("App.Razor.init", dotNetInstance);
            }
            await base.OnAfterRenderAsync(firstRender);
        }
        
        protected void HandleSave(string content)
        {
            CodeEditorService.ActiveProjectFile.Content = content;
            var currentFile = CodeEditorService.ActiveProjectFile;
            currentFile.Content = content;
            CodeEditorService.SaveCode(currentFile);

        }
        protected async Task AddCodeFile()
        {
            var inputForm = new ModalDataInputForm("Create new conponent", "what should we call this component?");
            var snippetField = inputForm.AddStringField("Name", "Component Name", "");
            var languageField = inputForm.AddEnumField("FileType", "File type", FileType.Razor);
            string filename = string.Empty;
            string fileType = "razor";
            var options = new ModalDialogOptions()
            {
                Style = "small-modal"
            };
            if (await inputForm.ShowAsync(ModalService, options))
            {
                filename = snippetField.Value;
                fileType = languageField.Value.AsString();
            }

            if (languageField.Value == FileType.Class)
            {
                isCSharp = true;
                await InvokeAsync(StateHasChanged);
            }
            if (string.IsNullOrEmpty(filename)) return;
            var newCodeFile = new ProjectFile();
            if (!filename.Contains(".razor") && !filename.Contains(".cs"))
            {
                newCodeFile.Path = $"{filename}.{fileType}";
                newCodeFile.FileType = languageField.Value;
            }
            sampleSnippet = languageField.Value == FileType.Razor ? $"<h1>{filename}</h1>" : $"class {filename}\r{{\r\t\r}}";

            CodeEditorService.CodeSnippet = sampleSnippet;
            newCodeFile.Content = sampleSnippet;
            CodeEditorService.ActiveProjectFile = newCodeFile;
            await InvokeAsync(StateHasChanged);
        }

        protected async void SelectActiveFile()
        {
            var result = await ModalService.ShowDialogAsync<CodeFileModal>("Select a code snippet");
            if (result.Success)
            {
                
                var codeFile = result.ReturnParameters.Get<ProjectFile>("ActiveCodeFile");
                isCSharp = codeFile.FileType == FileType.Class;
                await InvokeAsync(StateHasChanged);
                await Task.Delay(50);
                CodeEditorService.ActiveProjectFile = codeFile;
                CodeEditorService.CodeSnippet = codeFile.Content;
                
            }

            await InvokeAsync(StateHasChanged);
        }

        protected async void SelectSampleSnippet()
        {
            var result = await ModalService.ShowDialogAsync<RazorSamplesModal>("Select a code snippet");
            if (result.Success)
            {
                var snippet = result.ReturnParameters.Get<ProjectFile>("ActiveCodeFile");
                CodeEditorService.ActiveProjectFile = snippet;
                CodeEditorService.CodeSnippet = snippet.Content;
            }

            await InvokeAsync(StateHasChanged);
        }

        private Task LoadSampleProject()
        {
            var sampleProject = SampleProjects.IntroProject;
            var sampleMain = sampleProject.FirstOrDefault(x => x.Path == DefaultStrings.MainComponentFilePath);
            CodeEditorService.CodeFiles = sampleProject;
            CodeEditorService.ActiveProjectFile = sampleMain;
            CodeEditorService.CodeSnippet = sampleMain?.Content;
            return InvokeAsync(StateHasChanged);
        }
        protected async Task ExecuteProject()
        {
            isCodeCompiling = true;
            Diagnostics = new List<string>();
            StateHasChanged();
            CodeAssemblyModel compilationResult = null;
            ProjectFile mainComponent = null;
            string originalMainComponentContent = null;
            originalMainComponentContent = CodeEditorService.CodeFiles
                .FirstOrDefault(x => x.Path == DefaultStrings.MainComponentFilePath)
                ?.Content ?? DefaultStrings.MainComponentDefaultFileContent;
            var codeFiles = CodeEditorService.CodeFiles.PagifyMainComponent();
            
            compilationResult = await RazorCompile.CompileToAssemblyAsync(codeFiles);
            Diagnostics.AddRange(compilationResult?.Diagnostics?.Select(x => x.ToString()) ?? new List<string> { "None" });
           
            if (compilationResult?.AssemblyBytes?.Length > 0)
            {
                await JsRuntime.InvokeVoidAsync("App.Razor.updateUserAssemblyInCacheStorage", compilationResult.AssemblyBytes);

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
            CodeEditorService.CodeSnippet = CodeEditorService.ActiveProjectFile.Content;
            StateHasChanged();

        }

        #region TextExecution
        private async Task ExecuteProjectTest()
        {

            CodeAssemblyModel compilationResult = null;
            var codeFiles = new List<ProjectFile>
            {
                new ProjectFile
                {
                    Path = MainComponentFilePath,
                    Content = MainComponentCodePrefix + CodeSnippets.RazorSnippets["RazorParent"]
                },
                new ProjectFile
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
                await JsRuntime.InvokeVoidAsync("App.Razor.updateUserAssemblyInCacheStorage", compilationResult.AssemblyBytes);

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
            _ = JsRuntime.InvokeVoidAsync("App.Razor.dispose");
            Console.WriteLine("RazorCodeHome.razor disposed");
        }
    }
}
