using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazor.ModalDialog;
using BlazorApp.Shared;
using BlazorApp.Shared.CodeServices;
using BlazorApp.Shared.RazorCompileService;
using BlazorApp.Shared.UserModels;
using Microsoft.AspNetCore.Components;

namespace BlazorApp.Client.Pages.RazorProject
{
    public partial class ProjectCrudModal
    {
        [Inject]
        public AppStateService AppStateService { get; set; }
        [Inject]
        public CodeEditorService CodeEditorService { get; set; }
        [Inject]
        public IModalDialogService ModalDialogService { get; set; }

        protected void SelectUserProject(UserProject project)
        {
            AppStateService.ActiveProject = project;
            CodeEditorService.CodeFiles = project.Files;
            CodeEditorService.ActiveProjectFile =
                project.Files.FirstOrDefault(x => x.Path == DefaultStrings.MainComponentFilePath);
            CodeEditorService.CodeSnippet = CodeEditorService.ActiveProjectFile?.Content ?? "EMPTY";
            ModalDialogService.Close(true);
        }

        protected async Task DeleteUserProject(UserProject project)
        {

        }
    }
}
