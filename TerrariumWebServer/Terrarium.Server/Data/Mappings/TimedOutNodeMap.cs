using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using Terrarium.Server.Models;

namespace Terrarium.Server.Data.Mappings
{
    public class TimedOutNodeMap : EntityTypeConfiguration<TimedOutNode>
    {
        public TimedOutNodeMap()
        {
            ToTable("TimedOutNodes");
            Property(x => x.Id).IsRequired().HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(x => x.GUID).IsRequired();
            Property(x => x.TimeoutDate).IsRequired();
        }
    }
}