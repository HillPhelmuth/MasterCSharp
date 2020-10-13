using System.ComponentModel.DataAnnotations.Schema;
using MasterCSharp.Shared.CodeModels;
using Newtonsoft.Json;

namespace MasterCSharp.Shared.ArenaChallenge
{
    public class Arena
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        //[JsonProperty("partitionKey")] 
        //public string PartitionKey => $"{Creator}|{Id}";
        public string Name { get; set; }

        public string Creator { get; set; }

        public string Opponent { get; set; }
        [NotMapped]
        [JsonIgnore]
        public Challenge CurrentChallenge { get; set; }

        public string ChallengeName { get; set; }


        public bool IsFull => !string.IsNullOrEmpty(Creator) && !string.IsNullOrEmpty(Opponent);
    }
}