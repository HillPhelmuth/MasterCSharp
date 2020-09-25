using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Newtonsoft.Json;

namespace BlazorApp.Api.Models
{
    public class UserAppData
    {
        [JsonProperty("id")]
        public int ID { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("challengeSuccessData")]
        public string ChallengeSuccessData { get; set; }
        [JsonProperty("snippets")]
        public List<UserSnippet> Snippets { get; set; }
        public List<ArenaDuel> CompletedDuels { get; set; }
        [JsonProperty("challengeSuccessIds")]
        [NotMapped]
        public List<int> ChallengeSuccessIds
        {
            get
            {
                var idList = ChallengeSuccessData?.Split(',').ToList();
                var list = new List<int>();
                foreach (var id in idList ?? new List<string>())
                {
                    var didParse = int.TryParse(id, out int val);
                    if (didParse) list.Add(val);
                }

                return list;
            }
            set => ChallengeSuccessData = string.Join(',', value);
        }
    }

    public class UserSnippet
    {
        public int ID { get; set; }
        public int UserAppDataID { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("snippet")]
        public string Snippet { get; set; }
    }
}
