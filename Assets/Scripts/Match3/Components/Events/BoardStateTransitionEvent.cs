using Unity.Entities;

namespace Match3Game
{
    public struct BoardStateTransitionEvent : IComponentData, StateMachineHelper.IStateTransition<BoardState> {
        public BoardState NewState;

        BoardState StateMachineHelper.IStateTransition<BoardState>.State => NewState;
    }
}
