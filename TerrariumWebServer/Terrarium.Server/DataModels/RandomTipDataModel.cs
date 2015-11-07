using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using Terrarium.Server.Models;

namespace Terrarium.Server.DataModels
{
    public class RandomTipDataModel : EntityTypeConfiguration<RandomTip>
    {
        public RandomTipDataModel()
        {
            ToTable("RandomTips");
            Property(x => x.Id).IsRequired().HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(x => x.Tip).IsRequired().HasMaxLength(512);
        }
    }
}