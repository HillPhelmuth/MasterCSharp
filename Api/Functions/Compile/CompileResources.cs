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

        object[] basicReferenceAssemblyRoots = new[]
        {
            typeof(AssemblyTargetedPatchBandAttribute).Assembly, // System.Runtime
            typeof(NavLink).Assembly, // Microsoft.AspNetCore.Components.Web
            typeof(IQueryable).Assembly, // System.Linq
            typeof(HttpClientJsonExtensions).Assembly, // System.Net.Http.Json
            typeof(HttpClient).Assembly, // System.Net.Http
            typeof(IJSRuntime).Assembly, // Microsoft.JSInterop
            typeof(RequiredAttribute).Assembly, // System.ComponentModel.Annotations
        };


        public static List<PortableExecutableReference> PortableExecutableReferences
        {
            get
            {
                var appAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                var narrowedAssemblies = appAssemblies.Where(x =>
                    !x.IsDynamic && !string.IsNullOrWhiteSpace(x.Location) || x.FullName == nameof(HttpClientJsonExtensions)
                    //&&
                    /*(x.FullName.Contains("System") || x.FullName.Contains("AspNetCore"))*/
                );
                int i = 0;
                foreach (var narrowedAssembly in narrowedAssemblies)
                {
                    if (narrowedAssembly.FullName != null && narrowedAssembly.FullName.ToLower().Contains("http") || narrowedAssembly.FullName.ToLower().Contains("json"))
                    {
                        Console.WriteLine($"Ass {++i} {narrowedAssembly.FullName}");
                    }
                }                
                return narrowedAssemblies.Select(assembly => MetadataReference.CreateFromFile(assembly.Location)).ToList();
            }
        }
    }
}
