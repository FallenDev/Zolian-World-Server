using Unity.Networking.Transport;

namespace Assets.Zolian.Networking
{
    public struct PlayerJoinRequest
    {
        public NetworkConnection Connection;
        public NetworkEndpoint Endpoint;
        public float Timestamp;
    }
}