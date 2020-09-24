using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazor.ModalDialog;
using MasterSharpOpen.Shared;
using MasterSharpOpen.Shared.CodeServices;
using Microsoft.AspNetCore.Components;

namespace MasterSharpOpen.Client.Pages.ShareCode
{
    public partial class SnippetMenu : ComponentBase
    {
        [Inject]
        protected AppStateService AppStateService { get; set; }
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
