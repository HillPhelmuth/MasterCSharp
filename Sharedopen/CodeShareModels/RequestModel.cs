using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace MasterSharpOpen.Shared.CodeShareModels
{
    public class RequestModel
    {
        [JsonProperty("userId")]
        public string UserId { get; set; }
        [JsonProperty("userName")]
        public string UserName { get; set; }
        [JsonProperty("groupName")]
        public string GroupName { get; set; }
        [JsonProperty("messageContent")]
        public string MessageContent { get; set; }
    }
}
