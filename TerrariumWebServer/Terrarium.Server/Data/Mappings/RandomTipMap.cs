using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using Terrarium.Server.Models;

namespace Terrarium.Server.Data.Mappings
{
    public class RandomTipMap : EntityTypeConfiguration<RandomTip>
    {
        public RandomTipMap()
        {
            ToTable("RandomTips");
            Property(x => x.Id).IsRequired().HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(x => x.Tip).IsRequired().HasMaxLength(512);
        }
    }
}