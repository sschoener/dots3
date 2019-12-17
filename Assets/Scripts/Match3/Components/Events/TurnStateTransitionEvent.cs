using Unity.Entities;

namespace Match3Game
{
    public struct TurnStateTransitionEvent : IComponentData, StateMachineHelper.IStateTransition<TurnState>
    {
        public TurnState NewState;

        TurnState StateMachineHelper.IStateTransition<TurnState>.State => NewState;
    }
}
