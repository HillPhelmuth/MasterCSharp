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

namespace BlazorApp.Api.Functions.Compile
{
    public class CompileConsole
    {
        private readonly CompilerService _compilerService;

        public CompileConsole(CompilerService compilerService)
        {
            _compilerService = compilerService;
        }

        [FunctionName("CompileConsole")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "console")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP/console trigger function processed a request.");

            var executableReferences = CompileResources.PortableExecutableReferences;
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var codeInput = JsonConvert.DeserializeObject<CodeInputModel>(requestBody);
            var testCode = codeInput.Solution;
            var result = await _compilerService.RunConsole(testCode, executableReferences);

            return new OkObjectResult(result);
        }
    }
}
