using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using BlazorApp.Api.Data;
using BlazorApp.Shared.VideoModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BlazorApp.Api.Functions.ChallengeFunctions
{
    public class ChallengeDbFunction
    {
        private readonly ChallengeContext context;

        public ChallengeDbFunction(ChallengeContext context)
        {
            this.context = context;
        }

        [FunctionName("GetChallenges")]
        public async Task<IActionResult> GetChallenges(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP GET/challenges trigger function processed a request.");

            var challenges = await context.Challenges.ToListAsync();
            var tests = await context.Tests.ToListAsync();
            foreach (var challenge in challenges)
            {
                challenge.Tests = tests.Where(x => x.ChallengeID == challenge.ID).ToList();
            }
            JsonSerializerSettings settings = new JsonSerializerSettings(){ReferenceLoopHandling = ReferenceLoopHandling.Ignore};
            var logString = JsonConvert.SerializeObject(challenges, Formatting.Indented, settings);
            log.LogInformation(logString);
            return new OkObjectResult(challenges);
        }
        [FunctionName("AddChallenge")]
        public async Task<IActionResult> AddChallenge(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", "post", Route = "Addchallenge")] HttpRequest req, 
            CancellationToken cts,
            ILogger log)
        {
            log.LogInformation("C# HTTP POST/challenge trigger function processed a request.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var challenge = JsonConvert.DeserializeObject<Shared.CodeModels.Challenge>(requestBody);
            if (challenge.ID > 0)
            {
                return new BadRequestErrorMessageResult("Challenge already exists in database");
            }

            await context.Challenges.AddAsync(challenge, cts);
            foreach (var test in challenge.Tests)
            {
                test.ChallengeID = challenge.ID;
                await context.Tests.AddAsync(test, cts);
            }
            await context.SaveChangesAsync(cts);

            return new OkResult();

        }
        [FunctionName("GetVideos")]
        public async Task<IActionResult> GetVideos(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "videos")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP GET/Videos trigger function processed a request.");

            var videoSections = await context.VideoSections.ToListAsync();
            
            var videos = await context.Videos.ToListAsync();
            foreach (var section in videoSections)
            {
                section.Videos = videos.Where(x => x.VideoSectionID == section.ID).ToList();
            }
            return new OkObjectResult(videoSections);
        }
        [FunctionName("AddVideo")]
        public async Task<IActionResult> AddVideo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", "post", Route = "video")] HttpRequest req,
            CancellationToken cts,
            ILogger log)
        {
            log.LogInformation("C# HTTP POST/video trigger function processed a request.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var video = JsonConvert.DeserializeObject<Video>(requestBody);

            if (video.ID > 0)
            {
                return new BadRequestErrorMessageResult("Video already exists in database");
            }

            if (video.VideoSectionID == 0)
                video.VideoSectionID = await context.VideoSections.Where(x => x.Name == "User Videos").Select(x => x.ID).FirstOrDefaultAsync(cts);

            await context.Videos.AddAsync(video, cts);

            await context.SaveChangesAsync(cts);

            return new OkResult();

        }

        private (string name, string subheader) GetSectionValues(string sectionName)
        {
            if (!sectionName.Contains('-')) return ("User Videos", "Added by app users");
            var splitNames = sectionName.Split(',');
            return (splitNames[0], splitNames[1]);
        }
    }
}
