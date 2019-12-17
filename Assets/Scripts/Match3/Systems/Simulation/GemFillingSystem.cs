using Unity.Collections;
using Unity.Entities;

namespace Match3Game
{
    [UpdateInGroup(typeof(Match3SimulationGroup))]
    [UpdateAfter(typeof(BoardCacheSystem))]
    public class GemFillingSystem : ComponentSystem
    {
        private BoardCacheSystem _cache;
        protected override void OnCreate()
        {
            _cache = World.GetOrCreateSystem<BoardCacheSystem>();
            RequireSingletonForUpdate<GameComponent>();
        }

        protected override void OnUpdate()
        {
            int w = _cache.BoardWidth;
            int h = _cache.BoardHeight;
            var fallDown = new NativeArray<int>(w * h, Allocator.Temp);
            for (int x = 0; x < w; x++) {
                int height = 0;
                for (int y = 0; y < h; y++) {
                    fallDown[Index(x, y)] = height;
                    height += (_cache.GetGem(x, y) == GemType.None) ? 1 : 0;
                }
            }

            Entities.ForEach((Entity e, ref BoardPositionComponent gem) => {
                var pos = gem.BoardPosition;
                int amount = fallDown[Index(pos.x, pos.y)];
                if (amount > 0) {
                    pos.y -= amount;
                    gem.BoardPosition = pos;

                    PostUpdateCommands.AddComponent(e, typeof(GemFallAnimationComponent));
                    PostUpdateCommands.AddComponent(e, typeof(BoardAnimationComponent));
                }
            });

            fallDown.Dispose();

            int Index(int x, int y) => x * _cache.BoardHeight + y;
        }
    }
}
