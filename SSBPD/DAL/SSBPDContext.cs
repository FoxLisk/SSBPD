using System;
using System.Collections.Generic;
using System.Data.Entity;
using SSBPD.Models;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace SSBPD.Models
{
    public class SSBPDContext : DbContext, IDisposable
    {
        public DbSet<Player> Players { get; set; }
        public DbSet<Set> Sets { get; set; }
        public DbSet<Tournament> Tournaments { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<TournamentFile> TournamentFiles { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<PlayerFlag> PlayerFlags { get; set; }
        public DbSet<EloScore> EloScores { get; set; }
        public DbSet<RegionFlag> RegionFlags { get; set; }
        public DbSet<CharacterFlag> CharacterFlags { get; set; }
        public DbSet<CustomRegion> CustomRegions { get; set; }
        public DbSet<SetLink> SetLinks { get; set; }
        public DbSet<SetLinkFlag> SetLinkFlags { get; set; }
        public DbSet<LogMessage> LogMessages { get; set; }

    }
}