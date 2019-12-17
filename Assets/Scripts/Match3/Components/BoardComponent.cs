using Unity.Entities;
using Unity.Mathematics;

namespace Match3Game
{
    public struct BoardComponent : IComponentData {
        public int2 Size;
        public float CellSize;

        public float2 MinGridPosition => -GridDim / 2;
        public float2 MaxGridPosition => GridDim / 2;
        public float2 GridDim => Size * new float2(CellSize, CellSize);

        public float2 GridCellCenter(int x, int y) => MinGridPosition + CellSize * new float2(x + .5f, y + .5f);
        public float2 GridCellCenter(int2 p) => MinGridPosition + CellSize * new float2(p.x + .5f, p.y + .5f);
    }
}