using Unity.Collections;
using Unity.Entities;

namespace Match3Game
{
    [UpdateInGroup(typeof(Match3SimulationGroup))]
    [UpdateAfter(typeof(BoardCacheSystem))]
    public class MatchFindingSystem : ComponentSystem
    {
        private BoardCacheSystem _boardCache;
        private EventSystem _events;

        protected override void OnCreate()
        {
            _boardCache = World.GetExistingSystem<BoardCacheSystem>();
            _events = World.GetOrCreateSystem<EventSystem>();

            RequireSingletonForUpdate<GameComponent>();
        }

        protected override void OnUpdate()
        {
            if (GetSingleton<BoardStateComponent>().State != BoardState.LookingForMatch)
                return;

            var board = GetSingleton<BoardComponent>();
            int w = board.Size.x;
            int h = board.Size.y;

            int numMatches = 0;
            using (var matches = new NativeArray<MatchInfo>(w * h, Allocator.Temp))
            {
                MatchFinder.FindMatches(w, _boardCache.Board, matches);

                var eventQueue = _events.CreateEventQueue<MatchEvent>();
                for (int i = 0; i < matches.Length; i++)
                {
                    var match = matches[i];
                    bool isHMatch = match.HorizontalLength >= 3 && match.MatchLeft == 0;
                    bool isVMatch = match.VerticalLength >= 3 && match.MatchDown == 0;
                    if (!isHMatch && !isVMatch)
                    {
                        continue;
                    }
                    numMatches += 1;
                    int x = i / h;
                    int y = i % h;
                    byte length = (byte)(1 + (isHMatch ? match.MatchRight : match.MatchUp));
                    eventQueue.Enqueue(new MatchEvent
                    {
                        Position = new Unity.Mathematics.int2(x, y),
                        Gem = _boardCache.GetGem(x, y),
                        MatchLength = length
                    });
                }

                Entities.ForEach((Entity e, ref GemComponent gem, ref BoardPositionComponent boardPos) =>
                {
                    int index = boardPos.BoardPosition.x * h + boardPos.BoardPosition.y;
                    if (matches[index].HorizontalLength >= 3 || matches[index].VerticalLength >= 3)
                    {
                        PostUpdateCommands.AddComponent(e, typeof(MatchedGemComponent));
                        PostUpdateCommands.AddComponent(e, typeof(BoardAnimationComponent));
                        PostUpdateCommands.AddComponent(e, typeof(GemMatchAnimationComponent));
                    }
                });
            }

            if (numMatches > 0)
            {
                _events.PostOneShot(new BoardStateTransitionEvent
                {
                    NewState = BoardState.Animating
                });
                return;
            }

            // check whether there are any moves
            using (var matches = new NativeArray<SwapMatches>(w * h, Allocator.Temp))
            {
                MatchFinder.ScoreSwapMoves(w, _boardCache.Board, matches);
                bool anyMatch = false;
                for (int i = 0; i < matches.Length; i++)
                {
                    if (matches[i].SwapRight1.Match.MaxLength > 2 ||
                        matches[i].SwapRight2.Match.MaxLength > 2 ||
                        matches[i].SwapUp1.Match.MaxLength > 2 ||
                        matches[i].SwapUp2.Match.MaxLength > 2)
                    {
                        anyMatch = true;
                        break;
                    }

                }
                if (anyMatch) {
                    _events.PostOneShot(new BoardStateTransitionEvent {
                        NewState = BoardState.Ready
                    });
                    return;
                }
                UnityEngine.Debug.Log("No more matches! Cleaning board");
                EntityManager.DestroyEntity(Entities.WithAll<GemComponent>().ToEntityQuery());
                _events.PostOneShot(new BoardStateTransitionEvent {
                    NewState = BoardState.Initializing
                });
            }
        }
    }
}
