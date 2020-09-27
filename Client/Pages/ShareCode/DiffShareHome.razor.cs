using System.Threading.Tasks;
using Blazor.ModalDialog;
using BlazorApp.Client.Pages.Interactive;
using BlazorApp.Shared;
using BlazorApp.Shared.CodeServices;
using Microsoft.AspNetCore.Components;

namespace BlazorApp.Client.Pages.ShareCode
{
    public partial class DiffShareHome
    {
        [Inject]
        public CodeEditorService CodeEditorService { get; set; }
        [Inject]
        protected IModalDialogService ModalService { get; set; }
        [Inject]
        protected AppStateService AppStateService { get; set; }
        [Inject]
        protected PublicClient PublicClient { get; set; }
        protected string CodeSnippet { get; set; }
        private string ChatContent { get; set; } = "";

        private string userName;
        private string otherUser;
        private string teamname;
        private bool userSubmitted;
        private bool isSelectSnippet;
        private bool isNewChat;

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
        private void HandleNewMessage(string message)
        {
            if (!message.Contains("::"))
            {
                ChatContent += $"<div class='text'>{message}</div><br/>";
                InvokeAsync(StateHasChanged);
                return;
            }
            var userMessage = message.Split("::");
            ChatContent +=
                $"<div class='user'>From: {userMessage[0]}<br/></div><div class='text'>{userMessage[1]}</div><br/>";
            isNewChat = true;
            InvokeAsync(StateHasChanged);
        }
        protected async void UpdateCodeSnippet()
        {
            var result = await ModalService.ShowDialogAsync<SnippetMenu>("Select a code snippet");
            if (result.Success)
            {
                string snippet = result.ReturnParameters.Get<string>("CodeSnippet");
                CodeSnippet = snippet;
                CodeEditorService.CodeSnippet = snippet;
                //CodeEditorService.UpdateSnippet(snippet);
                isSelectSnippet = true;
            }

            await InvokeAsync(StateHasChanged);
        }

        protected async void UpdateFromPublicRepo()
        {
            var option = new ModalDialogOptions
            {
                Style = "modal-dialog-githubform"
            };
            var result = await ModalService.ShowDialogAsync<GitHubForm>("Get code from a public Github Repo", option);
            if (result.Success)
            {
                string code = result.ReturnParameters.Get<string>("FileCode");
                CodeSnippet = code;
                CodeEditorService.CodeSnippet = code;
                //CodeEditorService.UpdateSnippet(code);
                isSelectSnippet = true;
            }

            await InvokeAsync(StateHasChanged);
        }

        private async Task ShowChatContent()
        {
            isNewChat = false;
            var parameters = new ModalDialogParameters
            {
                {"ChatContent", ChatContent}
            };
            var options = new ModalDialogOptions
            {
                Position = ModalDialogPositionOptions.TopRight
            };
            bool isClear = false;
            var result = await ModalService.ShowDialogAsync<ChatDisplay>("Chat Messages", options, parameters);
            if (result.Success)
            {
                isClear = result.ReturnParameters.Get<bool>("IsClear");
            }

            ChatContent = isClear ? string.Empty : ChatContent;
        }
    }
}
