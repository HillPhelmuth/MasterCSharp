using System;
using System.Threading.Tasks;
using BlazorMonaco;
using BlazorMonaco.Bridge;
using MasterCSharp.Shared;
using MasterCSharp.Shared.ArenaChallenge;
using MasterCSharp.Shared.CodeModels;
using Microsoft.AspNetCore.Components;

namespace MasterCSharp.Client.Pages.ChallengeArena
{
    public partial class ArenaEditor : IDisposable
    {
        [Inject]
        protected ArenaService ArenaService { get; set; }
        [Inject]
        protected AppStateService AppStateService { get; set; }
        private string codeSnippet;
        private bool takeChallenge = false;

        [Parameter]
        public string CodeSnippet { get; set; }

        [Parameter]
        public Arena Arena { get; set; }
        [Parameter]
        public EventCallback<string> OnCodeSubmit { get; set; }

        private Challenge arenaChallenge = new Challenge();
        private TimeSpan stopWatch = new TimeSpan();
        private bool isClockRunning;

        protected override Task OnInitializedAsync()
        {
            arenaChallenge = Arena.CurrentChallenge;
            codeSnippet = arenaChallenge?.Snippet;
            ArenaService.OnArenasUpdate += HandleUpdateArena;
            isClockRunning = true;
            _ = StartClock();
            return base.OnInitializedAsync();
        }

        protected async Task StartClock()
        {

            while (isClockRunning)
            {
                await Task.Delay(1000);
                if (!isClockRunning) continue;
                stopWatch = stopWatch.Add(new TimeSpan(0, 0, 1));
                StateHasChanged();
            }


        }

        public async Task SubmitCode()
        {
            var currentCode = await Editor.GetValue();
            Console.WriteLine($"code submitted: {currentCode}");
            await OnCodeSubmit.InvokeAsync(currentCode);
        }

        private async Task HandleUpdateArena()
        {
            if (codeSnippet == Arena.CurrentChallenge.Snippet)
                return;
            await Editor.SetValue(Arena.CurrentChallenge.Snippet);
            await InvokeAsync(StateHasChanged);
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
                Value = CodeSnippet ?? codeSnippet
                //Value = CodeSnippet ?? codeSnippet "private string MyProgram() \n" +
                //    "{\n" +
                //    "    string input = \"this does not\"; \n" +
                //    "    string modify = input + \" suck!\"; \n" +
                //    "    return modify;\n" +
                //    "}\n" +
                //    "return MyProgram();"
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
            isClockRunning = false;
            ArenaService.OnArenasChanged -= StateHasChanged;
            Console.WriteLine("ArenaEditor.razor is disposed");
        }
    }
}
