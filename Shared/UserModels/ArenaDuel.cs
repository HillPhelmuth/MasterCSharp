using System;
using Newtonsoft.Json;

namespace MasterCSharp.Shared.UserModels
{
    public class ArenaDuel
    {
        public int ID { get; set; }
        public int UserAppDataID { get; set; }
        [JsonProperty("duelName")]
        public string DuelName { get; set; }

        [JsonProperty("challengeName")]
        public string ChallengeName { get; set; }

        [JsonProperty("rivalId")]
        public string RivalId { get; set; }

        [JsonProperty("solution")]
        public string Solution { get; set; }

        [JsonProperty("timeCompleted")]
        public DateTime TimeCompleted { get; set; }

        [JsonProperty("wonDuel")]
        public bool WonDuel { get; set; }

        //public override string ToString()
        //{
        //    return $"{DuelName},{ChallengeName},{RivalId},{Solution},{WonDuel},{TimeCompleted}";
        //}
    }
    public class ArenaResult
    {
        public string DuelName { get; set; }
        public string DuelWinner { get; set; }
        public string DuelLoser { get; set; }
    }
    public partial class ArenaResultMessage
    {
        [JsonProperty("group")]
        public string Group { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("end")]
        public bool End { get; set; }
        [JsonProperty("duelWinner")]
        public string DuelWinner { get; set; }
        [JsonProperty("duelLoser")]
        public string DuelLoser { get; set; }
    }

    public partial class ArenaResultMessage
    {
        public static ArenaResultMessage FromJson(string json) => JsonConvert.DeserializeObject<ArenaResultMessage>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this ArenaResultMessage self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
        };
    }
}