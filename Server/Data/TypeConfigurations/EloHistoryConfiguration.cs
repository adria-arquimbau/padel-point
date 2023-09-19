using EventsManager.Server.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventsManager.Server.Data.TypeConfigurations;

public class EloHistoryConfiguration : IEntityTypeConfiguration<EloHistory>
{
    public void Configure(EntityTypeBuilder<EloHistory> builder)
    {
        builder.HasKey(eh => eh.Id);
        builder.Property(eh => eh.OldElo)
            .IsRequired();
        builder.Property(eh => eh.NewElo)
            .IsRequired();
        builder.HasOne(eh => eh.Player)
            .WithMany(p => p.EloHistories)
            .HasForeignKey(eh => eh.PlayerId);
        builder.HasOne(eh => eh.Match)
            .WithMany(m => m.EloHistories)
            .HasForeignKey(eh => eh.MatchId);
        builder.Property(mp => mp.ChangeReason)
            .HasConversion<string>()
            .IsRequired();
    }
}