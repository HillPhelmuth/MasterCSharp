﻿using System.Collections.Generic;

namespace BlazorApp.Shared.CodeModels
{
    public class CodeSample
    {
        public CodeSample()
        {
        }

        public CodeSample(string name, string code, string description, string toolTip)
        {
            Name = name;
            Code = code;
            Description = description;
            ToolTip = toolTip;
        }

        public CodeSample(string name, string code, string description)
        {
            Name = name;
            Code = code;
            Description = description;
        }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string ToolTip { get; set; }
        
    }

    public class CodeSamples
    {
        public SampleSection Section { get; set; }
        public List<CodeSample> Samples { get; set; }
        public Dictionary<string,string> ResourceURLs { get; set; }
    }

    public enum SampleSection
    {
        Linq, Collection, String, ConditionalsLoops, Extension
    }
}
