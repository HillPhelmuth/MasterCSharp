using Blazor.ModalDialog;
using BlazorApp.Shared;
using Microsoft.AspNetCore.Components;

namespace BlazorApp.Client.Pages.ShareCode
{
    public partial class SnippetMenu : ComponentBase
    {
        [Inject]
        protected AppState AppState { get; set; }
        [Inject]
        protected IModalDialogService ModalService { get; set; }
        [Parameter]
        public string CodeSnippet { get; set; }
        

        protected void UpdateCodeSnippet(string snippet)
        {
            var parameters = new ModalDialogParameters();
            CodeSnippet = snippet;
            parameters.Add("CodeSnippet", snippet);
            ModalService.Close(true, parameters);
        }
    }
}
