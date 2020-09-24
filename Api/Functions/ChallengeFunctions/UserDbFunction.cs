using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using BlazorApp.Api.Data;
using BlazorApp.Shared.UserModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BlazorApp.Api.Functions.ChallengeFunctions
{
    public class UserDbFunction
    {
        private readonly ChallengeContext context;
        private const string FunctionBaseUrl = "https://csharpduelshubfunction.azurewebsites.net/api";
        public UserDbFunction(ChallengeContext context)
        {
            this.context = context;
        }

        [FunctionName("GetUserData")]
        public async Task<IActionResult> GetUser(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "users/{userName}")] HttpRequest req,
            ILogger log, string userName)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var users = await context.UserAppData.ToListAsync();
            foreach (var user in users)
            {
                log.LogInformation($"{user.Name} found");
            }
            if (users.Any(x => x.Name == userName))
            {
                var currentUser = users.FirstOrDefault(x => x.Name == userName);
                var userSnippets = await context.UserSnippets.ToListAsync();
                var userDuels = await context.UserDuels.ToListAsync();
                currentUser.Snippets = userSnippets.Where(x => x.UserAppDataID == currentUser.ID).ToList();
                currentUser.CompletedDuels = userDuels.Where(x => x.UserAppDataID == currentUser.ID).ToList();
                JsonSerializerSettings settings = new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
                var logString = JsonConvert.SerializeObject(currentUser, Formatting.Indented, settings);
                log.LogInformation(logString);
                return new OkObjectResult(currentUser);
            }
            
            await context.UserAppData.AddAsync(new UserAppData { Name = userName });
            await context.SaveChangesAsync();
            return new OkObjectResult(new UserAppData { Name = userName });
        }

        
        [FunctionName("AddUserSnippet")]
        public async Task<IActionResult> AddSnippet(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "addSnippet/{userName}")]
            HttpRequest req, string userName,
            ILogger log)
        {
            log.LogInformation($"C# HTTP trigger function processed a request.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var snippet = JsonConvert.DeserializeObject<UserSnippet>(requestBody);
            if (string.IsNullOrEmpty(userName))
                return new BadRequestErrorMessageResult("Parameter UserName is required");
            if (!await context.UserAppData.AnyAsync(x => x.Name == userName))
                return new BadRequestErrorMessageResult("User does not exist in database");
            var currentUser = await context.UserAppData.FirstOrDefaultAsync(x => x.Name == userName);
            snippet.UserAppDataID = currentUser.ID;
            await context.UserSnippets.AddAsync(snippet);
            await context.SaveChangesAsync();
            return new OkResult();

        }
        [FunctionName("AddUserDuel")]
        public async Task<IActionResult> AddDuel(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "addDuel/{userName}/{arenaName}")]
            HttpRequest req, string userName, string arenaName,
            ILogger log)
        {
            log.LogInformation($"C# HTTP trigger function processed a request.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var completedDuel = JsonConvert.DeserializeObject<ArenaDuel>(requestBody);
            if (string.IsNullOrEmpty(userName))
                return new BadRequestErrorMessageResult("Parameter UserName is required");
            if (!await context.UserAppData.AnyAsync(x => x.Name == userName))
                return new BadRequestErrorMessageResult("User does not exist in database");
            var currentUser = await context.UserAppData.FirstOrDefaultAsync(x => x.Name == userName);
            completedDuel.UserAppDataID = currentUser.ID;
            var userWon = completedDuel.WonDuel;
            var userId = userName;
            var output = userWon ? $"{userId} Defeated {completedDuel.RivalId} in challenge {completedDuel.ChallengeName}!" : $"{completedDuel?.RivalId} Defeated {userId} in challenge {completedDuel?.ChallengeName}!";
            
            var arenaResult = new ArenaResultMessage
            {
                DuelWinner = completedDuel.WonDuel ? userName : completedDuel.RivalId,
                DuelLoser = completedDuel.WonDuel ? completedDuel.RivalId : userName
            };
            var client = new HttpClient();
            var url = $"{FunctionBaseUrl}/alerts/DataBase Function";
            var resultUrl = $"{FunctionBaseUrl}/duelResult/{arenaName}/{output}";
            log.LogInformation($"Result posted to Hub: \r\n url: {resultUrl} \r\n arenaResult: {JObject.FromObject(arenaResult)}");
            var message = $"{output}";
            await client.PostAsJsonAsync(url, message);
            await client.PostAsJsonAsync(resultUrl, arenaResult);
            await context.UserDuels.AddAsync(completedDuel);
            await context.SaveChangesAsync();
            return new OkResult();

        }
        [FunctionName("UpdateUserChallenge")]
        public async Task<IActionResult> UpdateUserChallenge(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "addSnippet/{userName}/{challengeId}")]
            HttpRequest req, int challengeId, string userName,
            ILogger log)
        {
            log.LogInformation($"C# HTTP trigger function processed a request to updateUserChallenge (challengeId: {challengeId})");
            if (string.IsNullOrEmpty(userName))
                return new BadRequestErrorMessageResult("Parameter UserName is required");
            if (!await context.UserAppData.AnyAsync(x => x.Name == userName))
                return new BadRequestErrorMessageResult("User does not exist in database");
            var currentUser = await context.UserAppData.FirstOrDefaultAsync(x => x.Name == userName);
            var currentChallengeString = currentUser.ChallengeSuccessData ?? "";
            currentUser.ChallengeSuccessData = $"{currentChallengeString},{challengeId}";
            await context.SaveChangesAsync();
            return new OkResult();
        }
    }
}
