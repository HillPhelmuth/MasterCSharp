using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BlazorApp.Api.Services;
using BlazorApp.Shared.CodeModels;
using BlazorApp.Shared.RazorCompileService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BlazorApp.Api.Functions.Compile
{
    public class CompileCode
    {
        private readonly CompilerService _compilerService;
        public CompileCode(CompilerService compilerService)
        {
            _compilerService = compilerService;
        }

        [FunctionName("CompileCode")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "code")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP/code trigger function processed a request.");

            var executableReferences = CompileResources.PortableExecutableReferences;
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var codeInput = JsonConvert.DeserializeObject<CodeInputModel>(requestBody);
            var testCode = codeInput.Solution;
            var result = await _compilerService.SubmitCode(testCode, executableReferences);
            
            return new OkObjectResult(result);
        }
        [FunctionName("CompileRazorText")]
        public async Task<IActionResult> RunRazorText(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP/code trigger function processed a request.");

            var executableReferences = CompileResources.PortableExecutableReferences;
            var engine = _compilerService.CreateRazorProjectEngine(executableReferences);
            string fileName = Path.GetRandomFileName();
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var codeInput = JsonConvert.DeserializeObject<CodeInputModel>(requestBody);
            var testCode = codeInput.Solution;
           
            RazorSourceDocument document = RazorSourceDocument.Create(testCode, fileName);

            RazorCodeDocument codeDocument = engine.Process(
                document,
                null,
                new List<RazorSourceDocument>(),
                new List<TagHelperDescriptor>());

            RazorCSharpDocument razorCSharpDocument = codeDocument.GetCSharpDocument();

            var resultCode = razorCSharpDocument.GeneratedCode;
            //var result = await _compilerService.SubmitCode(testCode, executableReferences);

            return new OkObjectResult(resultCode);
        }
        [FunctionName("CompileRazorCode")]
        public async Task<IActionResult> RunRazor(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP/code trigger function processed a request.");

            var executableReferences = CompileResources.PortableExecutableReferences;
            var engine = _compilerService.CreateRazorProjectEngine(executableReferences);
            string fileName = Path.GetRandomFileName();
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var codeInput = JsonConvert.DeserializeObject<CodeFile>(requestBody);
            var testCode = codeInput.Content;
            CodeAssemblyModel assemblyModel = await _compilerService.GetRazorAssembly(codeInput, executableReferences);
            //RazorSourceDocument document = RazorSourceDocument.Create(testCode, fileName);

            //RazorCodeDocument codeDocument = engine.Process(
            //    document,
            //    null,
            //    new List<RazorSourceDocument>(),
            //    new List<TagHelperDescriptor>());

            //RazorCSharpDocument razorCSharpDocument = codeDocument.GetCSharpDocument();

            //var resultCode = razorCSharpDocument.GeneratedCode;
            //var result = await _compilerService.SubmitCode(testCode, executableReferences);

            return new OkObjectResult(assemblyModel);
        }
    }
}
