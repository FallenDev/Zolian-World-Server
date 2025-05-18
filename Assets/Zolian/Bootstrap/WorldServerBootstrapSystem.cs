using Unity.Entities;
using Unity.Networking.Transport;
using Unity.Collections;

namespace Assets.Zolian.Bootstrap
{
    /// <summary>
    /// Initializes the world server network driver and connection list at startup.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct WorldServerBootstrapSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            var driver = NetworkDriver.Create();
            var endpoint = NetworkEndpoint.AnyIpv4.WithPort(7777);
            driver.Bind(endpoint);
            driver.Listen();

            state.EntityManager.CreateSingleton(new Transport.WorldServerNetwork
            {
                Driver = driver,
                Connections = new NativeList<NetworkConnection>(16, Allocator.Persistent)
            });
        }

        public void OnDestroy(ref SystemState state)
        {
            var serverNet = SystemAPI.GetSingleton<Transport.WorldServerNetwork>();
            if (serverNet.Driver.IsCreated) serverNet.Driver.Dispose();
            if (serverNet.Connections.IsCreated) serverNet.Connections.Dispose();
        }
    }
}