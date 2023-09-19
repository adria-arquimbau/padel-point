using EventsManager.Server.Models;
using EventsManager.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventsManager.Server.Data.TypeConfigurations;

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
        builder.Property(mp => mp.Winner)
            .HasConversion<string>();
        builder.Property(mp => mp.Location)
            .HasDefaultValue(MatchLocation.None)
            .HasConversion<string>();
        builder.HasMany(m => m.Promotions)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.Tournament)
            .WithMany(x => x.RoundRobinMatches)
            .HasForeignKey(x => x.TournamentId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}