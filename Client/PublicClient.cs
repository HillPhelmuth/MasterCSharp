using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BlazorApp.Shared.ArenaChallenge;
using BlazorApp.Shared.CodeModels;
using BlazorApp.Shared.UserModels;
using BlazorApp.Shared.VideoModels;
using Newtonsoft.Json;

namespace BlazorApp.Client
{
    public class PublicClient
    {
        //local http://localhost:7071/api or https://challengefunction.azurewebsites.net/api
        private const string ChallengeFunctionUrl = "api";
        private const string CompileFunctionUrl = "api";
        private const string DuelsCosmosFunctionUrl = "api";
        private const string RealtimeFunctionUrl = "https://csharprealtimefunction.azurewebsites.net/api";
        //private const string DUELS_COSMOS_FUNCTION_URL = "https://csharpduels.azurewebsites.net/api";
        
        public HttpClient Client { get; }

        public PublicClient(HttpClient httpClient)
        {
            Client = httpClient;
        }

        public async Task<List<Arena>> GetActiveArenas()
        {
            var sw = new Stopwatch();
            sw.Start();
            var activeArenas = await Client.GetFromJsonAsync<List<Arena>>($"{DuelsCosmosFunctionUrl}/GetActiveArenas");
            var challenges = await GetChallenges();
            foreach (var activeArena in activeArenas)
            {
                activeArena.CurrentChallenge =
                    challenges.Challenges.FirstOrDefault(x => x.Name == activeArena.ChallengeName);
            }
            sw.Stop();
            Console.WriteLine($"GetActiveArenas: {sw.ElapsedMilliseconds}ms");
            return activeArenas;
        }

        public async Task<bool> UpdateActiveArena(Arena arena)
        {
            var apiResult = await Client.PostAsJsonAsync($"{DuelsCosmosFunctionUrl}/joinArena/{arena.Id}/{arena.Name}", arena);
            return apiResult.IsSuccessStatusCode;
        }

        public async Task<bool> CreateActiveArena(Arena arena)
        {
            var random = new Random();
            var id = random.Next(1, 999999);
            arena.Id = id.ToString();
            arena.ChallengeName = arena.CurrentChallenge.Name;
            var apiResult = await Client.PostAsJsonAsync($"{DuelsCosmosFunctionUrl}/AddActiveArenas", arena);
            return apiResult.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteActiveArena(Arena arena)
        {
            var arenaId = arena.Id;
            var arenaName = arena.Name;
            var apiResult = await Client.PostAsJsonAsync($"{DuelsCosmosFunctionUrl}/removeArena/{arenaId}/{arenaName}", arenaId);
            return  apiResult.IsSuccessStatusCode;
        }
        public async Task<bool> AddCompleteDuel(Arena arena, bool isWon)
        {
            var completedDuel = new ArenaDuel
            {
                ChallengeName = arena.CurrentChallenge?.Name,
                WonDuel = isWon,
                Solution = arena.CurrentChallenge?.Solution,
                DuelName = arena.Name,
                RivalId = arena.Opponent,
                TimeCompleted = DateTime.Now
            };
            
            var result = await Client.PostAsJsonAsync($"{ChallengeFunctionUrl}/addDuel/{arena.Creator}/{arena.Name}", completedDuel);

            return result.IsSuccessStatusCode;
        }

        public async Task<CodeChallenges> GetChallenges()
        {
            var sw = new Stopwatch();
            sw.Start();
            var codeChallengeList = await Client.GetFromJsonAsync<List<Challenge>>($"{ChallengeFunctionUrl}/GetChallenges");
            sw.Stop();
            Console.WriteLine($"challenges from function: {sw.ElapsedMilliseconds}ms");
            var codeChallenges = new CodeChallenges { Challenges = codeChallengeList };
            Console.WriteLine($"challenges from function: {string.Join(", ", codeChallenges.Challenges.Select(x => x.Name))}");
            return codeChallenges;
        }

        public async Task<Videos> GetVideos()
        {
            var sw = new Stopwatch();
            sw.Start();
            var videos = await Client.GetFromJsonAsync<List<VideoSection>>($"{ChallengeFunctionUrl}/videos");
            sw.Stop();
            Console.WriteLine($"videos from function: {sw.ElapsedMilliseconds}ms");
            return new Videos { VideoSections = videos };
        }
        public async Task<UserAppData> GetOrAddUserAppData(string userName)
        {
            var sw = new Stopwatch();
            sw.Start();
            var userstring = await Client.GetStringAsync($"{ChallengeFunctionUrl}/users/{userName}");
            Console.WriteLine($"userData = {userstring}");
            var userData = JsonConvert.DeserializeObject<UserAppData>(userstring);
           
            sw.Stop();
            Console.WriteLine($"User from function: {sw.ElapsedMilliseconds}ms");
            return userData;
        }
        public async Task<bool> AddUserSnippet(string userName, UserSnippet snippet)
        {
            var apiResult = await Client.PostAsJsonAsync($"{ChallengeFunctionUrl}/addSnippet/{userName}", snippet);
            return apiResult.IsSuccessStatusCode;
        }

        public async Task<bool> AddSuccessfulChallenge(string userName, int challengeId)
        {
            var apiResult = await Client.PostAsJsonAsync($"{ChallengeFunctionUrl}/addSnippet/{userName}/{challengeId}","");
            return apiResult.IsSuccessStatusCode;
        }
        public async Task<bool> PostChallenge(Challenge challenge)
        {
            var apiResult = await Client.PostAsJsonAsync($"{ChallengeFunctionUrl}/challenge", challenge);
            return apiResult.IsSuccessStatusCode;
        }

        public async Task<bool> PostVideo(Video video)
        {
            var apiResult = await Client.PostAsJsonAsync($"{ChallengeFunctionUrl}/video", video);
            return apiResult.IsSuccessStatusCode;
        }
        public async Task<CodeOutputModel> SubmitChallenge(Challenge challenge)
        {
            var sw = new Stopwatch();
            sw.Start();
            var apiResult = await Client.PostAsJsonAsync($"{CompileFunctionUrl}/challenge", challenge);
            var result = await apiResult.Content.ReadAsStringAsync();
            sw.Stop();
            Console.WriteLine($"challenge submit too {sw.ElapsedMilliseconds}ms");
            var output = JsonConvert.DeserializeObject<CodeOutputModel>(result);
            return output;
        }

        public async Task<string> SubmitCode(string code)
        {
            var sw = new Stopwatch();
            sw.Start();
            var challenge = new Challenge { Solution = code };
            var apiResult = await Client.PostAsJsonAsync($"{CompileFunctionUrl}/code", challenge);
            var result = await apiResult.Content.ReadAsStringAsync();
            Console.WriteLine($"code submit too {sw.ElapsedMilliseconds}ms");
            return result;
        }

        public async Task<string> SubmitConsole(string code)
        {
            var sw = new Stopwatch();
            sw.Start();
            var challenge = new Challenge { Solution = code };
            var apiResult = await Client.PostAsJsonAsync($"{CompileFunctionUrl}/console", challenge);
            var result = await apiResult.Content.ReadAsStringAsync();
            Console.WriteLine($"code submit too {sw.ElapsedMilliseconds}ms");
            return result;
        }
        public async Task SendMessage(string userName, string messageInput)
        {
            await Client.PostAsJsonAsync($"{RealtimeFunctionUrl}/messages/{userName}", messageInput);
        }

        public async Task SendSnippet(string snippet, string otherUser)
        {
            await Client.PostAsJsonAsync($"{RealtimeFunctionUrl}/sendCode/{otherUser}", snippet);
        }

        public async Task JoinGroup(string groupName, string userName)
        {
            await Client.PostAsJsonAsync($"{RealtimeFunctionUrl}/joinGroup/{groupName}/{userName}", userName);
        }

        public async Task MessageGroup(string groupName, string messageInput)
        {
            await Client.PostAsJsonAsync($"{RealtimeFunctionUrl}/groupMessage/{groupName}", messageInput);
        }

        public async Task SendCodeOutput(string group, string output)
        {
            await Client.PostAsJsonAsync($"{RealtimeFunctionUrl}/sendOut/{group}", output);
        }
    }
}
