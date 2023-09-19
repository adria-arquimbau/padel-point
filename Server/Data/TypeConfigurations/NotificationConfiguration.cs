using EventsManager.Server.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventsManager.Server.Data.TypeConfigurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.CreationDate)
            .IsRequired();
        builder.HasOne<Player>(m => m.Player)
            .WithMany(p => p.Notifications);
    }
}