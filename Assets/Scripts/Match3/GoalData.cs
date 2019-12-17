using System;

namespace Match3Game
{
    [Serializable]
    public struct GoalData
    {
        public GemType GemType;
        public int Value;
        public int TargetValue;
    }
}
