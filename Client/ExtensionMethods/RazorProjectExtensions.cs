using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorApp.Shared.RazorCompileService;

namespace BlazorApp.Client.ExtensionMethods
{
    public static class RazorProjectExtensions
    {
        public static List<CodeFile> PagifyMainComponent(this List<CodeFile> codeFiles)
        {
            var mainComponent = codeFiles.FirstOrDefault(x => x.Path == CoreConstants.MainComponentFilePath);
            if (!mainComponent.Content.Contains("@page"))
            {
                mainComponent.Content = CoreConstants.MainComponentCodePrefix + mainComponent.Content;
            }

            return codeFiles;
        }

        public static List<CodeFile> UnPagifyMainComponent(this List<CodeFile> codeFiles, string originalContent)
        {
            var mainComponent = codeFiles.FirstOrDefault(x => x.Path == CoreConstants.MainComponentFilePath);
            if (mainComponent.Content.Contains("@page"))
            {
                mainComponent.Content = originalContent;
            }

            return codeFiles;
        }
    }
}
