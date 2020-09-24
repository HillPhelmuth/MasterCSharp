using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Blazor.ModalDialog;
using BlazorApp.Shared;
using Microsoft.AspNetCore.Components;

namespace BlazorApp.Client.Pages.ShareCode
{
    public partial class GitHubForm
    {
        [Inject]
        public AppStateService AppStateService { get; set; }
        [Inject]
        public IModalDialogService ModalService { get; set; }
        [Inject]
        public PublicGithubClient GithubClient { get; set; }
        public GitHubFormModel FormModel { get; set; } = new GitHubFormModel();

        private async Task Submit()
        {
            var codeFile = await GithubClient.CodeFromPublicRepo(FormModel.GithubName, FormModel.RepoName,
                FormModel.FilePath, FormModel.FileName);
            var parameters = new ModalDialogParameters
            {
                {"FileCode", codeFile}
            };
            ModalService.Close(true, parameters);
        }
    }

    public class GitHubFormModel
    {
        [Required]
        public string GithubName { get; set; }
        [Required]
        public string RepoName { get; set; }
       
        public string FilePath { get; set; }
        [Required]
        public string FileName { get; set; }
    }
}
