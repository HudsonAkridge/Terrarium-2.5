using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using Terrarium.Server.Models;

namespace Terrarium.Server.Data.Mappings
{
    public class VersionedSettingMap : EntityTypeConfiguration<VersionedSetting>
    {
        public VersionedSettingMap()
        {
            ToTable("VersionedSettings");
            Property(x => x.Id).IsRequired().HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(x => x.Version).IsRequired().HasMaxLength(255);
            Property(x => x.Disabled).IsRequired();
            Property(x => x.Message).HasMaxLength(255);
        }
    }
}