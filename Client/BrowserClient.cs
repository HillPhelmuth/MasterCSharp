using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BlazorApp.Client
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
