using Unity.Collections;
using Unity.Entities;

namespace Match3Game
{
    [UpdateInGroup(typeof(Match3SimulationGroup))]
    [UpdateAfter(typeof(PlayerInputSystem))]
    public class SwapMoveSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            var query = Entities.WithAll<SwapMoveComponent>().ToEntityQuery();
            using (var moves = query.ToComponentDataArray<SwapMoveComponent>(Allocator.TempJob))
            {
                foreach (var move in moves)
                {
                    Entities.ForEach((Entity e, ref BoardPositionComponent gem) =>
                    {
                        if (gem.BoardPosition.Equals(move.From))
                            gem.BoardPosition = move.To;
                        else if (gem.BoardPosition.Equals(move.To))
                            gem.BoardPosition = move.From;
                        else
                            return;

                        PostUpdateCommands.AddComponent(e, typeof(GemFallAnimationComponent));
                        PostUpdateCommands.AddComponent(e, typeof(BoardAnimationComponent));
                    });
                }
            }
        }
    }
}
