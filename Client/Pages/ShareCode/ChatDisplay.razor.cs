using System.Threading.Tasks;
using Blazor.ModalDialog;
using Microsoft.AspNetCore.Components;

namespace MasterCSharp.Client.Pages.ShareCode
{
    public partial class ChatDisplay
    {
        [Inject]
        protected IModalDialogService ModalService { get; set; }
        [Parameter]
        public string ChatContent { get; set; }

        private async Task Close(bool isClear = false)
        {
            if (isClear)
            {
                ChatContent = "<h2>Chat Cleared</h2>";
                await InvokeAsync(StateHasChanged);
                await Task.Delay(2000);
            }
            var modalParams = new ModalDialogParameters
            {
                {"IsClear", isClear}
            };

            ModalService.Close(true, modalParams);
        }
    }
}
