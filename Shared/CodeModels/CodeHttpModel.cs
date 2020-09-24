using System.Collections.Generic;
using Newtonsoft.Json;

namespace BlazorApp.Shared.CodeModels
{
    public class CodeHttpModel
    {
        [JsonProperty("snippet")]
        public string Snippet { get; set; }
        [JsonProperty("solution")]
        public string Solution { get; set; }
        [JsonProperty("tests")]
        public virtual List<Test> Tests { get; set; }
        [JsonProperty("compilation")]
        public CSharpCompilation Compilation { get; set; }
    }
}
