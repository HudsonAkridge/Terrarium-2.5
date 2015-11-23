using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using Terrarium.Server.Models;

namespace Terrarium.Server.Data.Mappings
{
    public class WatsonMap : EntityTypeConfiguration<Watson>
    {
        public WatsonMap()
        {
            ToTable("Watson");
            Property(x => x.Id).IsRequired().HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(x => x.LogType).HasMaxLength(50);
            Property(x => x.MachineName).HasMaxLength(255);
            Property(x => x.OSVersion).HasMaxLength(50);
            Property(x => x.GameVersion).HasMaxLength(50);
            Property(x => x.CLRVersion).HasMaxLength(50);
            Property(x => x.UserEmail).HasMaxLength(255);
        }
    }
}