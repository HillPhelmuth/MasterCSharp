using System.Collections.Generic;
using Newtonsoft.Json;

namespace BlazorApp.Shared.CodeModels
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
    public class CodeInputModel
    {
        [JsonProperty("solution")]
        public string Solution { get; set; }
        [JsonProperty("tests")]
        public List<Test> Tests { get; set; }
    }

}
