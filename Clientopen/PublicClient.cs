using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using MasterSharpOpen.Shared.ArenaChallenge;
using MasterSharpOpen.Shared.CodeModels;
using MasterSharpOpen.Shared.UserModels;
using MasterSharpOpen.Shared.VideoModels;
using Newtonsoft.Json;

namespace MasterSharpOpen.Client
{
    public class PublicClient
    {
        //local http://localhost:7071/api
        private const string CHALLENGE_FUNCTION_URL = "https://challengefunction.azurewebsites.net/api";
        private const string COMPILE_FUNCTION_URL = "https://compilefunction.azurewebsites.net/api";
        private const string REALTIME_FUNCTION_URL = "https://csharprealtimefunction.azurewebsites.net/api";
        private const string DUELS_COSMOS_FUNCTION_URL = "https://csharpduels.azurewebsites.net/api";
        //private const string DUELS_COSMOS_FUNCTION_URL = "http://localhost:7071/api";
        public HttpClient Client { get; }

        public PublicClient(HttpClient httpClient)
        {
            Client = httpClient;
        }

        public async Task<List<Arena>> GetActiveArenas()
        {
            var sw = new Stopwatch();
            sw.Start();
            var activeArenas = await Client.GetFromJsonAsync<List<Arena>>($"{DUELS_COSMOS_FUNCTION_URL}/getAllArenas");
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

        //public async Task<List<Arena>> GetUserActiveArenas(int userId)
        //{
        //    var sw = new Stopwatch();
        //    sw.Start();
        //    var activeArenas = await Client.GetFromJsonAsync<List<Arena>>($"{DUELS_COSMOS_FUNCTION_URL}//{userId}");

        //    sw.Stop();
        //    Console.WriteLine($"GetUserActiveArenas {sw.ElapsedMilliseconds}ms");
        //}

        public async Task<bool> UpdateActiveArena(Arena arena)
        {
            var apiResult = await Client.PostAsJsonAsync($"{DUELS_COSMOS_FUNCTION_URL}/joinArena/{arena.Id}/{arena.Name}", arena);
            return apiResult.IsSuccessStatusCode;
        }

        public async Task<bool> CreateActiveArena(Arena arena)
        {
            var random = new Random();
            var id = random.Next(1, 999999);
            arena.Id = id.ToString();
            arena.ChallengeName = arena.CurrentChallenge.Name;
            var apiResult = await Client.PostAsJsonAsync($"{DUELS_COSMOS_FUNCTION_URL}/addArena", arena);
            return apiResult.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteActiveArena(Arena arena)
        {
            var arenaId = arena.Id;
            var arenaName = arena.Name;
            var apiResult = await Client.PostAsJsonAsync($"{DUELS_COSMOS_FUNCTION_URL}/removeArena/{arenaId}/{arenaName}", arenaId);
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
            
            var result = await Client.PostAsJsonAsync($"{CHALLENGE_FUNCTION_URL}/addDuel/{arena.Creator}/{arena.Name}", completedDuel);

            return result.IsSuccessStatusCode;
        }

        public async Task<CodeChallenges> GetChallenges()
        {
            var sw = new Stopwatch();
            sw.Start();
            var codeChallengeList = await Client.GetFromJsonAsync<List<Challenge>>($"{CHALLENGE_FUNCTION_URL}/challenges");
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
            var videos = await Client.GetFromJsonAsync<List<VideoSection>>($"{CHALLENGE_FUNCTION_URL}/videos");
            sw.Stop();
            Console.WriteLine($"videos from function: {sw.ElapsedMilliseconds}ms");
            return new Videos { VideoSections = videos };
        }
        public async Task<UserAppData> GetOrAddUserAppData(string userName)
        {
            var sw = new Stopwatch();
            sw.Start();
            var userstring = await Client.GetStringAsync($"{CHALLENGE_FUNCTION_URL}/users/{userName}");
            Console.WriteLine($"userData = {userstring}");
            var userData = JsonConvert.DeserializeObject<UserAppData>(userstring);
           
            sw.Stop();
            Console.WriteLine($"User from function: {sw.ElapsedMilliseconds}ms");
            return userData;
        }
        public async Task<bool> AddUserSnippet(string userName, UserSnippet snippet)
        {
            var apiResult = await Client.PostAsJsonAsync($"{CHALLENGE_FUNCTION_URL}/addSnippet/{userName}", snippet);
            return apiResult.IsSuccessStatusCode;
        }

        public async Task<bool> AddSuccessfulChallenge(string userName, int challengeId)
        {
            var apiResult = await Client.PostAsJsonAsync($"{CHALLENGE_FUNCTION_URL}/addSnippet/{userName}/{challengeId}","");
            return apiResult.IsSuccessStatusCode;
        }
        public async Task<bool> PostChallenge(Challenge challenge)
        {
            var apiResult = await Client.PostAsJsonAsync($"{CHALLENGE_FUNCTION_URL}/challenge", challenge);
            return apiResult.IsSuccessStatusCode;
        }

        public async Task<bool> PostVideo(Video video)
        {
            var apiResult = await Client.PostAsJsonAsync($"{CHALLENGE_FUNCTION_URL}/video", video);
            return apiResult.IsSuccessStatusCode;
        }
        public async Task<CodeOutputModel> SubmitChallenge(Challenge challenge)
        {
            var sw = new Stopwatch();
            sw.Start();
            var apiResult = await Client.PostAsJsonAsync($"{COMPILE_FUNCTION_URL}/challenge", challenge);
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
            var apiResult = await Client.PostAsJsonAsync($"{COMPILE_FUNCTION_URL}/code", challenge);
            var result = await apiResult.Content.ReadAsStringAsync();
            Console.WriteLine($"code submit too {sw.ElapsedMilliseconds}ms");
            return result;
        }

        public async Task<string> SubmitConsole(string code)
        {
            var sw = new Stopwatch();
            sw.Start();
            var challenge = new Challenge { Solution = code };
            var apiResult = await Client.PostAsJsonAsync($"{COMPILE_FUNCTION_URL}/console", challenge);
            var result = await apiResult.Content.ReadAsStringAsync();
            Console.WriteLine($"code submit too {sw.ElapsedMilliseconds}ms");
            return result;
        }

    }
}
