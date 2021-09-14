using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

namespace Vermetio.Server
{
    [UpdateInGroup(typeof(ServerSimulationSystemGroup))]
    [UpdateAfter(typeof(BoatHitSystem))]
    public class DieSystem : SystemBase
    {
        private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            _endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var endFrameEcb = _endSimulationEcbSystem.CreateCommandBuffer();

            Entities.ForEach((Entity entity, in HealthComponent health) =>
                {
                    endFrameEcb.DestroyEntity(entity);
                }).Schedule();

            _endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
