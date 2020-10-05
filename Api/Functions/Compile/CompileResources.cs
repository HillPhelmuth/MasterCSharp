using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.CodeAnalysis;
using Microsoft.JSInterop;

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
