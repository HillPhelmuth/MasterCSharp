using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using BlazorApp.Shared.ArenaChallenge;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json; //using CSharpDuels.DataContext;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore;

namespace BlazorApp.Api.Functions.Duels
{
    public class ActiveArenaFunction
    {
        //private readonly CSharpDuelsDbContext _context;
        private readonly Container _database;
        public ActiveArenaFunction(CosmosClient client)
        {
            //_context = context;
            _database = client.GetContainer("ActiveArenasDb", "ActiveArenas");
        }

        [FunctionName("GetActiveArenas")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "getAllArenas")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request for GetActiveArenas.");


            var arenasIterator = _database.GetItemLinqQueryable<Arena>().ToFeedIterator();
            var arenaList = new List<Arena>();
            while (arenasIterator.HasMoreResults)
            {
                var resultSet = await arenasIterator.ReadNextAsync();
                arenaList.AddRange(resultSet);
            }
            return new OkObjectResult(arenaList);
        }
        [FunctionName("AddActiveArenas")]
        public async Task<IActionResult> AddArena(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "addArena")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request for AddActiveArenas.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var arena = JsonConvert.DeserializeObject<Arena>(requestBody);
            log.LogInformation($"Arena Created: {requestBody}");
            await _database.CreateItemAsync(arena);
            await SendAppHubAlert("Add");

            return new OkResult();
        }
        [FunctionName("UpdateActiveArenas")]
        public async Task<IActionResult> UpdateArena(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "joinArena/{arenaId}/{name}")] HttpRequest req,
            string arenaId, string name, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request for UpdateActiveArenas.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var arena = JsonConvert.DeserializeObject<Arena>(requestBody);
            var partitionKey = new PartitionKey(name);
            var response = await _database.UpsertItemAsync(arena);
            log.LogInformation($"Upsert Status Code: {response.StatusCode} For Arena: {response.Resource?.Name}");
            await SendAppHubAlert("Update");
            return new OkResult();
        }
        [FunctionName("RemoveActiveArenas")]
        public async Task<IActionResult> RemoveArena(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "removeArena/{arenaId}/{name}")] HttpRequest req,
            string arenaId, string name, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request for RemoveActiveArenas.");

            var response = await _database.DeleteItemAsync<Arena>(arenaId, new PartitionKey(name));

            log.LogInformation($"Arena Remove: {response.StatusCode} for Arena: {response.Resource?.Name}");
            await SendAppHubAlert("Remove");
            return new OkResult();
        }

        private async Task SendAppHubAlert(string action)
        {
            const string FunctionBaseUrl = "https://csharpduelshubfunction.azurewebsites.net/api";
            using var client = new HttpClient();
            var url = $"{FunctionBaseUrl}/alert/";
            var message = $"";
            await client.PostAsJsonAsync(url, message);
        }

    }
}
