using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime;
using System.Threading.Tasks;
using BlazorApp.Shared.RazorCompileService;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Razor;
using Microsoft.JSInterop;

namespace BlazorApp.Shared.CodeServices
{
    public class RazorCompile
    {
        private static CSharpCompilation baseCompilation;
        private IEnumerable<MetadataReference> _references;
        private const string DefaultImports = @"@using System.ComponentModel.DataAnnotations
@using System.Linq
@using System.Net.Http
@using System.Net.Http.Json
@using Microsoft.AspNetCore.Components.Forms
@using Microsoft.AspNetCore.Components.Routing
@using Microsoft.AspNetCore.Components.Web
@using Microsoft.JSInterop";
        public const string DefaultRootNamespace = "BlazorApp.OutputRCL";

        private const string WorkingDirectory = "/BlazorOut/";
        private readonly RazorProjectFileSystem _fileSystem = new WebRazorProjectFileSystem();
        private readonly RazorConfiguration _config = RazorConfiguration.Create(
            RazorLanguageVersion.Latest,
            configurationName: "Blazor",
            extensions: Array.Empty<RazorExtension>());

        
        //public async Task<CodeAssemblyModel> CompileToAssemblyAsync(CodeFile codeFile, string preset = "basic")
        //{
        //    if (codeFile == null)
        //    {
        //        throw new ArgumentNullException(nameof(codeFile));
        //    }

        //    var cSharpResults = await ConvertRazorToCSharp(codeFile);

        //    //await (updateStatusFunc?.Invoke("Compiling Assembly") ?? Task.CompletedTask);
        //    var result = GetCodeAssembly(new List<RazorToCSharpModel>(cSharpResults));

        //    return result;
        //}
        public async Task<CodeAssemblyModel> CompileToAssemblyAsync(ICollection<CodeFile> codeFiles, string preset = "basic")
        {
            if (codeFiles == null)
            {
                throw new ArgumentNullException(nameof(codeFiles));
            }

            var cSharpResults = await ConvertRazorToCSharp(codeFiles);

            //await (updateStatusFunc?.Invoke("Compiling Assembly") ?? Task.CompletedTask);
            var result = GetCodeAssembly(new List<RazorToCSharpModel>(cSharpResults));

            return result;
        }

        private async Task<ICollection<RazorToCSharpModel>> ConvertRazorToCSharp(ICollection<CodeFile> codeFiles)
        {
            // The first phase won't include any metadata references for component discovery. This mirrors what the build does.
            var projectEngine = CreateRazorProjectEngine(Array.Empty<MetadataReference>());
            var declarations = new List<RazorToCSharpModel>();
            foreach (var codeFile in codeFiles)
            {
                var projectItem = CreateRazorProjectItem(codeFile.Path, codeFile.Content);

                var codeDocument = projectEngine.ProcessDeclarationOnly(projectItem);
                var cSharpDocument = codeDocument.GetCSharpDocument();

                declarations.Add(new RazorToCSharpModel
                {
                    ProjectItem = projectItem,
                    Code = cSharpDocument.GeneratedCode,
                    Diagnostics = cSharpDocument.Diagnostics.Select(diagnostic => new CustomDiag(diagnostic)),
                });
            }

            // Get initial core assembly
            var tempAssembly = GetCodeAssembly(declarations);
            if (tempAssembly.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
            {
                return new[] { new RazorToCSharpModel { Diagnostics = tempAssembly.Diagnostics } };
            }
            // add new assemblies to project refs
            var references = new List<MetadataReference>(baseCompilation.References) { tempAssembly.Compilation.ToMetadataReference() };
            projectEngine = this.CreateRazorProjectEngine(references);
            var results = new List<RazorToCSharpModel>();
            // Iterates through raw c# code and converts to members of generated Razor project
            foreach (var declaration in declarations)
            {
                var codeDocument = projectEngine.Process(declaration.ProjectItem);
                var cSharpDocument = codeDocument.GetCSharpDocument();

                results.Add(new RazorToCSharpModel
                {
                    ProjectItem = declaration.ProjectItem,
                    Code = cSharpDocument.GeneratedCode,
                    Diagnostics = cSharpDocument.Diagnostics.Select(x => new CustomDiag(x))
                });
            }
            return results;
        }

        private CodeAssemblyModel GetCodeAssembly(ICollection<RazorToCSharpModel> cSharpResults)
        {
            //return new CodeAssemblyModel();
            var cSharpParseOptions = new CSharpParseOptions(LanguageVersion.Preview);

            if (cSharpResults.Any(r => r.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error)))
            {
                return new CodeAssemblyModel { Diagnostics = cSharpResults.SelectMany(r => r.Diagnostics).ToList() };
            }

            var syntaxTrees = new List<SyntaxTree>(cSharpResults.Count);
            foreach (var cSharpResult in cSharpResults)
            {
                syntaxTrees.Add(CSharpSyntaxTree.ParseText(cSharpResult.Code, cSharpParseOptions));
            }

            var finalCompilation = baseCompilation.AddSyntaxTrees(syntaxTrees);

            var compilationDiagnostics = finalCompilation.GetDiagnostics().Where(d => d.Severity > DiagnosticSeverity.Info);

            var result = new CodeAssemblyModel
            {
                Compilation = finalCompilation,
                Diagnostics = compilationDiagnostics.Select(diag => new CustomDiag(diag)),
                //.Select(CompilationDiagnostic.FromCSharpDiagnostic)
                //.Concat(cSharpResults.SelectMany(r => r.Diagnostics))
                //.ToList(),
            };

            if (result.Diagnostics.All(x => x.Severity != DiagnosticSeverity.Error))
            {
                using var peStream = new MemoryStream();
                finalCompilation.Emit(peStream);

                result.AssemblyBytes = peStream.ToArray();

                return result;
            }

            return result;
        }
        public RazorProjectEngine CreateRazorProjectEngine(IReadOnlyList<MetadataReference> references)
        {
            return RazorProjectEngine.Create(_config, _fileSystem, builder =>
            {
                builder.SetRootNamespace(DefaultRootNamespace);
                builder.AddDefaultImports(DefaultImports);

                // Features that use Roslyn are mandatory for components
                CompilerFeatures.Register(builder);

                builder.Features.Add(new CompilationTagHelperFeature());
                builder.Features.Add(new DefaultMetadataReferenceFeature { References = references });
            });
        }

        private static RazorProjectItem CreateRazorProjectItem(string fileName, string fileContent)
        {
            var fullPath = WorkingDirectory + fileName;

            // File paths in Razor are always of the form '/a/b/c.razor'
            var filePath = fileName;
            if (!filePath.StartsWith('/'))
            {
                filePath = '/' + filePath;
            }

            fileContent = fileContent.Replace("\r", string.Empty);

            return new WebRazorProjectItem(
                WorkingDirectory,
                filePath,
                fullPath,
                fileName,
                FileKinds.Component,
                Encoding.UTF8.GetBytes(fileContent.TrimStart()));
        }
        public static async Task InitAsync(HttpClient httpClient)
        {
            var basicReferenceAssemblyRoots = new[]
            {
                typeof(AssemblyTargetedPatchBandAttribute).Assembly, // System.Runtime
                typeof(NavLink).Assembly, // Microsoft.AspNetCore.Components.Web
                typeof(IQueryable).Assembly, // System.Linq
                typeof(HttpClientJsonExtensions).Assembly, // System.Net.Http.Json
                typeof(HttpClient).Assembly, // System.Net.Http
                typeof(IJSRuntime).Assembly, // Microsoft.JSInterop
                typeof(RequiredAttribute).Assembly, // System.ComponentModel.Annotations
            };

            var assemblyNames = basicReferenceAssemblyRoots
                .SelectMany(assembly => assembly.GetReferencedAssemblies().Concat(new[] { assembly.GetName() }))
                .Select(x => x.Name)
                .Distinct()
                .ToList();

            var assemblyStreams = await GetAssemblyStreams(httpClient, assemblyNames);

            Dictionary<string, PortableExecutableReference> allReferenceAssemblies = assemblyStreams.ToDictionary(a => a.Key, a => MetadataReference.CreateFromStream(a.Value));

            var basicReferenceAssemblies = allReferenceAssemblies
                .Where(a => basicReferenceAssemblyRoots
                    .Select(x => x.GetName().Name)
                    .Union(basicReferenceAssemblyRoots.SelectMany(y => y.GetReferencedAssemblies().Select(z => z.Name)))
                    .Any(n => n == a.Key))
                .Select(a => a.Value)
                .ToList();

            baseCompilation = CSharpCompilation.Create(
                "BlazorRepl.UserComponents",
                Array.Empty<SyntaxTree>(),
                basicReferenceAssemblies,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        }
        private static async Task<IDictionary<string, Stream>> GetAssemblyStreams(HttpClient httpClient, IEnumerable<string> assemblyNames)
        {
            var streams = new ConcurrentDictionary<string, Stream>();

            await Task.WhenAll(
                assemblyNames.Select(async assemblyName =>
                {
                    var result = await httpClient.GetAsync($"/_framework/_bin/{assemblyName}.dll");

                    result.EnsureSuccessStatusCode();

                    streams.TryAdd(assemblyName, await result.Content.ReadAsStreamAsync());
                }));

            return streams;
        }
    }
}
