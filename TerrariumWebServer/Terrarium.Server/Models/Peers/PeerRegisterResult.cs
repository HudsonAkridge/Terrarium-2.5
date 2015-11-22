namespace Terrarium.Server.Models.Peers
{
    public class PeerRegisterResult
    {
        public object Peers { get; set; }
        public RegisterPeerResult Result { get; set; }
        public int Count { get; set; }
    }
}