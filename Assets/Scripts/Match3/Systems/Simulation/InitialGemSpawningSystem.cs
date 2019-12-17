using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Match3Game
{
    [UpdateInGroup(typeof(Match3SimulationGroup))]
    [UpdateBefore(typeof(BoardCacheSystem))]
    public class InitialGemSpawningSystem : ComponentSystem
    {
        private Random _rng;
        private EntityQuery _gemPrefabs;
        private EventSystem _events;

        protected override void OnCreate()
        {
            RequireSingletonForUpdate<GameComponent>();
            _rng = new Random();
            _rng.InitState(math.asuint(UnityEngine.Time.realtimeSinceStartup));

            _events = World.GetOrCreateSystem<EventSystem>();

            _gemPrefabs = Entities.With(EntityQueryOptions.IncludePrefab).WithAll<GemComponent, Prefab>().ToEntityQuery();
        }

        protected override void OnUpdate()
        {
            if (GetSingleton<BoardStateComponent>().State != BoardState.Initializing)
                return;
            var board = GetSingleton<BoardComponent>();
            int boardWidth = board.Size.x;
            int boardHeight = board.Size.y;

            var prefabEntities = _gemPrefabs.ToEntityArray(Allocator.TempJob);
            var boardGems = new NativeArray<int>(boardWidth * boardHeight, Allocator.Temp);
            int numGems = prefabEntities.Length;
            for (int i = 0; i < boardGems.Length; i++)
                boardGems[i] = _rng.NextInt(numGems);

            using (var matches = new NativeArray<MatchInfo>(boardWidth * boardHeight, Allocator.Temp))
            {
                // Remove all intial matches
                bool hasMatches;
                do
                {
                    hasMatches = false;
                    MatchFinder.FindMatches(boardWidth, boardGems, matches);

                    for (int i = 0; i < matches.Length; i++)
                    {
                        if (matches[i].HorizontalLength >= 3 || matches[i].VerticalLength >= 3)
                        {
                            boardGems[i] = _rng.NextInt(numGems);
                            hasMatches = true;
                        }
                    }
                } while (hasMatches);
            }

            {
                // spawn prefabs
                int i = 0;
                for (int x = 0; x < boardWidth; x++)
                {
                    float height = board.MaxGridPosition.y;
                    for (int y = 0; y < boardHeight; y++, i++)
                    {
                        var e = EntityManager.Instantiate(prefabEntities[boardGems[i]]);
                        EntityManager.AddComponent<GemFallAnimationComponent>(e);
                        EntityManager.AddComponent<BoardAnimationComponent>(e);
                        EntityManager.SetComponentData(e, new Translation
                        {
                            Value = new float3(board.GridCellCenter(x, y).x, height, 0),
                        });
                        EntityManager.AddComponentData(e, new BoardPositionComponent
                        {
                            BoardPosition = new int2(x, y)
                        });
                        height += board.CellSize;
                    }
                }
            }

            _events.PostOneShot(new BoardStateTransitionEvent{
                NewState = BoardState.Animating
            });
            prefabEntities.Dispose();
            boardGems.Dispose();
        }
    }
}
