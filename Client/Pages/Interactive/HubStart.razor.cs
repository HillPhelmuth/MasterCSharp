using Blazor.ModalDialog;
using BlazorApp.Shared;
using Microsoft.AspNetCore.Components;

namespace BlazorApp.Client.Pages.Interactive
{
    public partial class HubStart
    {
        [Inject]
        protected AppStateService AppStateService { get; set; }
        [Inject]
        protected IModalDialogService ModalService { get; set; }
        [Parameter]
        public EventCallback<bool> ToggleChat { get; set; }
        private string userName;
        private string otherUser;
        private string teamname;
        private bool userSubmitted;
        private string CssClass = "chat-standard";
        
        private async void UpdateUser()
        {
            var result = await ModalService.ShowDialogAsync<HubSignIn>("Hub Sign-in");
            if (result.Success)
            {
                userName = result.ReturnParameters.Get<string>("UserName");
                otherUser = result.ReturnParameters.Get<string>("OtherUser");
                teamname = result.ReturnParameters.Get<string>("TeamName");
                AppStateService.UpdateShareUser(userName);
                if (!string.IsNullOrEmpty(otherUser))
                    AppStateService.UpdatePrivateUser(otherUser);
                if (!string.IsNullOrEmpty(teamname))
                    AppStateService.UpdateShareUser(teamname);
                userSubmitted = true;
                
            }
            StateHasChanged();
        }

        private void MoveWindow()
        {
            var cssClass = CssClass switch
            {
                "chat-standard" => "chat-left",
                "chat-left" => "chat-right",
                _ => "chat-standard"
            };
            CssClass = cssClass;
            StateHasChanged();
        }

        private async void HideChat()
        {
            userSubmitted = false;
            await ToggleChat.InvokeAsync(true);
        }
    }
}
