using Assets.Zolian.Networking;

using Unity.Collections;
using Unity.Jobs;

namespace Assets.Zolian.Transport
{
    /// <summary>
    /// Provides shared access to server-wide context such as join queues and job handles.
    /// Used to safely bridge data between Jobified socket handling and ECS systems.
    /// </summary>
    public static class ServerWorldContext
    {
        /// <summary>
        /// A persistent native queue for join requests, written to by connection jobs and read by ECS systems.
        /// </summary>
        public static NativeQueue<PlayerJoinRequest> JoinRequests;

        /// <summary>
        /// The active job handle representing connection processing.
        /// ECS systems must call Complete() on this before reading from JoinRequests.
        /// </summary>
        public static JobHandle ConnectionJobHandle;
    }
}