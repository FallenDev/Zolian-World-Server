using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Networking.Transport;

using UnityEngine;

namespace Assets.Zolian.Systems
{
    /// <summary>
    /// Polls the network driver, accepts new connections, and processes network events.
    /// </summary>
    [BurstCompile]
    public partial struct ServerUpdateConnectionsSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var serverNet = SystemAPI.GetSingletonRW<Transport.WorldServerNetwork>();

            serverNet.ValueRW.Driver.ScheduleUpdate().Complete();

            // Accept new connections
            NetworkConnection conn;
            while ((conn = serverNet.ValueRW.Driver.Accept()) != default)
            {
                serverNet.ValueRW.Connections.Add(conn);
                Debug.Log("[WorldServer] Accepted new connection.");
            }

            // Poll connections for events
            for (var i = 0; i < serverNet.ValueRW.Connections.Length; ++i)
            {
                if (!serverNet.ValueRW.Connections[i].IsCreated)
                {
                    serverNet.ValueRW.Connections.RemoveAtSwapBack(i);
                    --i;
                    continue;
                }

                DataStreamReader stream;
                NetworkEvent.Type evt;
                while ((evt = serverNet.ValueRW.Driver.PopEventForConnection(serverNet.ValueRW.Connections[i], out _)) != NetworkEvent.Type.Empty)
                {
                    if (evt == NetworkEvent.Type.Data)
                    {
                        // TODO: Parse stream data, enqueue for ECS processing
                    }
                    else if (evt == NetworkEvent.Type.Disconnect)
                    {
                        Debug.Log("[WorldServer] Client disconnected.");
                        serverNet.ValueRW.Connections[i] = default;
                    }
                }
            }
        }
    }
}
