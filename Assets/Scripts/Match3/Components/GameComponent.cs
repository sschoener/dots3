using Unity.Entities;

namespace Match3Game
{
    public struct GameComponent : IComponentData
    {
        public int TurnsLeft;
    }
}