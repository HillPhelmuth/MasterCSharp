using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazor.ModalDialog;
using BlazorApp.Shared.CodeServices;
using BlazorApp.Shared.RazorCompileService;
using Microsoft.AspNetCore.Components;

namespace BlazorApp.Client.Pages.RazorProject
{
    public partial class RazorSamplesModal
    {
        [Parameter]
        public CodeFile ActiveCodeFile { get; set; }
        [Inject]
        protected IModalDialogService ModalService { get; set; }
        [Inject]
        protected CodeEditorService CodeEditorService { get; set; }


        protected void UpdateActiveFile(KeyValuePair<string, string> selectedFile)
        { 
            ActiveCodeFile = new CodeFile{Path = $"{selectedFile.Key}.razor", Content = selectedFile.Value};
            var parameters = new ModalDialogParameters {{"ActiveCodeFile", ActiveCodeFile} };
            ModalService.Close(true, parameters);
        }
    }
}
