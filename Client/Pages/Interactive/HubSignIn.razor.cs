using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Blazor.ModalDialog;
using MasterCSharp.Shared;
using Microsoft.AspNetCore.Components;

namespace MasterCSharp.Client.Pages.Interactive
{
    public partial class HubSignIn
    {
        [Inject]
        protected IModalDialogService ModalService { get; set; }
        [Inject]
        protected AppStateService AppStateService { get; set; }
        private SignInForm SignInForm { get; set; } = new SignInForm();
        protected override Task OnInitializedAsync()
        {
            if (!string.IsNullOrEmpty(AppStateService.UserName))
                SignInForm.UserName = AppStateService.UserName;
            return base.OnInitializedAsync();
        }

        protected void Submit()
        {
            var parameters = new ModalDialogParameters
            {
                {"UserName", SignInForm.UserName}, {"OtherUser", SignInForm.OtherUser}, {"TeamName", SignInForm.TeamName}
            };
            ModalService.Close(true, parameters);
        }
    }

    public class SignInForm
    {
        [Required]
        public string UserName { get; set; }
        public string OtherUser { get; set; }
        public string TeamName { get; set; }
    }
}
