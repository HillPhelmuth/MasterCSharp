using System;
using System.ComponentModel;
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
        [CascadingParameter(Name = nameof(AppState))]
        protected AppState AppState { get; set; }
        public VideoModels Videos { get; set; }
        protected string selectedVideoId { get; set; }
        protected bool IsVideoReady;
        protected bool IsPageVideosReady;
        protected bool IsAddVideo;
        protected override async Task OnInitializedAsync()
        {

            Videos = AppState.Videos ?? await PublicClient.GetVideos();
            AppState.Videos = Videos;
            AppState.PropertyChanged += UpdateVideos;
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

        private void UpdateVideos(object sender, PropertyChangedEventArgs args)
        {
            Videos = AppState.Videos;
            StateHasChanged();
        }
        public void Dispose()
        {
            Console.WriteLine("VideoHome.razor disposed");
            AppState.PropertyChanged -= UpdateVideos;
        }
    }
}