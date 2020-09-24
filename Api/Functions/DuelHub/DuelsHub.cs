using System;
using System.IO;
using System.Threading.Tasks;
using BlazorApp.Shared.UserModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BlazorApp.Api.Functions.DuelHub
{
    public class DuelsHub
    {
        [FunctionName("negotiate")]
        public static SignalRConnectionInfo Negotiate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
            [SignalRConnectionInfo(HubName = "duels", UserId = "{headers.x-ms-client-principal-id}")] SignalRConnectionInfo connectionInfo)
        {
            Console.WriteLine($"url: {connectionInfo.Url} token: {connectionInfo.AccessToken}");
            return connectionInfo;
        }
        [FunctionName("alerts")]
        public static Task SendAlert(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "alerts/{user}")] object message, string user,
            [SignalR(HubName = "duels")] IAsyncCollector<SignalRMessage> signalRMessages)
        {
            Console.WriteLine($"message: {JsonConvert.SerializeObject(message)}");
            var messageString = $"{user}::{message}";
            return signalRMessages.AddAsync(
                new SignalRMessage
                {
                    Target = "getAlert",
                    Arguments = new object[] { messageString }
                });
        }
        [FunctionName("joinDuel")]
        public static Task JoinDuel(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "joinDuel/{groupName}/{userName}")] HttpRequest req, string groupName, string userName,
            [SignalR(HubName = "duels")] IAsyncCollector<SignalRGroupAction> signalRGroupActions)
        {
            Console.WriteLine($"{userName} joined group {groupName}");
            return signalRGroupActions.AddAsync(
                new SignalRGroupAction
                {
                    UserId = userName,
                    GroupName = groupName,
                    Action = GroupAction.Add
                });
        }
        [FunctionName("joinAlert")]
        public static Task JoinAlert(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "joinAlert/{groupName}/{userName}")] HttpRequest req, string groupName, string userName,
            [SignalR(HubName = "duels")] IAsyncCollector<SignalRMessage> signalRMessages)
        {
            Console.WriteLine($"{userName} joined group {groupName}");
            var jsonObject = new JObject { { "group", groupName }, { "user", userName }, {"message", $"{userName} has joined Arena: {groupName}"} };
            return signalRMessages.AddAsync(
                new SignalRMessage
                {
                    Target = "joinAlert",
                    Arguments = new object[] { jsonObject }
                });
        }
        [FunctionName("duelResult")]
        public async Task<IActionResult> DuelResult([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "duelResult/{group}/{message}")] HttpRequest req, string group, string message,
            [SignalR(HubName = "duels")] IAsyncCollector<SignalRMessage> signalRMessages)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var arenaResult = JsonConvert.DeserializeObject<ArenaResult>(requestBody);
            Console.WriteLine($"Log message: {message}");
            var alertAllString = $"{group} has ended";
            var messageJObject = new JObject { { "group", group }, { "message", message }, {"end", true}, {"duelWinner", arenaResult.DuelWinner}, {"duelLoser", arenaResult.DuelLoser} };
            await signalRMessages.AddAsync(
                new SignalRMessage
                {
                    GroupName = group,
                    Target = "resultAlert",
                    Arguments = new object[] { messageJObject }
                });
            await signalRMessages.AddAsync(
                new SignalRMessage
                {
                    GroupName = group,
                    Target = "resultActual",
                    Arguments = new object[] { arenaResult }
                });
            await signalRMessages.AddAsync(
                new SignalRMessage
                {
                    Target = "getAlert",
                    Arguments = new object[] { alertAllString }
                });
            return new OkObjectResult(signalRMessages);
        }
        [FunctionName("duelAttempt")]
        public static Task DuelAttempt([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "duelAttempt/{group}")]
            object message, string group,
            [SignalR(HubName = "duels")] IAsyncCollector<SignalRMessage> signalRMessages)
        {
            Console.WriteLine($"message: {JsonConvert.SerializeObject(message)}");
            var jsonObject = new JObject {{"group", group}, {"message", message as JToken}};
            return signalRMessages.AddAsync(
                new SignalRMessage
                {
                    GroupName = group,
                    Target = "resultAlert",
                    Arguments = new object[] { jsonObject }
                });
        }
        [FunctionName("create")]
        public static Task SendNewArena(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "create/{group}/{user}/{challenge}")] object message, string user, string group, string challenge, [SignalR(HubName = "duels")] IAsyncCollector<SignalRMessage> signalRMessages)
        {
            Console.WriteLine($"message: {user} created arena {group}");
           var jsonObject = new JObject {{"user", user}, {"group", group}, {"challenge", challenge}};
            return signalRMessages.AddAsync(
                new SignalRMessage
                {
                    Target = "showArena",
                    Arguments = new object[] { jsonObject }
                });
        }
        [FunctionName("leaveDuel")]
        public static Task LeaveDuel([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "leaveDuel/{groupName}/{userName}")] HttpRequest req, string groupName, string userName,
            [SignalR(HubName = "duels")] IAsyncCollector<SignalRGroupAction> signalRGroupActions)
        {
            Console.WriteLine($"{userName} left group {groupName}");
            return signalRGroupActions.AddAsync(
                new SignalRGroupAction
                {
                    UserId = userName,
                    GroupName = groupName,
                    Action = GroupAction.Remove
                });
        }
        [FunctionName("leave")]
        public static Task LeaveDuelMessage(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "leave/{group}/{user}")] object message, string user, string group, [SignalR(HubName = "duels")] IAsyncCollector<SignalRMessage> signalRMessages)
        {
            Console.WriteLine($"message: {user} created arena {group}");
            var messageString = $"{user} Removed Arena: {group}";
            return signalRMessages.AddAsync(
                new SignalRMessage
                {
                    Target = "leaveArena",
                    Arguments = new object[] { messageString }
                });
        }
        [FunctionName("AlertUpdate")]
        public static Task AlertUpdate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "alert")] object message,
            [SignalR(HubName = "duels")] IAsyncCollector<SignalRMessage> signalRMessages)
        {
            
            var messageString = $"arena: Alert";
            return signalRMessages.AddAsync(
                new SignalRMessage
                {
                    Target = "alertArena",
                    Arguments = new object[] { messageString }
                });
        }
    }
}