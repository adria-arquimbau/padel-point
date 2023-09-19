using EventsManager.Server.Models;
using EventsManager.Shared.Enums;
using EventsManager.Shared.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventsManager.Server.Data.TypeConfigurations;

public class TournamentConfiguration : IEntityTypeConfiguration<Tournament>
{
    public void Configure(EntityTypeBuilder<Tournament> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(x => x.RoundRobinPhaseGroups)
            .HasDefaultValue(1);
        builder.Property(mp => mp.Location)
            .HasDefaultValue(MatchLocation.None)
            .HasConversion<string>();
        builder.Property(mp => mp.RoundRobinType)
            .HasDefaultValue(RoundRobinType.Random)
            .HasConversion<string>();
        builder.Property(mp => mp.CompetitionStyle)
            .HasDefaultValue(CompetitionStyle.RoundRobinPhaseOnly)
            .HasConversion<string>();
    }
}