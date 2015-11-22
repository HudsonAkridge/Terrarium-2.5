using System;
using Terrarium.Server.Models.Peers;

namespace Terrarium.Server.Models
{
    public class ShutdownPeer : BasePeer
    {
        public DateTime LastContact { get; set; }
        public bool UnRegister { get; set; }
    }
}