using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorApp.Shared.RazorCompileService;

namespace BlazorApp.Client.ExtensionMethods
{
    public static class RazorProjectExtensions
    {
        public static List<ProjectFile> PagifyMainComponent(this List<ProjectFile> codeFiles)
        {
            var mainComponent = codeFiles.FirstOrDefault(x => x.Path == DefaultStrings.MainComponentFilePath);
            if (!mainComponent.Content.Contains("@page"))
            {
                mainComponent.Content = DefaultStrings.MainComponentCodePrefix + mainComponent.Content;
            }

            return codeFiles;
        }

        public static List<ProjectFile> UnPagifyMainComponent(this List<ProjectFile> codeFiles, string originalContent)
        {
            var mainComponent = codeFiles.FirstOrDefault(x => x.Path == DefaultStrings.MainComponentFilePath);
            if (mainComponent.Content.Contains("@page"))
            {
                mainComponent.Content = originalContent;
            }

            return codeFiles;
        }
    }
}
