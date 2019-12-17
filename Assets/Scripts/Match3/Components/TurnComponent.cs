using Unity.Entities;

namespace Match3Game
{
    public struct TurnComponent : IComponentData {
        public TurnState State;
    }
}