using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BlazorMonaco;
using BlazorMonaco.Bridge;
using MasterSharpOpen.Shared;
using MasterSharpOpen.Shared.CodeServices;
using MasterSharpOpen.Shared.UserModels;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TextCopy;

namespace MasterSharpOpen.Client.Pages.Practice
{
    public partial class MonacoEdit : ComponentBase, IDisposable
    {
        [Inject]
        public IJSRuntime jsRuntime { get; set; }

        [Inject]
        public CodeEditorService CodeEditorService { get; set; }
        [Inject]
        public IClipboard Clipboard { get; set; }
        [Inject]
        protected AppStateService AppStateService { get; set; }
        protected MonacoEditor Editor { get; set; }
      
        [Parameter]
        public EventCallback<string> OnSaveUserSnippet { get; set; }
        [Parameter]
        public string CodeSnippet { get; set; }
        [Parameter]
        public EventCallback<string> OnCodeSubmit { get; set; }
       
        private string currentCode = "";
        protected override Task OnInitializedAsync()
        {
            Editor = new MonacoEditor();
            CodeEditorService.OnSnippetChange += UpdateSnippet;
            return base.OnInitializedAsync();
        }
        public async Task SubmitCode()
        {
            currentCode = await Editor.GetValue();
            await OnCodeSubmit.InvokeAsync(currentCode);
        }

        protected async Task UpdateSnippet()
        {
            CodeSnippet = CodeEditorService.CodeSnippet;
            currentCode = CodeSnippet;
            await Editor.SetValue(CodeSnippet);
            Console.WriteLine("Snippet Updated");
            StateHasChanged();
        }
        private async Task AddSnippetToUser()
        {
            var snippetClip = await Editor.GetValue();
            await OnSaveUserSnippet.InvokeAsync(snippetClip);
        }
        protected StandaloneEditorConstructionOptions EditorOptionsRoslyn(MonacoEditor editor)
        {
            return new StandaloneEditorConstructionOptions
            {
                AutomaticLayout = true,
                AutoIndent = true,
                HighlightActiveIndentGuide = true,
                Language = "csharp",
                Value = CodeSnippet ?? "private string MyProgram() \n" +
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

        public void Dispose()
        {
            Console.WriteLine("MonacoEdit.razor Disposed");
            //CodeEditorService.OnChange -= StateHasChanged;
            CodeEditorService.OnSnippetChange -= UpdateSnippet;
        }
    }
}
