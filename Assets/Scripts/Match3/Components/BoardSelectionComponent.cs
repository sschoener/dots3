using Unity.Entities;
using Unity.Mathematics;

namespace Match3Game
{
    public struct BoardSelectionComponent : IComponentData
    {
        public int2 Selection;
    }
}