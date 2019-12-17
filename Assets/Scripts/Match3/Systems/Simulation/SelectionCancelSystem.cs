using Unity.Entities;

namespace Match3Game
{
    [UpdateInGroup(typeof(Match3SimulationGroup))]
    public class SelectionCancelSystem : ComponentSystem
    {
        private EntityQuery _selectionQuery;
        private EntityQuery _turnEndQuery;
        private BeginInitializationEntityCommandBufferSystem _initCommands;
        protected override void OnCreate()
        {
            _selectionQuery = Entities.WithAll<BoardSelectionComponent>().ToEntityQuery();
            _turnEndQuery = Entities.WithAll<TurnEndEvent>().ToEntityQuery();
            _initCommands = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            if (_turnEndQuery.CalculateEntityCount() > 0)
            {
                _initCommands.CreateCommandBuffer().DestroyEntity(_selectionQuery);
            }
        }
    }
}
