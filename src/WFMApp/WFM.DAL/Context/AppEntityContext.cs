using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

using WFM.DAL.Entities;

namespace WFM.DAL.Context
{
    public class AppEntityContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        // DbSets
        public DbSet<User> User { get; set; }

        public DbSet<Team> Team { get; set; }

        public DbSet<TimeOffRequest> TimeOffRequest { get; set; }

        public DbSet<Approval> Approvals { get; set; }

        public DbSet<DaysOffLimitDefault> DaysOffLimitDefault { get; set; }


        public AppEntityContext(DbContextOptions<AppEntityContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User
            modelBuilder.Entity<User>().HasMany(x => x.Teams).WithMany(x => x.TeamMembers);
            modelBuilder.Entity<User>().HasQueryFilter(p => !p.IsDeleted);

            // DaysOffLimitDefault
            modelBuilder.Entity<DaysOffLimitDefault>().HasMany(d => d.Users).WithOne(u => u.DaysOffLimitDefault);

            // TimeOffRequest
            modelBuilder.Entity<TimeOffRequest>().HasQueryFilter(p => !p.IsDeleted);

            //Team
            modelBuilder.Entity<Team>().HasQueryFilter(p => !p.IsDeleted);
            modelBuilder.Entity<Team>().HasMany(u => u.TeamMembers).WithMany(t => t.Teams);
            modelBuilder.Entity<Team>().HasQueryFilter(p => !p.IsDeleted);

            //Approval
            modelBuilder.Entity<Approval>().HasQueryFilter(p => !p.IsDeleted);
            modelBuilder.Entity<Approval>().HasKey(sc => new { sc.TimeOffRequestId, sc.TeamLeaderId });
        }
    }
}
