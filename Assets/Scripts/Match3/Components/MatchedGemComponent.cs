using Unity.Entities;

namespace Match3Game
{
    public struct MatchedGemComponent : IComponentData {
        public MatchInfo Match;
    }
}