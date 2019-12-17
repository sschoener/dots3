using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Match3Game
{
    [UpdateInGroup(typeof(Match3SimulationGroup))]
    [UpdateAfter(typeof(BoardCacheSystem))]
    [UpdateAfter(typeof(InitialGemSpawningSystem))]
    public class GemSpawningSystem : ComponentSystem
    {
        private BoardCacheSystem _boardCache;
        private EntityQuery _gemPrefabs;

        private Random _gemRng;

        protected override void OnCreate()
        {
            _boardCache = World.GetExistingSystem<BoardCacheSystem>();
            RequireSingletonForUpdate<GameComponent>();

            _gemRng = new Random();
            _gemRng.InitState();

            _gemPrefabs = Entities.With(EntityQueryOptions.IncludePrefab).WithAll<GemComponent, Prefab>().ToEntityQuery();
        }

        protected override void OnUpdate()
        {
            if (GetSingleton<BoardStateComponent>().State != BoardState.Animating)
                return;
            
            int w = _boardCache.BoardWidth;
            int h = _boardCache.BoardHeight;

            var board = GetSingleton<BoardComponent>();

            using (var prefabEntities = _gemPrefabs.ToEntityArray(Allocator.TempJob))
            {
                for (int x = 0; x < w; x++)
                {
                    int numToSpawn = 0;
                    for (int y = 0; y < h; y++)
                    {
                        if (_boardCache.GetGem(x, y) == GemType.None)
                        {
                            numToSpawn++;
                        }
                    }
                    if (numToSpawn == 0)
                        continue;

                    float maxHeight = math.max(_boardCache.GetMaxHeight(x), board.MaxGridPosition.y + board.CellSize / 2);
                    for (int i = 0; i < numToSpawn; i++)
                    {
                        int y = h - numToSpawn + i;
                        var entity = EntityManager.Instantiate(prefabEntities[_gemRng.NextInt(prefabEntities.Length)]);
                        EntityManager.AddComponent<BoardAnimationComponent>(entity);
                        EntityManager.AddComponent<GemFallAnimationComponent>(entity);
                        EntityManager.SetComponentData(entity, new Translation
                        {
                            Value = new float3(board.GridCellCenter(x, y).x, maxHeight, 0),
                        });
                        EntityManager.AddComponentData(entity, new BoardPositionComponent
                        {
                            BoardPosition = new int2(x, y),
                        });

                        maxHeight += board.CellSize;
                    }
                }
            }
        }
    }
}
