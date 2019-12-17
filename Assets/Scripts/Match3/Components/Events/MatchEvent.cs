using Unity.Entities;

namespace Match3Game
{
    public struct MatchEvent : IComponentData
    {
        public Unity.Mathematics.int2 Position;
        public GemType Gem;
        public byte MatchLength;
    }
}