using EventsManager.Server.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventsManager.Server.Data.TypeConfigurations;

public class AnnouncementsConfiguration : IEntityTypeConfiguration<Announcements>
{
    public void Configure(EntityTypeBuilder<Announcements> builder)
    {
        builder.HasKey(mp => mp.Id);
    }
}