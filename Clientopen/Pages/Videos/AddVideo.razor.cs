using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MasterSharpOpen.Client.ExtensionMethods;
using MasterSharpOpen.Shared;
using MasterSharpOpen.Shared.VideoModels;
using Microsoft.AspNetCore.Components;

namespace MasterSharpOpen.Client.Pages.Videos
{
    public partial class AddVideo
    {
        [Inject]
        protected AppStateService AppStateService { get; set; }
        [Inject]
        protected PublicClient PublicClient { get; set; }
        private Video Video { get; set; }
        private List<VideoSection> VideoSections { get; set; }
        private VideoSection selectedSection;
        
        private string title;
        private string videoUrl;
       
        private bool isSubmitReady;
        private string userMessage;
        private string apiResponse;
        [Parameter]
        public EventCallback<string> TryPlayVideo { get; set; }

        protected override Task OnInitializedAsync()
        {
            VideoSections = AppStateService?.Videos?.VideoSections;
            return base.OnInitializedAsync();
        }

        private void SubmitVideo()
        {
            if (string.IsNullOrEmpty(title) && string.IsNullOrEmpty(videoUrl))
            {
                userMessage = "<p class=\"pageError\">Please provide a Title and Url</p>";
                return;
            }
            var videoId = videoUrl.GetVideoId();
            if (videoId == null)
            {
                userMessage = "<p class=\"pageError\">Please provide a full YouTube Url. </p>";
                return;
            }
            Console.WriteLine($"Selected Section:\nName:{selectedSection.Name},\nSubHeader:{selectedSection.SubHeader},\nSectionID:{selectedSection.ID}");
            Video = new Video { Title = title, VideoId = videoId, VideoSectionID = selectedSection.ID};
            isSubmitReady = true;
            TryPlayVideo.InvokeAsync(videoId);
            StateHasChanged();
        }

        private async Task AddVideoToDb()
        {
            var apiResult = await PublicClient.PostVideo(Video);
            apiResponse = apiResult ? "Submission Successful!" : "Sorry, something went wrong. Submission failed";
            if (apiResult)
            {
                AppStateService.AddVideo(Video);
                title = "";
                videoUrl = "";
                Video = null;
                StateHasChanged();
                await Task.Delay(2000);
                isSubmitReady = !isSubmitReady;
                StateHasChanged();
            }
            StateHasChanged();
        }
    }
}
