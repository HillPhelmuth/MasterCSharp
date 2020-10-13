using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using MasterCSharp.Api.Data;
using MasterCSharp.Shared.RazorCompileService;
using MasterCSharp.Shared.UserModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MasterCSharp.Api.Functions.ChallengeFunctions
{
    public class UserProjectFunctions
    {
        private readonly ChallengeContext context;

        public UserProjectFunctions(ChallengeContext context)
        {
            this.context = context;
        }

        [FunctionName("CreateProject")]
        public async Task<IActionResult> AddProject(
           [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "CreateProject/{userName}")]
            HttpRequest req, string userName, ILogger log)
        {
            log.LogInformation($"C# HTTP trigger function processed a request to AddProject");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var newProject = JsonConvert.DeserializeObject<UserProject>(requestBody);
            if (string.IsNullOrEmpty(userName))
                return new BadRequestErrorMessageResult("Parameter UserName is required");
            if (!await context.UserAppData.AnyAsync(x => x.Name == userName))
                return new BadRequestErrorMessageResult("User does not exist in database");
            var currentUser = await context.UserAppData.FirstOrDefaultAsync(x => x.Name == userName);
            if (currentUser.RazorProjects?.Any(x => x.Name == newProject.Name) ?? false)
                newProject.Name = $"{newProject.Name}(copy)";
            newProject.UserAppDataID = currentUser.ID;
            await context.UserProject.AddAsync(newProject);
            await context.SaveChangesAsync();
            return new OkResult();
        }
        [FunctionName("UpdateProject")]
        public async Task<IActionResult> UpdateProject(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "UpdateProject/{userName}/{project}")]
            HttpRequest req, string userName, string project, ILogger log)
        {
            log.LogInformation($"C# HTTP trigger function processed a request to AddProject");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var updatedProjectFiles = JsonConvert.DeserializeObject<List<ProjectFile>>(requestBody);
            if (string.IsNullOrEmpty(userName))
                return new BadRequestErrorMessageResult("Parameter UserName is required");
            if (!await context.UserAppData.AnyAsync(x => x.Name == userName))
                return new BadRequestErrorMessageResult("User does not exist in database");
            var currentUser = await context.UserAppData.FirstOrDefaultAsync(x => x.Name == userName);
            var currentProject = await context.UserProject.FirstOrDefaultAsync(x => x.Name == project && x.UserAppDataID == currentUser.ID);
            if (currentProject == null)
            {
                await context.UserProject.AddAsync(new UserProject
                    {Name = project, UserAppDataID = currentUser.ID, Files = updatedProjectFiles});
                await context.SaveChangesAsync();
                return new OkResult();
            }
            currentProject.Files = updatedProjectFiles;
            await context.SaveChangesAsync();
            return new OkResult();
        }
        [FunctionName("DeleteProject")]
        public async Task<IActionResult> RemoveProject(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "DeleteProject/{userName}/{project}")]
            HttpRequest req, string userName, string project, ILogger log)
        {
            log.LogInformation($"C# HTTP trigger function processed a request to AddProject");

            if (string.IsNullOrEmpty(userName))
                return new BadRequestErrorMessageResult("Parameter UserName is required");
            if (!await context.UserAppData.AnyAsync(x => x.Name == userName))
                return new BadRequestErrorMessageResult("User does not exist in database");
            var currentUser = await context.UserAppData.FirstOrDefaultAsync(x => x.Name == userName);
            var currentProject = await context.UserProject.FirstOrDefaultAsync(x => x.Name == userName && x.UserAppDataID == currentUser.ID);
            if (currentProject == null)
            {
                return new BadRequestErrorMessageResult("Project not found in database");
            }
            context.UserProject.Remove(currentProject);
            await context.SaveChangesAsync();
            return new OkResult();
        }
    }
}
