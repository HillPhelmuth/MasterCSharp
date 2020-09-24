using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace MasterSharpOpen.Shared.CodeModels
{
    
    public class CodeChallenges
    {
        [JsonProperty("challenges")]
        public List<Challenge> Challenges { get; set; }
    }
   
    public class Challenge
    {
        [JsonIgnore]
        public int ID { get; set; }
        [Required]
        [JsonProperty("name")]
        public string Name { get; set; }
        [Required]
        [JsonProperty("difficulty")]
        public string Difficulty { get; set; }
        [Required]
        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("examples")]
        public string Examples { get; set; }

        [JsonProperty("snippet")]
        public string Snippet { get; set; }
        [JsonProperty("solution")]
        public string Solution { get; set; }
        [JsonProperty("tests")]
        public virtual List<Test> Tests { get; set; }
        public string AddedBy { get; set; }
        [NotMapped]
        public bool UserCompleted { get; set; }

    }
    public class Test
    {
        [JsonIgnore]
        public int ID { get; set; }
        [JsonIgnore]
        public int ChallengeID { get; set; }
        [JsonProperty("append")]
        public string Append { get; set; }

        [JsonProperty("testAgainst")]
        public string TestAgainst { get; set; }
        //[JsonIgnore]
        //public Challenge Challenge { get; set; }
    }

}
