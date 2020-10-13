using System;
using System.ComponentModel;
using System.Threading.Tasks;
using BlazorMonaco;
using BlazorMonaco.Bridge;
using MasterCSharp.Shared;
using MasterCSharp.Shared.CodeServices;
using Microsoft.AspNetCore.Components;
using TextCopy;

namespace MasterCSharp.Client.Pages.Practice
{
    public partial class MonacoEdit : ComponentBase, IDisposable
    {
        //[Inject]
        //public IJSRuntime jsRuntime { get; set; }

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
        [Parameter]
        public string ButtonLabel { get; set; }
        [Parameter]
        public string Language { get; set; }
        
       
        //private string currentCode = "";
        protected override Task OnInitializedAsync()
        {
            Language ??= "csharp";
            Editor = new MonacoEditor();
            
            CodeEditorService.PropertyChanged += UpdateSnippet;
            return base.OnInitializedAsync();
        }
        public async Task SubmitCode()
        {
            var currentCode = await Editor.GetValue();
            await OnCodeSubmit.InvokeAsync(currentCode);
        }

        protected async void UpdateSnippet(object sender, PropertyChangedEventArgs args)
        {
            if (!args.PropertyName.Contains("CodeSnippet") && !args.PropertyName.Contains("Language"))
                return;
            CodeSnippet = CodeEditorService.CodeSnippet;
            Language = CodeEditorService.Language;
            await Editor.SetValue(CodeSnippet);
            Console.WriteLine("Snippet Updated");
            StateHasChanged();
        }
        private async Task AddSnippetToUser()
        {
            var snippetClip = await Editor.GetValue();
            await OnSaveUserSnippet.InvokeAsync(snippetClip);
        }
        #region Monaco Editor

        protected StandaloneEditorConstructionOptions EditorOptionsRoslyn(MonacoEditor editor)
        {
            return new StandaloneEditorConstructionOptions
            {
                AutomaticLayout = true,
                AutoIndent = true,
                //HighlightActiveIndentGuide = true,
                ColorDecorators = true,
                Minimap = new MinimapOptions{Enabled = false},
                Hover = new HoverOptions { Delay = 400 },
                Find = new FindOptions{AutoFindInSelection = true,SeedSearchStringFromSelection = true,AddExtraSpaceOnTop = true},
                Lightbulb = new LightbulbOptions{Enabled = true},
                AcceptSuggestionOnEnter = "smart",
                Language = Language,
                Value = CodeEditorService.CodeSnippet ?? "private string MyProgram() \n" +
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
            await Editor.AddAction("saveAction", "Save Snippet", new int[] { (int)KeyMode.CtrlCmd | (int)KeyCode.KEY_D, (int)KeyMode.CtrlCmd | (int)KeyCode.KEY_S }, null, null, "navigation", 1.5, async (editor, keyCodes) =>
            {
                await AddSnippetToUser();
                Console.WriteLine("Ctrl+D : Editor action is triggered.");
            });
            await Editor.AddAction("executeAction", "Execute Code",
                new int[] {(int) KeyMode.CtrlCmd | (int) KeyCode.Enter}, null, null, "navigation", 2.5,
                async (editor, keyCodes) =>
                {
                    await SubmitCode();
                    Console.WriteLine("Code Executed from Editor Command");
                });
            await Editor.SetValue(CodeEditorService.CodeSnippet);
            var newDecorations = new[]
            {
                new ModelDeltaDecoration
                {
                    Range = new BlazorMonaco.Bridge.Range(3,1,3,1),
                    Options = new ModelDecorationOptions
                    {
                        IsWholeLine = false,
                        ClassName = "decorationContentClass",
                        GlyphMarginClassName = "decorationGlyphMarginClass"
                    }
                }
            };

            decorationIds = await Editor.DeltaDecorations(null, newDecorations);
        }
        private string[] decorationIds;
        
        protected void OnContextMenu(EditorMouseEvent eventArg)
        {
            
            Console.WriteLine("OnContextMenu : " + System.Text.Json.JsonSerializer.Serialize(eventArg));
        }
        private async Task ChangeTheme(ChangeEventArgs e)
        {
            Console.WriteLine($"setting theme to: {e.Value.ToString()}");
            await MonacoEditorBase.SetTheme(e.Value.ToString());
        }
       
        public async Task CopyCodeToClipboard()
        {
            var snippetClip = await Editor.GetValue();
            await Clipboard.SetTextAsync(snippetClip);
        }

        #endregion


        public void Dispose()
        {
            Console.WriteLine("MonacoEdit.razor Disposed");
            //CodeEditorService.OnChange -= StateHasChanged;
            CodeEditorService.PropertyChanged -= UpdateSnippet;
        }
    }
}
