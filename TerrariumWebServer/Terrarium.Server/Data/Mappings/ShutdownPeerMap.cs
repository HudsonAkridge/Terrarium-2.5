using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using Terrarium.Server.Models;

namespace Terrarium.Server.Data.Mappings
{
    public class ShutdownPeerMap : EntityTypeConfiguration<ShutdownPeer>
    {
        public ShutdownPeerMap()
        {
            ToTable("ShutdownPeers");
            Property(x => x.Id).IsRequired().HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(x => x.Channel).IsRequired().HasMaxLength(255);
            Property(x => x.IPAddress).IsRequired().HasMaxLength(50);
            Property(x => x.Version).IsRequired().HasMaxLength(255);
            Property(x => x.FirstContact).IsRequired();
            Property(x => x.LastContact).IsRequired();
            Property(x => x.UnRegister).IsRequired();
        }
    }
}