using BlazorApp.Shared.ArenaChallenge;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp.Api.Data
{
    public class CSharpDuelsDbContext : DbContext
    {
        //public DbSet<UserDuel> Duels { get; set; }
        //public DbSet<CompletedDuel> CompletedDuels { get; set; }
        public DbSet<Arena> ActiveArenas { get; set; }
        public CSharpDuelsDbContext(DbContextOptions<CSharpDuelsDbContext> options)
            :base(options){}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new DuelEntityConfig());
            
        }
    }
}
