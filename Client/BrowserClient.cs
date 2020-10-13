using System.Net.Http;

namespace MasterCSharp.Client
{
    public class BrowserClient
    {
        public HttpClient Client { get; }

        public BrowserClient(HttpClient client)
        {
            Client = client;
        }
    }
}
