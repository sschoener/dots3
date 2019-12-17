using Unity.Entities;
using Unity.Mathematics;

namespace Match3Game
{
    [UpdateInGroup(typeof(Match3InitGroup))]
    [UpdateAfter(typeof(BoardStateSystem))]
    public class TurnStateSystem : ComponentSystem
    {
        private EntityQuery _actionQuery;
        private EventSystem _events;

        protected override void OnCreate() 
        {
            RequireSingletonForUpdate<TurnComponent>();
            _events = World.GetOrCreateSystem<EventSystem>();
            _actionQuery = Entities.WithAll<ActionEvent>().ToEntityQuery();
        }

        protected override void OnUpdate()
        {
            var turn = GetSingleton<TurnComponent>();
            var oldState = turn.State;

            var boardState = GetSingleton<BoardStateComponent>().State;
            switch (turn.State)
            {
                case TurnState.Begin:
                    {
                        if (boardState == BoardState.Ready) {
                            turn.State = TurnState.ReceiveAction;
                        }
                        break;
                    }
                case TurnState.ReceiveAction:
                    {
                        if (_actionQuery.CalculateEntityCount() > 0)
                        {
                            turn.State = TurnState.PerformAction;
                        }
                        break;
                    }
                case TurnState.PerformAction:
                    {
                        turn.State = TurnState.FinishAction;
                        break;
                    }
                case TurnState.FinishAction:
                    {
                        if (boardState == BoardState.Ready)
                        {
                            turn.State = TurnState.End;
                            _events.PostOneShot(new TurnEndEvent());
                        }
                        break;
                    }
                case TurnState.End:
                    {
                        var game = GetSingleton<GameComponent>();
                        turn.State = TurnState.Begin;
                        break;
                    }
            }

            if (turn.State != oldState)
            {
                UnityEngine.Debug.Log($"[Turn] {turn.State}");
                SetSingleton(turn);
            }
        }
    }
}
