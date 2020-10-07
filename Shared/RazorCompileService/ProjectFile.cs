﻿using System;

namespace BlazorApp.Shared.RazorCompileService
{
    public class ProjectFile
    {
        public int ID { get; set; }
        public int UserProjectID { get; set; }
        public string Path { get; set; }
        public FileType FileType { get; set; }
        public string Content { get; set; }
    }

    public enum FileType
    {
        [EnumString("cs")]
        Class,
        [EnumString("razor")]
        Razor
    }

    public class EnumString : Attribute
    {
        public EnumString(string value)
        {
            Value = value;
        }
        public string Value { get; }
    }
}
