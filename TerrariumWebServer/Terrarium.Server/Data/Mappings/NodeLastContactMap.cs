using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using Terrarium.Server.Models;

namespace Terrarium.Server.Data.Mappings
{
    public class NodeLastContactMap : EntityTypeConfiguration<NodeLastContact>
    {
        public NodeLastContactMap()
        {
            ToTable("NodeLastContact");
            Property(x => x.Id).IsRequired().HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(x => x.GUID).IsRequired();
            Property(x => x.LastTick).IsRequired();
            Property(x => x.LastContact).IsRequired();
        }
    }
}