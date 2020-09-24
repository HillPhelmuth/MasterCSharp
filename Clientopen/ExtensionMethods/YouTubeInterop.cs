using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace MasterSharpOpen.Client.ExtensionMethods
{
    public static class YouTubeInterop
    {
        public static ValueTask<object> StartYouTube(this IJSRuntime jsRuntime)
        {
            return jsRuntime.InvokeAsync<object>("startYouTube");
        }
        public static ValueTask<object> AddYouTubePlayer(this IJSRuntime jsRuntime)
        {
            return jsRuntime.InvokeAsync<object>("addPlayer");
        }
        public static ValueTask<object> RemoveYouTubePlayer(this IJSRuntime jsRuntime)
        {
            return jsRuntime.InvokeAsync<object>("removeYouTube");
        }
    }
}
