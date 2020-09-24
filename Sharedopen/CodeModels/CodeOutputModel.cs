using System.Collections.Generic;
using Newtonsoft.Json;

namespace MasterSharpOpen.Shared.CodeModels
{
    public class CodeOutputModel
    {
        [JsonProperty("outputs")]
        public List<Output> Outputs { get; set; }
    }
    public partial class Output
    {
        [JsonProperty("testIndex")]
        public int TestIndex { get; set; }

        [JsonProperty("test")]
        public Test Test { get; set; }

        [JsonProperty("codeout")]
        public string Codeout { get; set; }

        [JsonProperty("testResult")]
        public bool TestResult { get; set; }
        [JsonIgnore]
        public string CssClass { get; set; }
    }

   
}
