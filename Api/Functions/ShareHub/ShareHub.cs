using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Newtonsoft.Json;

namespace MasterCSharp.Api.Functions.ShareHub
{
    public class ShareHub : ServerlessHub
    {
        [FunctionName("negotiate")]
        public static SignalRConnectionInfo GetSignalRInfo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
            [SignalRConnectionInfo(HubName = "share", UserId = "{headers.x-ms-client-principal-id}")] SignalRConnectionInfo connectionInfo)
        {
            
            Console.WriteLine($"url: {connectionInfo.Url} token: {connectionInfo.AccessToken}");
            return connectionInfo;
        }

        [FunctionName("messages")]
        public static Task SendMessage(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "messages/{user}")] object message, string user,
            [SignalR(HubName = "share")] IAsyncCollector<SignalRMessage> signalRMessages)
        {
            Console.WriteLine($"message: {JsonConvert.SerializeObject(message)}");
            var messageString = $"{user}::{message}";
            return signalRMessages.AddAsync(
                new SignalRMessage
                {
                    Target = "newMessage",
                    Arguments = new[] { messageString }
                });
        }
        [FunctionName("sendCode")]
        public static Task SendCode(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "sendCode/{user}")] object snippet, string user,
            [SignalR(HubName = "share")] IAsyncCollector<SignalRMessage> signalRMessages)
        {
           
            Console.WriteLine($"snippet: {snippet}");
            return signalRMessages.AddAsync(
                new SignalRMessage
                {
                    UserId = user,
                    Target = "newCode",
                    Arguments = new[] { snippet }
                });
        }
        [FunctionName("sendOut")]
        public static Task SendOutput(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "sendOut/{groupName}")] object snippet, 
            string groupName,
            [SignalR(HubName = "share")] IAsyncCollector<SignalRMessage> signalRMessages)
        {
            
            Console.WriteLine($"message: {JsonConvert.SerializeObject(snippet)}");

            return signalRMessages.AddAsync(
                new SignalRMessage
                {
                    GroupName = groupName,
                    Target = "newOut",
                    Arguments = new[] { snippet }
                });
        }

        [FunctionName("joinGroup")]
        public static Task JoinGroup([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "joinGroup/{groupName}/{userName}")] HttpRequest req, string groupName, string userName,
           [SignalR(HubName = "share")] IAsyncCollector<SignalRGroupAction> signalRGroupActions)
        {
           
            return signalRGroupActions.AddAsync(
                new SignalRGroupAction
                {
                    UserId = userName,
                    GroupName = groupName,
                    Action = GroupAction.Add
                });
        }
        [FunctionName("groupMessage")]
        public static Task SendGroupMessage(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "groupMessage/{groupName}")] object message, 
            string groupName, [SignalR(HubName = "share")] IAsyncCollector<SignalRMessage> signalRMessages)
        {
            Console.WriteLine($"{groupName}::{message}");
            var messageString = $"{groupName}::{message}";
            return signalRMessages.AddAsync(
                new SignalRMessage
                {
                    GroupName = groupName,
                    Target = "groupMessage",
                    Arguments = new object[] { messageString }
                });
        }
        [FunctionName("privateMessage")]
        public static Task SendUserMessage(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "privateMessage/{user}")] object message,
            string user, [SignalR(HubName = "share")] IAsyncCollector<SignalRMessage> signalRMessages)
        {
            Console.WriteLine($"private user message: \r\n from: {user} message: {message}");
            var sendMessage = $"{user}::{message}";

            return signalRMessages.AddAsync(
                new SignalRMessage
                {
                    UserId = user,
                    Target = "privateMessage",
                    Arguments = new object[] { sendMessage }
                });
        }

       
    }
}
