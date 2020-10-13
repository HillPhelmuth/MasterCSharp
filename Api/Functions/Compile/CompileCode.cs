using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MasterCSharp.Api.Services;
using MasterCSharp.Shared.CodeModels;
using MasterCSharp.Shared.CodeServices;
using MasterCSharp.Shared.RazorCompileService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MasterCSharp.Api.Functions.Compile
{
    public class CompileCode
    {
        private readonly CompilerService _compilerService;
        private readonly RazorCompile _razorCompile;
      
        public CompileCode(CompilerService compilerService, RazorCompile razorCompile)
        {
            _compilerService = compilerService;
            _razorCompile = razorCompile;
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
        //[FunctionName("CompileRazorText")]
        //public async Task<IActionResult> RunRazorText(
        //    [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
        //    ILogger log)
        //{
        //    log.LogInformation("C# HTTP/code trigger function processed a request.");

        //    var executableReferences = CompileResources.PortableExecutableReferences;
        //    var engine = _compilerService.CreateRazorProjectEngine(executableReferences);
        //    string fileName = Path.GetRandomFileName();
        //    string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        //    var codeInput = JsonConvert.DeserializeObject<CodeInputModel>(requestBody);
        //    var testCode = codeInput.Solution;

        //    RazorSourceDocument document = RazorSourceDocument.Create(testCode, fileName);

        //    RazorCodeDocument codeDocument = engine.Process(
        //        document,
        //        null,
        //        new List<RazorSourceDocument>(),
        //        new List<TagHelperDescriptor>());

        //    RazorCSharpDocument razorCSharpDocument = codeDocument.GetCSharpDocument();

        //    var resultCode = razorCSharpDocument.GeneratedCode;
        //    //var result = await _compilerService.SubmitCode(testCode, executableReferences);

        //    return new OkObjectResult(resultCode);
        //}
        [FunctionName("CompileRazorCode")]
        public async Task<IActionResult> RunRazor(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP/code trigger function processed a request.");
            var executableReferences = await _razorCompile.GetRequiredReferences();
            //var razorexecutableReferences = CompileResources.PortableRazorReferences;
            //executableReferences.AddRange(razorexecutableReferences);
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var codeFiles = JsonConvert.DeserializeObject<List<ProjectFile>>(requestBody);
            
            //var testCode = codeInput.Content;
            //Task initTask = RazorCompile.InitAsync(httpClient);
            //await initTask;
            //while (!initTask.IsCompletedSuccessfully)
            //{
            //    await Task.Delay(10);
            //}
            Console.WriteLine($"MetadataRefs: {string.Join(",", executableReferences.Select(x => x.Display))}");
            var assemblyModel = await _razorCompile.GetRazorProjectAssembly(codeFiles, executableReferences);
            var fileBytes = assemblyModel.AssemblyBytes;
            var diags = assemblyModel.Diagnostics;
            var assembly = new CodeAssemblyModel{ Compilation = null ,AssemblyBytes = fileBytes, Diagnostics = diags};
            //CodeAssemblyModel assemblyModel = await _compilerService.GetRazorAssembly(codeInput, executableReferences);
            //RazorSourceDocument document = RazorSourceDocument.Create(testCode, fileName);

            //RazorCodeDocument codeDocument = engine.Process(
            //    document,
            //    null,
            //    new List<RazorSourceDocument>(),
            //    new List<TagHelperDescriptor>());

            //RazorCSharpDocument razorCSharpDocument = codeDocument.GetCSharpDocument();

            //var resultCode = razorCSharpDocument.GeneratedCode;
            //var result = await _compilerService.SubmitCode(testCode, executableReferences);

            return new OkObjectResult(assembly);
        }
    }
}
