using System;
using MasterCSharp.Api;
using MasterCSharp.Api.Data;
using MasterCSharp.Api.Services;
using MasterCSharp.Shared.CodeServices;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]
namespace MasterCSharp.Api
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
            builder.Services.AddSingleton<RazorCompile>();
        }
    }
}
