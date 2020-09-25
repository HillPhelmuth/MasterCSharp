using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace BlazorApp.Api.Models
{
    public class ChallengeForm : Challenge
    {
        [Required]
        public string MethodName { get; set; }
        [Required]
        public string MethodInputs { get; set; }
        [Required]
        public string ReturnType { get; set; }
        public readonly string[] ReturnTypeItems = Enum.GetNames(typeof(InputType)).Select(x => x.ToLower()).ToArray();
        public string ReturnCollectionType { get; set; }

        public readonly string[] ReturnCollectionTypeItems =
            Enum.GetNames(typeof(InputCollectionType)).ToArray();

        public readonly string[] DifficultyItems = Enum.GetNames(typeof(DifficultyType)).ToArray();
        public List<string> ExampleList { get; set; }
        private enum InputCollectionType
        {
            None, Single, Array, List, Generic
        }

        private enum InputType
        {
            Choose, Bool, Byte, Char, Decimal, Int, Long, String
        }

        private enum DifficultyType
        {
           Easiest, Easier, Easy,Mid,Hard, Harder,Hardest
        }
    }
}
