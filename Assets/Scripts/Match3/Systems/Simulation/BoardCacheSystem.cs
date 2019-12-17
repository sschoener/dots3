using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Match3Game
{
    [UpdateInGroup(typeof(Match3SimulationGroup))]
    public class BoardCacheSystem : ComponentSystem
    {
        private NativeArray<GemType> _board;
        public NativeArray<GemType> Board => _board;
        private NativeArray<float> _topGem;
        private int2 _boardSize;

        public GemType GetGem(int x, int y) => _board[Index(x, y)];
        public float GetMaxHeight(int column) => _topGem[column];
        public int BoardWidth => _boardSize.x;
        public int BoardHeight => _boardSize.y;

        private int Index(int x, int y) => x * _boardSize.y + y;

        protected override void OnCreate()
        {
            RequireSingletonForUpdate<GameComponent>();
        }

        protected override void OnDestroy()
        {
            if (_board.IsCreated)
                _board.Dispose();
            if (_topGem.IsCreated)
                _topGem.Dispose();
        }

        protected override void OnUpdate()
        {
            var board = GetSingleton<BoardComponent>();
            if (!math.all(_boardSize == board.Size)) {
                if (_board.IsCreated)
                    _board.Dispose();
                if (_topGem.IsCreated)
                    _topGem.Dispose();
                _boardSize = board.Size;
                _board = new NativeArray<GemType>(_boardSize.x * _boardSize.y, Allocator.Persistent);
                _topGem = new NativeArray<float>(_boardSize.x, Allocator.Persistent);
            }

            for (int i = 0; i < _board.Length; i++)
                _board[i] = GemType.None;
            for (int i = 0; i < _topGem.Length; i++)
                _topGem[i] = 0;
            
            Entities.ForEach((ref BoardPositionComponent boardPosComp, ref GemComponent gem, ref Translation pos) => {
                var boardPos = boardPosComp.BoardPosition;
                _board[Index(boardPos.x, boardPos.y)] = gem.GemType;
                _topGem[boardPos.x] = math.max(_topGem[boardPos.x], pos.Value.y);
            });
        }
    }
}
