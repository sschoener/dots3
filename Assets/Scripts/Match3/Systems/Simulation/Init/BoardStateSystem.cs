using Unity.Entities;

namespace Match3Game
{
    [UpdateInGroup(typeof(Match3InitGroup))]
    public class BoardStateSystem : ComponentSystem
    {
        private EntityQuery _gemQuery;
        private EntityQuery _animationQuery;
        private static readonly BoardState[] _matchTargets = {
            BoardState.Animating,
            BoardState.Ready,
            BoardState.Initializing
        };

        protected override void OnCreate()
        {
            RequireSingletonForUpdate<GameComponent>();
            _gemQuery = Entities.WithAllReadOnly<GemComponent, BoardPositionComponent>().ToEntityQuery();
            _animationQuery = Entities.WithAllReadOnly<BoardAnimationComponent>().ToEntityQuery();
        }

        protected override void OnUpdate()
        {
            var board = GetSingleton<BoardComponent>();
            var size = board.Size;
            int total = size.x * size.y;
            bool isFull = total == _gemQuery.CalculateEntityCount();
            bool isStatic = _animationQuery.CalculateEntityCount() == 0;

            var state = GetSingleton<BoardStateComponent>();
            var oldState = state.State;

            bool boardIsReady = isFull && isStatic;
            switch (state.State)
            {
                case BoardState.Animating:
                    {
                        if (boardIsReady)
                        {
                            state.State = BoardState.LookingForMatch;
                        }
                        break;
                    }
                case BoardState.Initializing:
                    {
                        state.State = StateMachineHelper.TryFindTransition<BoardStateTransitionEvent, BoardState>(
                            Entities, state.State, BoardState.Animating
                        );
                        break;
                    }
                case BoardState.LookingForMatch:
                    {
                        state.State = StateMachineHelper.TryFindTransition<BoardStateTransitionEvent, BoardState>(
                            Entities, state.State, _matchTargets
                        );
                        break;
                    }
                case BoardState.Ready:
                    {
                        if (!boardIsReady)
                        {
                            state.State = BoardState.Animating;
                        }
                        break;
                    }
            }
            if (state.State != oldState) {
                UnityEngine.Debug.Log($"[Board] {state.State}");
                SetSingleton(state);
            }
        }
    }
}
