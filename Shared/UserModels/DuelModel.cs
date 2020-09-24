using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BlazorApp.Shared.UserModels
{

    public partial class DuelModel
    {
        [JsonProperty("id")]
        [JsonConverter(typeof(ParseStringConverter))]
        public int Id { get; set; }

        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("duelsWonAsJson")]
        public string DuelsWonAsJson { get; set; }

        [JsonProperty("completedDuels")]
        public List<CompletedDuel> CompletedDuels { get; set; }

        public Task<DuelModel> TempCreateDuel()
        {
            var random = new Random();
            var id = random.Next(1, 999999);
            var initial = new DuelModel() { Id = id, UserId = Guid.NewGuid().ToString(), CompletedDuels = new List<CompletedDuel>() };
            var completed = new CompletedDuel()
            {
                Attempts = 1,
                ChallengeName = "Braces",
                //Duel = initial,
                DuelId = "1|GoTime",
                RivalId = "adam@adam",
                Solution = "var x = 1; var y = 2; return x+y;",
                Time = new Dictionary<string, double>() { { "10", 20.00 } },
                WonDuel = true
            };
            Console.WriteLine(initial.DuelsWonAsJson);
            initial.CompletedDuels.Add(completed);
            return Task.FromResult(initial);
        }
    }


    public partial class CompletedDuel
    {
        [JsonProperty("duelId")]
        public string DuelId { get; set; }

        [JsonProperty("challengeName")]
        public string ChallengeName { get; set; }

        [JsonProperty("rivalId")]
        public string RivalId { get; set; }

        [JsonProperty("solution")]
        public string Solution { get; set; }

        [JsonProperty("time")]
        public Dictionary<string, double> Time { get; set; }

        [JsonProperty("wonDuel")]
        public bool WonDuel { get; set; }

        [JsonProperty("attempts")]
        public int Attempts { get; set; }
    }

    public partial class DuelModel
    {
        public static DuelModel FromJson(string json) => JsonConvert.DeserializeObject<DuelModel>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this DuelModel self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class ParseStringConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(int) || t == typeof(int?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            int l;
            if (int.TryParse(value, out l))
            {
                return l;
            }
            throw new Exception("Cannot unmarshal type int");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (int)untypedValue;
            serializer.Serialize(writer, value.ToString());
            return;
        }

        public static readonly ParseStringConverter Singleton = new ParseStringConverter();
    }
}


