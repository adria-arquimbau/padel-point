using EventsManager.Server.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventsManager.Server.Data.TypeConfigurations;

public class SuggestionsTypeConfiguration : IEntityTypeConfiguration<Suggestion>
{
    public void Configure(EntityTypeBuilder<Suggestion> builder) 
    {
        builder.HasKey(x => x.Id);
    }
}

public class PlayerConfiguration : IEntityTypeConfiguration<Player>
{
    public void Configure(EntityTypeBuilder<Player> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.NickName)
            .IsRequired();
        builder.Property(p => p.Elo)
            .HasDefaultValue(1500)
            .IsRequired();
        builder.HasOne(p => p.User)
            .WithOne(u => u.Player)
            .HasForeignKey<Player>(x => x.UserId);
        builder.HasMany(p => p.EloHistories)
            .WithOne(eh => eh.Player)
            .HasForeignKey(eh => eh.PlayerId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(p => p.CreatedMatches)
            .WithOne(m => m.Creator)
            .HasForeignKey(m => m.CreatorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class MatchConfiguration : IEntityTypeConfiguration<Match>
{
    public void Configure(EntityTypeBuilder<Match> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.CreationDate)
            .IsRequired();
        builder.HasOne<Player>(m => m.Creator)
            .WithMany(p => p.CreatedMatches) // Updated this line to reference the collection of created matches in the Player entity
            .HasForeignKey(m => m.CreatorId)
            .OnDelete(DeleteBehavior.Restrict); 
        builder.HasMany(m => m.EloHistories) // Added this line to define the relationship with EloHistory
            .WithOne(eh => eh.Match)
            .HasForeignKey(eh => eh.MatchId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class MatchPlayerConfiguration : IEntityTypeConfiguration<MatchPlayer>
{
    public void Configure(EntityTypeBuilder<MatchPlayer> builder)
    {
        builder.HasKey(mp => mp.Id);
        builder.HasOne(mp => mp.Match)
            .WithMany(m => m.MatchPlayers)
            .HasForeignKey(mp => mp.MatchId);
        builder.HasOne(mp => mp.Player)
            .WithMany()
            .HasForeignKey(mp => mp.PlayerId);
        builder.Property(mp => mp.Team)
            .HasConversion<string>()
            .IsRequired();
    }
}

public class SetConfiguration : IEntityTypeConfiguration<Set>
{
    public void Configure(EntityTypeBuilder<Set> builder)
    {
        builder.HasKey(s => s.Id);
        builder.HasOne(s => s.Match)
            .WithMany(m => m.Sets)
            .HasForeignKey(s => s.MatchId);
        builder.Property(s => s.SetNumber)
            .IsRequired();
        builder.Property(s => s.Team1Score)
            .IsRequired();
        builder.Property(s => s.Team2Score)
            .IsRequired();
    }
}

public class EloHistoryConfiguration : IEntityTypeConfiguration<EloHistory>
{
    public void Configure(EntityTypeBuilder<EloHistory> builder)
    {
        builder.HasKey(eh => eh.Id);
        builder.Property(eh => eh.PreviousElo)
            .IsRequired();
        builder.Property(eh => eh.CurrentElo)
            .IsRequired();
        builder.HasOne(eh => eh.Player)
            .WithMany(p => p.EloHistories)
            .HasForeignKey(eh => eh.PlayerId);
        builder.HasOne(eh => eh.Match)
            .WithMany(m => m.EloHistories)
            .HasForeignKey(eh => eh.MatchId);
    }
}


