using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using Terrarium.Server.Models;

namespace Terrarium.Server.Data.Mappings
{
    public class UserRegisterMap : EntityTypeConfiguration<UserRegister>
    {
        public UserRegisterMap()
        {
            ToTable("UserRegister");
            Property(x => x.Id).IsRequired().HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(x => x.IPAddress).IsRequired().HasMaxLength(50);
            Property(x => x.Email).HasMaxLength(255);
        }
    }
}