﻿using Duende.IdentityServer.EntityFramework.Options;
using EventsManager.Server.Data.TypeConfigurations;
using EventsManager.Server.Models;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EventsManager.Server.Data;

public class ApplicationDbContext : ApiAuthorizationDbContext<ApplicationUser>
{
    public override DbSet<ApplicationUser> Users { get; set; }  
    public DbSet<Suggestion> Suggestions { get; set; }
    public DbSet<Player> Player { get; set; }
    public DbSet<Match> Match { get; set; }
    public DbSet<Set> Set { get; set; }
    public DbSet<MatchPlayer> MatchPlayer { get; set; }

    public ApplicationDbContext(DbContextOptions options, IOptions<OperationalStoreOptions> operationalStoreOptions) : base(options, operationalStoreOptions)
    {
    }
        
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new SuggestionsTypeConfiguration());
        modelBuilder.ApplyConfiguration(new PlayerConfiguration());
        modelBuilder.ApplyConfiguration(new MatchConfiguration());
        modelBuilder.ApplyConfiguration(new MatchPlayerConfiguration());
        modelBuilder.ApplyConfiguration(new SetConfiguration());
    }
}