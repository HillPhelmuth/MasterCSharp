using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MasterSharpOpen.Shared.UserModels
{
    public class UserDuel
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("userId")]
        public string UserId { get; set; }

        //[JsonProperty("duelsCompleted")]
        //public string DuelsCompleted { get; set; }

        [JsonProperty("completedDuels")]
        public List<ArenaDuel> CompletedDuelsList { get; set; }
        public static UserDuel FromJson(string json) => JsonConvert.DeserializeObject<UserDuel>(json);
        public Task<UserDuel> TempCreateDuel()
        {
            var random = new Random();
            var id = random.Next(1, 999999);
            
            var completed = new ArenaDuel()
            {
                ChallengeName = "HardChallenge",
                DuelName = Guid.NewGuid().ToString(),
                RivalId = "adam@adam",
                Solution = @"var x = 1; var y = 2; return xy;",
                TimeCompleted = DateTime.Now,
                WonDuel = true
            };
            var completed2 = new ArenaDuel()
            {
                ChallengeName = "HarderChallenge",
                DuelName = Guid.NewGuid().ToString(),
                RivalId = "adam@adam",
                Solution = @"var x = 4; var y = 6; return xy;",
                TimeCompleted = DateTime.Now,
                WonDuel = true
            };
            var initial = new UserDuel { Id = id, UserId = "HillPhelmuth", CompletedDuelsList =  new List<ArenaDuel>{completed, completed2} };
            Console.WriteLine($"init: {JsonConvert.SerializeObject(initial)}");
            
            return Task.FromResult(initial);
        }
    }
    
    //public class CompletedDuel
    //{
    //    [JsonProperty("duelId")]
    //    public string DuelName { get; set; }

    //    [JsonProperty("challengeName")]
    //    public string ChallengeName { get; set; }

    //    [JsonProperty("rivalId")]
    //    public string RivalId { get; set; }

    //    [JsonProperty("solution")]
    //    public string Solution { get; set; }

    //    [JsonProperty("timeCompleted")]
    //    public DateTime TimeCompleted { get; set; }

    //    [JsonProperty("wonDuel")]
    //    public bool WonDuel { get; set; }
    //}
    
}
