using System;
using BlazorApp.Api;
using BlazorApp.Api.Data;
using BlazorApp.Api.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;


[assembly: FunctionsStartup(typeof(Startup))]
namespace BlazorApp.Api
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            string connectionString = Environment.GetEnvironmentVariable("SqlConnectionString") ?? "";
            builder.Services.AddDbContext<ChallengeContext>(
                options => options.UseSqlServer(connectionString)
                    .EnableDetailedErrors()
                    .EnableSensitiveDataLogging());
            string connectionStringCosmos = Environment.GetEnvironmentVariable("ConnectionString") ?? "";
            builder.Services.AddSingleton(s => new CosmosClient(connectionStringCosmos));
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddSingleton<CompilerService>();
        }
    }
}
