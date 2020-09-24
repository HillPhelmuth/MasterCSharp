using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace BlazorApp.Api.Functions.Compile
{
    public class CompileResources
    {
        

        

        public static List<PortableExecutableReference> PortableExecutableReferences
        {
            get
            {
                var appAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                var narrowedAssemblies = appAssemblies.Where(x =>
                    !x.IsDynamic && !string.IsNullOrWhiteSpace(x.Location) &&
                    (x.FullName.Contains("System")));

                return narrowedAssemblies.Select(assembly => MetadataReference.CreateFromFile(assembly.Location)).ToList();
            }
        }
    }
}
