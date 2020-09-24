using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace BlazorApp.Shared.VideoModels
{
    public class Videos
    {
        [JsonProperty("videosList")]
        public List<VideoSection> VideoSections { get; set; }
    }
    [Serializable]
    public class VideoSection
    {
        [JsonProperty]
        public int ID { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("subHeader")]
        public string SubHeader { get; set; }
        [JsonProperty("videos")]
        public List<Video> Videos { get; set; }
        [JsonIgnore]
        [NotMapped]
        public bool IsVisible { get; set; }
    }

    public class Video
    {
        [JsonIgnore]
        public int ID { get; set; }
        [JsonProperty("videoSectionId")]
        public int VideoSectionID { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("videoId")]
        public string VideoId { get; set; }
       
    }
}
