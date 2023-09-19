using EventsManager.Server.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventsManager.Server.Data.TypeConfigurations;

public class PlayerConfiguration : IEntityTypeConfiguration<Player>
{
    public void Configure(EntityTypeBuilder<Player> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.NickName)
            .IsRequired();
        builder.Property(p => p.Elo)
            .IsRequired();
        builder.Property(p => p.TrustFactor)
            .HasDefaultValue(25);
        builder.HasOne(p => p.User)
            .WithOne(u => u.Player)
            .HasForeignKey<Player>(x => x.UserId);
        builder.HasMany(p => p.EloHistories)
            .WithOne(eh => eh.Player)
            .HasForeignKey(eh => eh.PlayerId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(p => p.Notifications)
            .WithOne(eh => eh.Player)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(p => p.CreatedMatches)
            .WithOne(m => m.Creator)
            .HasForeignKey(m => m.CreatorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}