using Unity.Entities;

namespace Match3Game
{
    public struct GemComponent : IComponentData
    {
        public GemType GemType;
    }
}