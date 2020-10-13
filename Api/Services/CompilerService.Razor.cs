using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MasterCSharp.Shared.RazorCompileService;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Razor; //using BlazorApp.Shared.CodeModels;
//using BlazorApp.Shared.CodeModels;
using SyntaxTree = Microsoft.CodeAnalysis.SyntaxTree;

namespace MasterCSharp.Api.Services
{
    public partial class CompilerService
    {

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

        public async Task<CodeAssemblyModel> GetRazorAssembly(ProjectFile projectFile,
            IEnumerable<MetadataReference> references, string preset = "")
        {
            runningCompilation = CSharpCompilation.Create(
                "BlazorApp.OutputRCL",
                Array.Empty<SyntaxTree>(),
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            _references = references;
            return await CompileToAssemblyAsync(projectFile);
        }
        public async Task<CodeAssemblyModel> CompileToAssemblyAsync(ProjectFile projectFile, string preset = "basic")
        {
            if (projectFile == null)
            {
                throw new ArgumentNullException(nameof(projectFile));
            }

            var cSharpResults = await ConvertRazorToCSharp(projectFile);

            //await (updateStatusFunc?.Invoke("Compiling Assembly") ?? Task.CompletedTask);
            var result = GetCodeAssembly(new List<RazorToCSharpModel>(cSharpResults));

            return result;
        }

        private async Task<ICollection<RazorToCSharpModel>> ConvertRazorToCSharp(ProjectFile projectFile)
        {
            // The first phase won't include any metadata references for component discovery. This mirrors what the build does.
            var projectEngine = CreateRazorProjectEngine(Array.Empty<MetadataReference>());
            var declarations = new List<RazorToCSharpModel>();

            var projectItem = CreateRazorProjectItem(projectFile.Path, projectFile.Content);

            var codeDocument = projectEngine.ProcessDeclarationOnly(projectItem);
            var cSharpDocument = codeDocument.GetCSharpDocument();

            declarations.Add(new RazorToCSharpModel
            {
                ProjectItem = projectItem,
                Code = cSharpDocument.GeneratedCode,
                Diagnostics = cSharpDocument.Diagnostics.Select(diagnostic => new CustomDiag(diagnostic)),
            });
            // Result of doing 'temp' compilation
            var tempAssembly = GetCodeAssembly(declarations);
            if (tempAssembly.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
            {
                return new[] { new RazorToCSharpModel { Diagnostics = tempAssembly.Diagnostics } };
            }
            // Add the 'temp' compilation as a metadata reference
            var references = new List<MetadataReference>(runningCompilation.References) { tempAssembly.Compilation.ToMetadataReference() };
            projectEngine = this.CreateRazorProjectEngine(references);
            var results = new List<RazorToCSharpModel>();
            foreach (var declaration in declarations)
            {
                codeDocument = projectEngine.Process(declaration.ProjectItem);
                cSharpDocument = codeDocument.GetCSharpDocument();

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

            var finalCompilation = runningCompilation.AddSyntaxTrees(syntaxTrees);

            var compilationDiagnostics = finalCompilation.GetDiagnostics().Where(d => d.Severity > DiagnosticSeverity.Info);

            var result = new CodeAssemblyModel
            {
                Compilation = finalCompilation,
                Diagnostics = compilationDiagnostics.Select(diag => new CustomDiag(diag)),
               
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
            return RazorProjectEngine.Create(_config, _fileSystem, b =>
            {
                b.SetRootNamespace(DefaultRootNamespace);
                b.AddDefaultImports(DefaultImports);
                CompilerFeatures.Register(b);
                b.Features.Add(new CompilationTagHelperFeature());
                b.Features.Add(new DefaultMetadataReferenceFeature { References = references });
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
    }
}
