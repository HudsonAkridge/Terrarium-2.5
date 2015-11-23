using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using Terrarium.Server.Models.Peers;

namespace Terrarium.Server.Data.Mappings
{
    public class PeerMap : EntityTypeConfiguration<Peer>
    {
        public PeerMap()
        {
            ToTable("Peers");
            Property(x => x.Id).IsRequired().HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(x => x.Channel).IsRequired().HasMaxLength(32);
            Property(x => x.IPAddress).IsRequired().HasMaxLength(16);
            Property(x => x.Lease).IsRequired();
            Property(x => x.Version).IsRequired().HasMaxLength(16);
            Property(x => x.FirstContact).IsRequired();
        }
    }
}