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
        private static IEnumerable<PortableExecutableReference> _ref;
        public static async Task<dynamic> GetCodeCompletion(SourceInfo sourceInfo, ILogger log)
        {
            var thisAssembly = typeof(CompleteCode).Assembly;
            var loadContext = AssemblyLoadContext.GetLoadContext(thisAssembly);
            var refs = CompileResources.PortableExecutableCompletionReferences;

            var usings = new List<string>();
            var allusingsInCode = sourceInfo.SourceCode.Split(new string[] { "using " }, StringSplitOptions.None);

            foreach (var item in allusingsInCode)
            {
                if (!string.IsNullOrWhiteSpace(item))
                {
                    usings.Add(item.Split(';')[0]);
                }
            }

            List<Assembly> assemblies = loadContext.Assemblies.Where(a => !a.IsDynamic && File.Exists(a.Location)).ToList();

            var partTypes = MefHostServices.DefaultAssemblies.Concat(assemblies)
                    .Distinct()?
                    .SelectMany(x => x?.GetTypes())?
                    .ToArray();

            var compositionContext = new ContainerConfiguration()
                .WithParts(partTypes)
                .CreateContainer();
            var host = MefHostServices.Create(compositionContext);

            var workspace = new AdhocWorkspace(host);

            var scriptCode = sourceInfo.SourceCode;
            var _ = typeof(Microsoft.CodeAnalysis.CSharp.Formatting.CSharpFormattingOptions);
            var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, usings: usings);
            if (_ref == null || !_ref.Any())
            {
                var _refs = assemblies
                 .Select(asm => MetadataReference.CreateFromFile(asm.Location))
                 .ToArray();
                _ref = new PortableExecutableReference[0];


                _ref = _refs.Concatenate(_refs);
            }
            if (_ref?.Count() > 0)
            {
                var _ref = refs.Concatenate(new[]
                {
                     MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
                });
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
                var position = sourceInfo.LineNumberOffsetFromTemplate;
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
                List<string> overloads = GetMethodParams(scriptCode, position, log);
                if (overloads?.Count > 0)
                {
                    ImmutableArray<CompletionItem>.Builder builder = ImmutableArray.CreateBuilder<CompletionItem>();
                    foreach (var item in overloads)
                    {
                        string DisplayText = item;
                        string insertText = item.Split('(')[1].Split(')')[0];
                        CompletionItem ci = CompletionItem.Create(insertText, insertText, insertText);
                        builder.Add(ci);
                    }
                    if (builder.Count > 0)
                    {
                        ImmutableArray<CompletionItem> itemlist = builder.ToImmutable();
                        return CompletionList.Create(new TextSpan(), itemlist);
                    }
                }
                return results;
            }
            return null;
        }
        public static List<string> GetMethodParams(string scriptCode, int position, ILogger log = null)
        {
            //position = position - 2;
            List<string> overloads = new List<string>();
            var meta = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES");
            string[] assembliesNames = meta.ToString().Split(';', StringSplitOptions.None);
            var sourceLanguage = new CSharpLanguage();
            //SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(scriptCode);
            SyntaxTree syntaxTree = sourceLanguage.ParseText(scriptCode, SourceCodeKind.Script);
            var root = (CompilationUnitSyntax)syntaxTree.GetRoot();
            var thisAssembly = typeof(CompleteCode).Assembly;
            var loadContext = AssemblyLoadContext.GetLoadContext(thisAssembly);


            var Mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            var compilation = CSharpCompilation.Create("MyCompilation",
                syntaxTrees: new[] { syntaxTree }, references: CompileResources.PortableExecutableCompletionReferences);

            var model = compilation.GetSemanticModel(syntaxTree);

            var theToken = syntaxTree.GetRoot().FindToken(position);
            var theNode = theToken.Parent;
            while (!theNode.IsKind(SyntaxKind.InvocationExpression))
            {
                theNode = theNode.Parent;
                if (theNode == null) break; // There isn't an InvocationExpression in this branch of the tree
            }

            if (theNode == null)
            {
                overloads = null;
            }
            else
            {
                var symbolInfo = model.GetSymbolInfo(theNode);
                var symbol = symbolInfo.Symbol;
                var containingType = symbol?.ContainingType;

                if (symbolInfo.CandidateSymbols != default && symbolInfo.CandidateSymbols.Length > 0)
                {
                    foreach (var parameters in symbolInfo.CandidateSymbols)
                    {
                        var i = parameters.ToMinimalDisplayParts(model, position);
                        if (parameters.Kind == SymbolKind.Method)
                        {
                            var mp = parameters.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
                            overloads.Add(mp);
                        }
                    }
                }
            }
            return overloads;

        }
        public class CSharpLanguage : ILanguageService
        {
            private readonly LanguageVersion MaxLanguageVersion = Enum
                .GetValues(typeof(LanguageVersion))
                .Cast<LanguageVersion>()
                .Max();

            public SyntaxTree ParseText(string sourceCode, SourceCodeKind kind)
            {
                var options = new CSharpParseOptions(kind: kind, languageVersion: MaxLanguageVersion);

                // Return a syntax tree of our source code
                return CSharpSyntaxTree.ParseText(sourceCode, options);
            }
        }
    }
}

