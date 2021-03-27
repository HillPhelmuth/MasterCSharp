using System.IO;
using System.Threading.Tasks;
using BlazorApp.Api.Services;
using BlazorApp.Shared.CodeModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Assembly = System.Reflection.Assembly;
using Microsoft.CodeAnalysis.Host.Mef;
using System.Linq;
using System.Composition.Hosting;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Host;
using SyntaxTree = Microsoft.CodeAnalysis.SyntaxTree;
using Compilation = Microsoft.CodeAnalysis.Compilation;
using System.Runtime.Loader;


namespace BlazorApp.Api.Functions.Compile
{
    public class CompleteCode
    {
        [FunctionName("CompleteCode")]
        public async Task<IActionResult> Complete(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "CompleteCode")] HttpRequest req,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var sourceInfo = JsonConvert.DeserializeObject<SourceInfo>(requestBody);

            try
            {
                var result = await GetCodeCompletion(sourceInfo, log);
                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                log.LogError($"ERROR: {ex.Message}\r\n STACK-TRACE: {ex.StackTrace}\r\n INNER-TRACE: {ex.InnerException}");
                return new BadRequestObjectResult($"ERROR: {ex.Message}\r\n STACK-TRACE: {ex.StackTrace}\r\n INNER-TRACE: {ex.InnerException}");
            }

        }
        private static IEnumerable<PortableExecutableReference> _staticRefs;
        public static async Task<dynamic> GetCodeCompletion(SourceInfo sourceInfo, ILogger log = null)
        {
            var refs = CompileResources.PortableExecutableCompletionReferences;

            var usings = new List<string>()
            {
                "System",
                "System.IO",
                "System.Collections.Generic",
                "System.Collections",
                "System.Console",
                "System.Diagnostics",
                "System.Dynamic",
                "System.Linq",
                "System.Linq.Expressions",
                "System.Net.Http",
                "System.Text",
                "System.Net",
                "System.Threading.Tasks",
                "System.Numerics"
            };
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic && File.Exists(a.Location)).ToList();

            var partTypes = MefHostServices.DefaultAssemblies.Concat(assemblies)
                    .Distinct()?
                    .SelectMany(x => x?.GetTypes())?
                    .ToArray();

            var compositionContext = new ContainerConfiguration()
                .WithParts(partTypes)
                .CreateContainer();
            var host = MefHostServices.Create(compositionContext);

            var workspace = new AdhocWorkspace(host);

            string scriptCode = sourceInfo.SourceCode;
            var _ = typeof(Microsoft.CodeAnalysis.CSharp.Formatting.CSharpFormattingOptions);
            var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, usings: usings);
            if (refs == null || refs.Count == 0) return null;

            var scriptProjectInfo = ProjectInfo.Create(ProjectId.CreateNewId(), VersionStamp.Create(), "Script", "Script", LanguageNames.CSharp, isSubmission: true)
                .WithMetadataReferences(refs)
                .WithCompilationOptions(compilationOptions);

            var scriptProject = workspace.AddProject(scriptProjectInfo);
            var scriptDocumentInfo = DocumentInfo.Create(
                DocumentId.CreateNewId(scriptProject.Id), "Script",
                sourceCodeKind: SourceCodeKind.Script,
                loader: TextLoader.From(TextAndVersion.Create(SourceText.From(scriptCode), VersionStamp.Create())));
            var scriptDocument = workspace.AddDocument(scriptDocumentInfo);

            // cursor position is at the end
            int position = sourceInfo.LineNumberOffsetFromTemplate;
            var completionService = CompletionService.GetService(scriptDocument);
            var results = await completionService.GetCompletionsAsync(scriptDocument, position);
            if (results == null && sourceInfo.LineNumberOffsetFromTemplate < sourceInfo.SourceCode.Length)
            {
                sourceInfo.LineNumberOffsetFromTemplate++;
                await GetCodeCompletion(sourceInfo, log);
            }

            if (sourceInfo.SourceCode[sourceInfo.LineNumberOffsetFromTemplate - 1].ToString() == "(")
            {
                sourceInfo.LineNumberOffsetFromTemplate--;
                results = completionService.GetCompletionsAsync(scriptDocument, sourceInfo.LineNumberOffsetFromTemplate).Result;
            }

            //Method parameters
            var overloads = GetMethodParams(scriptCode, position, log);
            if (!(overloads?.Count > 0)) return results;
            var builder = ImmutableArray.CreateBuilder<CompletionItem>();
            foreach (var ci in overloads.Select(item => new { item, DisplayText = item })
                .Select(@t => @t.item.Split('(')[1].Split(')')[0])
                .Select(insertText => CompletionItem.Create(insertText, insertText, insertText)))
            {
                builder.Add(ci);
            }

            if (builder.Count <= 0) return results;
            var itemlist = builder.ToImmutable();
            return CompletionList.Create(new TextSpan(), itemlist);

        }
        public static List<string> GetMethodParams(string scriptCode, int position, ILogger log = null)
        {
            //position = position - 2;
            var overloads = new List<string>();
            var meta = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES");
            Console.WriteLine($"Trusted Platform Assemblies: {meta}");
            string[] assembliesNames = meta.ToString().Split(';');
            var sourceLanguage = new CSharpLanguage();
            var syntaxTree = sourceLanguage.ParseText(scriptCode, SourceCodeKind.Script);
            var compilation = CSharpCompilation.Create("MyCompilation",
                new[] { syntaxTree }, CompileResources.PortableExecutableCompletionReferences);

            var model = compilation.GetSemanticModel(syntaxTree);

            var theToken = syntaxTree.GetRoot().FindToken(position);
            var theNode = theToken.Parent;
            while (!theNode.IsKind(SyntaxKind.InvocationExpression))
            {
                theNode = theNode.Parent;
                if (theNode == null) break; // There isn't an InvocationExpression in this branch of the tree
            }

            if (theNode != null)
            {
                var symbolInfo = model.GetSymbolInfo(theNode);

                if (symbolInfo.CandidateSymbols.Length > 0)
                {
                    overloads.AddRange(symbolInfo.CandidateSymbols
                        .Select(param => new { parameters = param, i = param.ToMinimalDisplayParts(model, position) })
                        .Where(@t => @t.parameters.Kind == SymbolKind.Method)
                        .Select(@t => @t.parameters.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)));
                }
            }
            else
            {
                overloads = null;
            }

            return overloads;
        }
        public class CSharpLanguage : ILanguageService
        {
            private readonly LanguageVersion _maxLanguageVersion = Enum
                .GetValues(typeof(LanguageVersion))
                .Cast<LanguageVersion>()
                .Max();

            public SyntaxTree ParseText(string sourceCode, SourceCodeKind kind)
            {
                var options = new CSharpParseOptions(kind: kind, languageVersion: _maxLanguageVersion);

                // Return a syntax tree of our source code
                return CSharpSyntaxTree.ParseText(sourceCode, options);
            }
        }
    }
}

