using Unity.Entities;

namespace Match3Game
{
    public struct BoardStateComponent : IComponentData
    {
        public BoardState State;
    }
}