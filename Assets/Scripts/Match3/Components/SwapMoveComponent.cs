using Unity.Entities;
using Unity.Mathematics;

namespace Match3Game
{
    public struct SwapMoveComponent : IComponentData
    {
        public int2 From;
        public int2 To;
    }
}