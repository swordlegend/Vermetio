using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Networking.Transport;
using Unity.Transforms;
using UnityEngine;

[UpdateInWorld(UpdateInWorld.TargetWorld.Default)]
[AlwaysSynchronizeSystem]
public class ConnectionSystem : SystemBase
{
    // Singleton component to trigger connections once from a control system
    struct InitGameComponent : IComponentData
    {
    }
    
    public struct GoInGameRequest : IRpcCommand
    {
    }

    protected override void OnCreate()
    {
        RequireSingletonForUpdate<InitGameComponent>();
        // Create singleton, require singleton for update so system runs once
        EntityManager.CreateEntity(typeof(InitGameComponent));
    }

    protected override void OnUpdate()
    {
        // Destroy singleton to prevent system from running again
        EntityManager.DestroyEntity(GetSingletonEntity<InitGameComponent>());
        
        foreach (var world in World.All)
        {
            // Debug.Log($"World: {world.Name}");
            var network = world.GetExistingSystem<NetworkStreamReceiveSystem>();
            if (world.GetExistingSystem<ClientSimulationSystemGroup>() != null)
            {
                // var tickRate = world.EntityManager.CreateEntity();
                // world.EntityManager.AddComponentData(tickRate, new ClientServerTickRate
                // {
                //     SimulationTickRate = 30
                // });
                
                // Client worlds automatically connect to localhost
                NetworkEndPoint ep = NetworkEndPoint.LoopbackIpv4;
                ep.Port = 7979;
                Debug.Log("Connecting...");
                network.Connect(ep);
            }
            #if UNITY_EDITOR || UNITY_SERVER
            else if (world.GetExistingSystem<ServerSimulationSystemGroup>() != null)
            {
                // var tickRate = world.EntityManager.CreateEntity();
                // world.EntityManager.AddComponentData(tickRate, new ClientServerTickRate
                // {
                //     SimulationTickRate = 30
                // });

                // Server world automatically listens for connections from any host
                NetworkEndPoint ep = NetworkEndPoint.AnyIpv4;
                ep.Port = 7979;
                Debug.Log("Listening...");
                network.Listen(ep);
            }
            #endif
        }
    }
}