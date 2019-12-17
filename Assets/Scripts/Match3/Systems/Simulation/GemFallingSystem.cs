using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Match3Game
{
    [UpdateInGroup(typeof(Match3SimulationGroup))]
    [UpdateAfter(typeof(GemFillingSystem))]
    [UpdateAfter(typeof(GemSpawningSystem))]
    public class GemFallingSystem : ComponentSystem
    {
        private EntityQuery _gemQuery;
        protected override void OnCreate()
        {
            _gemQuery = Entities.WithAll<GemFallAnimationComponent, BoardAnimationComponent>().ToEntityQuery();
            RequireForUpdate(_gemQuery);
            RequireSingletonForUpdate<GameComponent>();
        }

        protected override void OnUpdate()
        {
            var board = GetSingleton<BoardComponent>();

            float deltaTime = UnityEngine.Time.deltaTime;
            const float animSpeed = 7;
            Entities.With(_gemQuery).ForEach((Entity e, ref BoardPositionComponent gem, ref Translation pos) => {
                var cell = board.GridCellCenter(gem.BoardPosition);
                float2 delta = cell - pos.Value.xy;
                float len = math.length(delta);
                float2 dir = delta / len;
                float dist = math.min(len, animSpeed * deltaTime);
                pos.Value += new float3(dist * dir, 0);
                if (dist >= len - 0.001f) {
                    pos.Value = new float3(cell, 0);
                    PostUpdateCommands.RemoveComponent(e, typeof(BoardAnimationComponent));
                    PostUpdateCommands.RemoveComponent(e, typeof(GemFallAnimationComponent));
                }
            });
        }
    }
}
