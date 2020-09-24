using BlazorApp.Shared.CodeModels;
using BlazorApp.Shared.UserModels;
using BlazorApp.Shared.VideoModels;
using Microsoft.EntityFrameworkCore;
//using ArenaDuel = ChallengeFunction.Models.ArenaDuel;

namespace BlazorApp.Api.Data
{
    public class ChallengeContext : DbContext
    {
        public DbSet<Challenge> Challenges { get; set; }
        public DbSet<Test> Tests { get; set; }
        public DbSet<VideoSection> VideoSections { get; set; }
        public DbSet<Video> Videos { get; set; }
        public DbSet<UserAppData> UserAppData { get; set; }
        public DbSet<UserSnippet> UserSnippets { get; set; }
        public DbSet<ArenaDuel> UserDuels { get; set; }
        public ChallengeContext(DbContextOptions<ChallengeContext> options)
            : base(options)
        {

        }
    }
}

