using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BlazorApp.Api.Functions.Challenge
{
    public static class CodeWarsCompleted
    {
        [FunctionName("CodeWarsCompleted")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "codeWars")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://www.codewars.com/api/v1/users/HillPhelmuth/code-challenges/completed?page=0");
            request.Headers.Add("access_key", "vu7itzrwz-6d7sj2rk2i");
            var response = await client.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();
            
            var cwChallenges = JsonConvert.DeserializeObject<CodeWarsChallenges>(responseString);
            foreach (string chalString in cwChallenges.Challenges.Select(challenge => JsonConvert.SerializeObject(challenge, Formatting.Indented)))
            {
                log.LogInformation(chalString);

            }

            foreach (var cwChallenge in cwChallenges.Challenges)
            {
                var slug = cwChallenge.Slug;
                var challengeRequest = new HttpRequestMessage(HttpMethod.Get, $"https://www.codewars.com/api/v1/code-challenges/{slug}");
                challengeRequest.Headers.Add("access_key", "vu7itzrwz-6d7sj2rk2i");
                var challengeResponse = await client.SendAsync(challengeRequest);
                var chalResponseString = await challengeResponse.Content.ReadAsStringAsync();
                var swChalObject = JsonConvert.DeserializeObject<CwChallenges>(chalResponseString);
                var jsonString = JsonConvert.SerializeObject(swChalObject, Formatting.Indented);
                log.LogInformation(jsonString);
            }

            return new OkObjectResult(cwChallenges);
        }
    }
}
