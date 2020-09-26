using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazor.ModalDialog;
using BlazorApp.Shared.CodeServices;
using Microsoft.AspNetCore.Components;

namespace BlazorApp.Client.Shared
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
