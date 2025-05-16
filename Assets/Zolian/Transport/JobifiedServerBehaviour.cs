using Assets.Zolian.Networking;
using Assets.Zolian.Systems;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Networking.Transport;
using Unity.Entities;

namespace Assets.Zolian.Transport
{
    /// <summary>
    /// Manages socket communication with Unity Transport + Jobs.
    /// Listens for new connections and dispatches player join requests into ECS.
    /// </summary>
    public class JobifiedServerBehaviour : MonoBehaviour
    {
        private NetworkDriver m_Driver;
        private NativeList<NetworkConnection> m_Connections;
        private JobHandle m_ServerJobHandle;
        private EntityManager _entityManager;

        void Start()
        {
            // Initialize transport driver and connection list
            m_Driver = NetworkDriver.Create();
            m_Connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);
            ServerWorldContext.JoinRequests = new NativeQueue<PlayerJoinRequest>(Allocator.Persistent);
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            // Bind to port
            var endpoint = NetworkEndpoint.AnyIpv4.WithPort(7777);
            if (m_Driver.Bind(endpoint) != 0)
            {
                Debug.LogError("Failed to bind to port 7777.");
                return;
            }

            m_Driver.Listen();
            Debug.Log($"[Server] Binding to endpoint: {endpoint.Address}, Port: {endpoint.Port}, Family: {endpoint.Family}");

            // Inject queue into ECS system via reflection
            var spawnSystem = World.DefaultGameObjectInjectionWorld
                .GetExistingSystemManaged<PlayerSpawnSystem>();

            typeof(PlayerSpawnSystem)
                .GetField("_joinRequests",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(spawnSystem, ServerWorldContext.JoinRequests);
        }

        void OnDestroy()
        {
            // Ensure job is complete before disposing resources
            m_ServerJobHandle.Complete();

            m_Driver.Dispose();
            m_Connections.Dispose();
            ServerWorldContext.JoinRequests.Dispose();
        }

        void Update()
        {
            // Wait for previous job
            m_ServerJobHandle.Complete();

            // Schedule the connection handling job
            var connectionJob = new ServerUpdateConnectionsJob
            {
                Driver = m_Driver,
                Connections = m_Connections,
                JoinRequests = ServerWorldContext.JoinRequests.AsParallelWriter()
            };

            m_ServerJobHandle = connectionJob.Schedule();

            // Save job handle for ECS world to complete before reading the queue
            ServerWorldContext.ConnectionJobHandle = m_ServerJobHandle;
        }

        /// <summary>
        /// Accepts new client connections and queues join requests.
        /// </summary>
        public struct ServerUpdateConnectionsJob : IJob
        {
            public NetworkDriver Driver;
            public NativeList<NetworkConnection> Connections;
            public NativeQueue<PlayerJoinRequest>.ParallelWriter JoinRequests;

            public void Execute()
            {
                Debug.Log("[ServerUpdateConnectionsJob] Execute started.");

                int removed = 0;
                for (int i = 0; i < Connections.Length; i++)
                {
                    if (!Connections[i].IsCreated)
                    {
                        Connections.RemoveAtSwapBack(i);
                        i--;
                        removed++;
                    }
                }

                if (removed > 0)
                    Debug.Log($"[ServerUpdateConnectionsJob] Removed {removed} invalid connections.");

                NetworkConnection c;
                int accepted = 0;
                while ((c = Driver.Accept()) != default)
                {
                    Connections.Add(c);
                    accepted++;

                    JoinRequests.Enqueue(new PlayerJoinRequest
                    {
                        Connection = c,
                        Endpoint = Driver.GetRemoteEndpoint(c),
                        Timestamp = UnityEngine.Time.time
                    });

                    Debug.Log($"[Server] Accepted connection from: {Driver.GetRemoteEndpoint(c)}");
                }

                if (accepted == 0)
                    Debug.Log("[ServerUpdateConnectionsJob] No new connections accepted.");
            }

        }
    }
}
