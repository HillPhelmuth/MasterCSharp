using System;
using System.Threading.Tasks;
using Blazor.ModalDialog;
using BlazorApp.Shared.CodeServices;
using BlazorMonaco;
using BlazorMonaco.Bridge;
using Microsoft.AspNetCore.Components;

namespace BlazorApp.Client.Pages.ShareCode
{
    public partial class DiffShare
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
            CodeEditorService.OnSnippetChange += UpdateSnippet;
            CodeEditorService.OnSharedSnippetChange += UpdateSharedSnippet;
            return base.OnInitializedAsync();
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
            TextModel original_model = await MonacoEditorBase.GetModel("sample-diff-editor-originalModel");
            if (original_model == null)
            {
                var originalValue = CodeSnippet ?? "private string MyProgram() \n" +
                                     "{\n" +
                                     "    string input = \"this does not\"; \n" +
                                     "    string modify = input + \" suck!\"; \n" +
                                     "    return modify;\n" +
                                     "}\n" +
                                     "return MyProgram();";
                original_model = await MonacoEditorBase.CreateModel(originalValue, "csharp", "sample-diff-editor-originalModel");
            }

            // Get or create the modified model
            TextModel modified_model = await MonacoEditorBase.GetModel("sample-diff-editor-modifiedModel");
            if (modified_model == null)
            {
                var modifiedValue = CodeSnippet ?? "private string MyProgram() \n" +
                                     "{\n" +
                                     "    string input = \"this does not\"; \n" +
                                     "    string modify = input + \" suck!\"; \n" +
                                     "    return modify;\n" +
                                     "}\n" +
                                     "return MyProgram();";
                modified_model = await MonacoEditorBase.CreateModel(modifiedValue, "csharp", "sample-diff-editor-modifiedModel");
            }

            // Set the editor model
            await DiffEditor.SetModel(new DiffEditorModel
            {
                Original = original_model,
                Modified = modified_model
            });
        }
        protected async Task UpdateSnippet()
        {
            CodeSnippet = CodeEditorService.CodeSnippet;
            ValueToSetOriginal = CodeSnippet;
            await DiffEditor.OriginalEditor.SetValue(CodeSnippet);
            Console.WriteLine("Snippet Updated");
            StateHasChanged();
        }

        protected async Task UpdateSharedSnippet()
        {
            ValueToSetModified = CodeEditorService.SharedCodeSnippet;
            await DiffEditor.ModifiedEditor.SetValue(ValueToSetModified);
        }

        protected async void AddSnippetToUser()
        {
            ValueToSetOriginal = await DiffEditor.OriginalEditor.GetValue();
            await OnSendCode.InvokeAsync(ValueToSetOriginal);
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
        private void EditorOnKeyUpOriginal(KeyboardEvent keyboardEvent)
        {
            switch (keyboardEvent.KeyCode)
            {
                case KeyCode.Enter when keyboardEvent.CtrlKey:
                    SubmitCode();
                    break;
                case KeyCode.KEY_S when keyboardEvent.CtrlKey && keyboardEvent.ShiftKey:
                    AddSnippetToUser();
                    break;
            }

            Console.WriteLine("OnKeyUpOriginal : " + keyboardEvent.Code);
        }

        private void EditorOnKeyUpModified(KeyboardEvent keyboardEvent)
        {
            Console.WriteLine("OnKeyUpModified : " + keyboardEvent.Code);
        }
    }
}
