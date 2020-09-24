using System;
using System.Threading.Tasks;
using BlazorApp.Shared;
using Microsoft.AspNetCore.Components;
using VideoModels = BlazorApp.Shared.VideoModels.Videos;

namespace BlazorApp.Client.Pages.Videos
{
    public partial class VideoHome : IDisposable
    {
        [Inject]
        protected PublicClient PublicClient { get; set; }
        [Inject]
        protected AppStateService AppStateService { get; set; }
        public VideoModels Videos { get; set; }
        protected string selectedVideoId { get; set; }
        protected bool IsVideoReady;
        protected bool IsPageVideosReady;
        protected bool IsAddVideo;
        protected override async Task OnInitializedAsync()
        {
            Videos = AppStateService.Videos;
            Videos ??= await PublicClient.GetVideos();
            AppStateService.SetVideos(Videos);
            AppStateService.OnChange += UpdateVideos;
            IsPageVideosReady = true;
        }
        protected void HandleVideoEnd(bool isEnd)
        {
            IsVideoReady = false;
        }
        protected async Task PlayVideos()
        {
            if (IsVideoReady)
            {
                IsVideoReady = false;
                StateHasChanged();
                await Task.Delay(200);
            }
            IsVideoReady = true;
            StateHasChanged();
        }

        protected Task HandleTryPlay(string videoId)
        {
            selectedVideoId = videoId;
            return PlayVideos();
        }

        private void UpdateVideos()
        {
            Videos = AppStateService.Videos;
            StateHasChanged();
        }
        public void Dispose()
        {
            Console.WriteLine("VideoHome.razor disposed");
            AppStateService.OnChange -= UpdateVideos;
        }
    }
}