using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Blazor.ModalDialog;
using BlazorMonaco;
using BlazorMonaco.Bridge;
using MasterCSharp.Shared.CodeServices;
using Microsoft.AspNetCore.Components;

namespace MasterCSharp.Client.Pages.ShareCode
{
    public partial class DiffShare : IDisposable
    {
        [Inject]
        public CodeEditorService CodeEditorService { get; set; }
        [Inject]
        protected IModalDialogService ModalService { get; set; }
        [Parameter]
        public string CodeSnippet { get; set; }
        [Parameter]
        public EventCallback<string> OnSubmitCode { get; set; }
        [Parameter]
        public EventCallback<string> OnSendCode { get; set; }
        //private string SharedSnippet { get; set; }
        private MonacoDiffEditor DiffEditor { get; set; }
        private string ValueToSetOriginal { get; set; }
        private string ValueToSetModified { get; set; }

        protected override Task OnInitializedAsync()
        {
            CodeEditorService.PropertyChanged += UpdateSnippet;
            //CodeEditorService.OnSharedSnippetChange += UpdateSharedSnippet;
            return base.OnInitializedAsync();
        }
        private async Task ChangeTheme(ChangeEventArgs e)
        {
            Console.WriteLine($"setting theme to: {e.Value}");
            await MonacoEditorBase.SetTheme(e.Value.ToString());
        }
        private DiffEditorConstructionOptions DiffEditorConstructionOptions(MonacoDiffEditor editor)
        {
            return new DiffEditorConstructionOptions
            {
                OriginalEditable = true
            };
        }
        private async Task EditorOnDidInit(MonacoEditorBase editor)
        {
            // Get or create the original model
            TextModel originalModel = await MonacoEditorBase.GetModel("sample-diff-editor-originalModel");
            if (originalModel == null)
            {
                var originalValue = CodeSnippet ?? "private string MyProgram() \n" +
                                     "{\n" +
                                     "    string input = \"this does not\"; \n" +
                                     "    string modify = input + \" suck!\"; \n" +
                                     "    return modify;\n" +
                                     "}\n" +
                                     "return MyProgram();";
                originalModel = await MonacoEditorBase.CreateModel(originalValue, "csharp", "sample-diff-editor-originalModel");
            }

            // Get or create the modified model
            TextModel modifiedModel = await MonacoEditorBase.GetModel("sample-diff-editor-modifiedModel");
            if (modifiedModel == null)
            {
                var modifiedValue = CodeSnippet ?? "private string MyProgram() \n" +
                                     "{\n" +
                                     "    string input = \"this does not\"; \n" +
                                     "    string modify = input + \" suck!\"; \n" +
                                     "    return modify;\n" +
                                     "}\n" +
                                     "return MyProgram();";
                modifiedModel = await MonacoEditorBase.CreateModel(modifiedValue, "csharp", "sample-diff-editor-modifiedModel");
            }

            // Set the editor model
            await DiffEditor.SetModel(new DiffEditorModel
            {
                Original = originalModel,
                Modified = modifiedModel
            });
        }
        protected async void UpdateSnippet(object sender, PropertyChangedEventArgs args)
        {
            ValueToSetModified = CodeEditorService.SharedCodeSnippet;
            CodeSnippet = CodeEditorService.CodeSnippet;
            switch (args.PropertyName)
            {
                case nameof(CodeEditorService.CodeSnippet):
                    await DiffEditor.OriginalEditor.SetValue(CodeSnippet);
                    break;
                case nameof(CodeEditorService.SharedCodeSnippet):
                    var result = await ModalService.ShowMessageBoxAsync("Confirm Replace",
                        "Your teammate is sharing their code. Replace the code in your Modfied code window (The right side of the code editor) with shared code?",
                        MessageBoxButtons.YesNo, MessageBoxDefaultButton.Button2);
                    if (result == MessageBoxDialogResult.Yes)
                    {
                        await DiffEditor.ModifiedEditor.SetValue(ValueToSetModified);
                    }
                    break;
            }
            
            Console.WriteLine("Snippet Updated");
            await InvokeAsync(StateHasChanged);
        }

        //protected async Task UpdateSharedSnippet()
        //{
        //    ValueToSetModified = CodeEditorService.SharedCodeSnippet;
        //    await DiffEditor.ModifiedEditor.SetValue(ValueToSetModified);
        //}

        protected async void SendSnippetToUser()
        {
            var toSetOriginal = await DiffEditor.OriginalEditor.GetValue();
            await OnSendCode.InvokeAsync(toSetOriginal);
        }

        protected async void SubmitCode()
        {
            var code = await DiffEditor.OriginalEditor.GetValue();
            await OnSubmitCode.InvokeAsync(code);
        }

        protected async Task TakeDiff()
        {
            var result = await ModalService.ShowMessageBoxAsync("Confirm Replace", "Are you sure you want to replace your code?", MessageBoxButtons.YesNo, MessageBoxDefaultButton.Button2);
            if (result == MessageBoxDialogResult.No)
                return;
            var diffValue = await DiffEditor.ModifiedEditor.GetValue();
            await DiffEditor.OriginalEditor.SetValue(diffValue);
        }

        private async void PushToDiff()
        {
            var result = await ModalService.ShowMessageBoxAsync("Push Code To Diff Window", "Are you sure you want to sync your code editor windows?", MessageBoxButtons.YesNo, MessageBoxDefaultButton.Button2);
            if (result != MessageBoxDialogResult.Yes) return;
            var originalValue = await DiffEditor.OriginalEditor.GetValue();
            await DiffEditor.ModifiedEditor.SetValue(originalValue);
        }
        private void EditorOnKeyUpOriginal(KeyboardEvent keyboardEvent)
        {
            switch (keyboardEvent.KeyCode)
            {
                case KeyCode.Enter when keyboardEvent.CtrlKey:
                    SubmitCode();
                    break;
                case KeyCode.KEY_S when keyboardEvent.CtrlKey && keyboardEvent.ShiftKey:
                    SendSnippetToUser();
                    break;
                case KeyCode.KEY_D when keyboardEvent.CtrlKey && keyboardEvent.ShiftKey:
                    PushToDiff();
                    break;
            }

            Console.WriteLine("OnKeyUpOriginal : " + keyboardEvent.Code);
        }

        private void EditorOnKeyUpModified(KeyboardEvent keyboardEvent)
        {
            Console.WriteLine("OnKeyUpModified : " + keyboardEvent.Code);
        }

        public void Dispose() => CodeEditorService.PropertyChanged -= UpdateSnippet;
    }
}
