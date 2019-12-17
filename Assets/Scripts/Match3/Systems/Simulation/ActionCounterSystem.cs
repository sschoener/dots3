using Unity.Entities;

namespace Match3Game
{
    [UpdateInGroup(typeof(Match3SimulationGroup))]
    [UpdateAfter(typeof(PlayerInputSystem))]
    public class ActionCounterSystem : ComponentSystem
    {
        private EntityQuery _actionQuery;
        private EventSystem _events;
        private EntityCommandBufferSystem _initCommands;
        protected override void OnCreate()
        {
            _actionQuery = Entities.WithAll<MoveComponent>().ToEntityQuery();
            _events = World.GetOrCreateSystem<EventSystem>();
            _initCommands = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();

            RequireForUpdate(_actionQuery);
        }

        protected override void OnUpdate()
        {
            Entities.With(_actionQuery).ForEach((Entity e) => {
                _events.PostOneShot(new ActionEvent());
            });
            _initCommands.CreateCommandBuffer().DestroyEntity(_actionQuery);
        }
    }
}
