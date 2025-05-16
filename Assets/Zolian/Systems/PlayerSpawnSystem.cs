using Assets.Zolian.Networking;
using Assets.Zolian.Transport;

using Unity.Entities;

using UnityEngine;

namespace Assets.Zolian.Systems
{
    /// <summary>
    /// ECS system that waits for player connection requests from the jobified server
    /// and processes them once the job has completed.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class PlayerSpawnSystem : SystemBase
    {
        protected override void OnCreate()
        {
            // Log to confirm ECS system initialized
            Debug.Log("[PlayerSpawnSystem] Initialized.");
        }

        protected override void OnUpdate()
        {
            // Wait until the server job finishes writing to the NativeQueue
            if (!ServerWorldContext.ConnectionJobHandle.IsCompleted)
                return;

            // Ensure job is fully completed before accessing the queue
            ServerWorldContext.ConnectionJobHandle.Complete();

            // Process all new join requests
            while (ServerWorldContext.JoinRequests.TryDequeue(out var request))
            {
                Debug.Log($"[PlayerSpawnSystem] Connection received from {request.Endpoint} at {request.Timestamp}");

                // TODO: In future, deserialize incoming data (e.g., ConfirmConnectionMessage)
                // and spawn a player entity here.
            }
        }
    }
}