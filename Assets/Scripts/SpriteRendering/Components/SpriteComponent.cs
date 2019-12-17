using Unity.Entities;
using Unity.Mathematics;

namespace Match3Game
{
    public struct SpriteComponent : IComponentData
    {
        public float2 MinUV;
        public float2 MaxUV;
        public float4 Color;
        public float2 HalfSize;
    }
}
