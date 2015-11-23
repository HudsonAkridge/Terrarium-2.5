using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using Terrarium.Server.Models;

namespace Terrarium.Server.Data.Mappings
{
    public class UsageSummaryMap : EntityTypeConfiguration<UsageSummary>
    {
        public UsageSummaryMap()
        {
            ToTable("UsageSummary");
            Property(x => x.Id).IsRequired().HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(x => x.Peers).IsRequired();
            Property(x => x.SummaryDateTime).IsRequired();
        }
    }
}