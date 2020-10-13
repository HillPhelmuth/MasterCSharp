using Blazor.ModalDialog;
using MasterCSharp.Shared.CodeServices;
using Microsoft.AspNetCore.Components;

namespace MasterCSharp.Client.Shared
{
    public partial class CodeOutModal
    {
        [Inject]
        public IModalDialogService ModalService { get; set; }
        [Inject]
        public CodeEditorService CodeEditorService { get; set; }
        [Parameter]
        public string CodeOutput { get; set; }

        private void ClearOutput()
        {
            CodeEditorService.CurrentOutput = string.Empty;
        }
    }
}
