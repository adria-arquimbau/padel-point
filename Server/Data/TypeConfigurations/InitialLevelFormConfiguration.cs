using EventsManager.Server.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventsManager.Server.Data.TypeConfigurations;

public class InitialLevelFormConfiguration : IEntityTypeConfiguration<InitialLevelForm>
{
    public void Configure(EntityTypeBuilder<InitialLevelForm> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(mp => mp.OtherRacketSportsLevel)
            .HasConversion<string>();
        builder.Property(mp => mp.SelfAssessedPadelSkillLevel)
            .HasConversion<string>();
        builder.HasOne(mp => mp.Player)
            .WithOne(p => p.InitialLevelForm)
            .HasForeignKey<InitialLevelForm>(mp => mp.PlayerId);
    }
}