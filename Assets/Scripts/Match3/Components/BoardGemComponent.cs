using Unity.Entities;
using Unity.Mathematics;

namespace Match3Game
{
    public struct BoardPositionComponent : IComponentData {
        public int2 BoardPosition;
    }
}