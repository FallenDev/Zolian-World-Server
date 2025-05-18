using Unity.Networking.Transport;
using Unity.Collections;
using Unity.Entities;

namespace Assets.Zolian.Transport
{
    /// <summary>
    /// Holds the server NetworkDriver and active connections in ECS singleton.
    /// </summary>
    public struct WorldServerNetwork : IComponentData
    {
        public NetworkDriver Driver;
        public NativeList<NetworkConnection> Connections;
    }
}