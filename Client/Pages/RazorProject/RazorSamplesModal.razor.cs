using System.Collections.Generic;
using Blazor.ModalDialog;
using MasterCSharp.Shared.CodeServices;
using MasterCSharp.Shared.RazorCompileService;
using Microsoft.AspNetCore.Components;

namespace MasterCSharp.Client.Pages.RazorProject
{
    public partial class RazorSamplesModal
    {
        [Parameter]
        public ProjectFile ActiveProjectFile { get; set; }
        [Inject]
        protected IModalDialogService ModalService { get; set; }
        [Inject]
        protected CodeEditorService CodeEditorService { get; set; }


        protected void UpdateActiveFile(KeyValuePair<string, string> selectedFile)
        { 
            ActiveProjectFile = new ProjectFile{Path = $"{selectedFile.Key}.razor", Content = selectedFile.Value, FileType = FileType.Razor};
            var parameters = new ModalDialogParameters {{"ActiveCodeFile", ActiveProjectFile} };
            ModalService.Close(true, parameters);
        }
    }
}
