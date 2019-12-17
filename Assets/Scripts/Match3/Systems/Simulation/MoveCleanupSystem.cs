using Unity.Entities;

namespace Match3Game
{
    [UpdateInGroup(typeof(Match3SimulationGroup))]
    public class MoveCleanupSystem : ComponentSystem
    {
        private EntityQuery _moveQuery;
        private BeginInitializationEntityCommandBufferSystem _initCommands;
        protected override void OnCreate()
        {
            _moveQuery = Entities.WithAll<MoveComponent>().ToEntityQuery();
            _initCommands = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            _initCommands.CreateCommandBuffer().DestroyEntity(_moveQuery);
        }
    }
}