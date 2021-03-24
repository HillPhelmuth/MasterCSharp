using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace BlazorApp.Api.Functions.Compile
{
    public static class CompileResources
    {
        public static List<PortableExecutableReference> PortableExecutableReferences =>
            AppDomain.CurrentDomain.GetAssemblies().Where(x =>
                !x.IsDynamic && !string.IsNullOrWhiteSpace(x.Location) &&
                (x.FullName.Contains("System"))).Select(assembly => MetadataReference.CreateFromFile(assembly.Location)).ToList();
        public static List<PortableExecutableReference> PortableExecutableCompletionReferences =>
           AppDomain.CurrentDomain.GetAssemblies().Where(x =>
               !x.IsDynamic && !string.IsNullOrWhiteSpace(x.Location)).Select(assembly => MetadataReference.CreateFromFile(assembly.Location)).ToList();
    }
    public static class Extension
    {
        public static IEnumerable<T> Concatenate<T>(this IEnumerable<T> first, IEnumerable<T> second)
        {
            if (first == null)
            {
                return second;
            }
            if (second == null)
            {
                return first;
            }

            return first.Concat(second).Distinct();
        }
    }
}
//var narrowedAssemblies = appAssemblies.Where(x =>
//    !x.IsDynamic && !string.IsNullOrWhiteSpace(x.Location) &&
//    (x.FullName.Contains("System")));
//return narrowedAssemblies.Select(assembly => MetadataReference.CreateFromFile(assembly.Location)).ToList();