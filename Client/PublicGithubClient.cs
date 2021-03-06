﻿using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace BlazorApp.Client
{
    public class PublicGithubClient
    {
        private readonly HttpClient client;
        private readonly string baseUrl = @"https://api.github.com/repos";

        private readonly string reposUrl =
            @"/HillPhelmuth/NakedCodeSnippets/contents/NakedCodeSnippets";
        public PublicGithubClient(HttpClient client)
        {
            this.client = client;
            client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3.raw");
        }
        public async Task<string> CodeFromGithub(string filename)
        {
            var sw = new Stopwatch();
            sw.Start();
            var code = await client.GetStringAsync($"{baseUrl}{reposUrl}/{filename}.cs");
            sw.Stop();
            Console.WriteLine($"Retrieved code from Github in {sw.ElapsedMilliseconds}ms");
            return code;
        }

        public async Task<string> CodeFromPublicRepo(string githubName, string repoName, string filepath)
        {
            if (!filepath.Contains("."))
            {
                return "Nope!, provide a file extension. I suggest '.cs'";
            }
            var sw = new Stopwatch();
            sw.Start();
            var code = await client.GetStringAsync($"{baseUrl}/{githubName}/{repoName}/contents/{filepath}");
            sw.Stop();
            Console.WriteLine($"Retrieved code from Github in {sw.ElapsedMilliseconds}ms");
            return code;
        }
    }
}
