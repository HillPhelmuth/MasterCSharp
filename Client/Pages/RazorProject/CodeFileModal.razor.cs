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
    public partial class CodeFileModal
    {
        [Parameter]
        public ProjectFile ActiveProjectFile { get; set; }
        [Inject]
        protected IModalDialogService ModalService { get; set; }
        [Inject]
        protected CodeEditorService CodeEditorService { get; set; }

        protected void UpdateActiveFile(ProjectFile selectedFile)
        {
            var parameters = new ModalDialogParameters();
            ActiveProjectFile = selectedFile;
            parameters.Add("ActiveCodeFile", selectedFile);
            ModalService.Close(true, parameters);

        }
    }
}
