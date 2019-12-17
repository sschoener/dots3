using Unity.Collections;
using Unity.Mathematics;

namespace Match3Game
{
    public struct MatchInfo
    {
        public byte MatchUp;
        public byte MatchLeft;
        public byte MatchRight;
        public byte MatchDown;

        public int VerticalLength => 1 + MatchUp + MatchDown;
        public int HorizontalLength => 1 + MatchLeft + MatchRight;
        public int MaxLength => math.max(VerticalLength, HorizontalLength);
    }

    public struct GemMatchInfo
    {
        public GemType Gem;
        public MatchInfo Match;
    }

    public struct SwapMatches
    {
        public GemMatchInfo SwapUp1;
        public GemMatchInfo SwapUp2;
        public GemMatchInfo SwapRight1;
        public GemMatchInfo SwapRight2;
    }

    public static class MatchFinder
    {
        public static void FindMatches<T>(T[,] board, MatchInfo[,] matches)
        {
            int width = board.GetLength(0);
            int height = board.GetLength(1);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var match = new MatchInfo();

                    ref T gem = ref board[x, y];
                    // find matches in the 4 directions
                    for (int cy = y - 1; cy >= 0 && board[x, cy].Equals(gem); cy--)
                        match.MatchDown++;
                    for (int cy = y + 1; cy < height && board[x, cy].Equals(gem); cy++)
                        match.MatchUp++;
                    for (int cx = x - 1; cx >= 0 && board[cx, y].Equals(gem); cx--)
                        match.MatchLeft++;
                    for (int cx = x + 1; cx < width && board[cx, y].Equals(gem); cx++)
                        match.MatchRight++;

                    matches[x, y] = match;
                }
            }
        }

        public static void FindMatches<T>(int width, NativeArray<T> board, NativeArray<MatchInfo> matches) where T : struct
        {
            int height = board.Length / width;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    matches[Index(x, y)] = FindMatch(width, height, board, new int2(x, y));
                }
            }

            int Index(int x, int y) => x * height + y;
        }

        private static MatchInfo FindMatch<T>(int width, int height, NativeArray<T> board, int2 at) where T : struct
        {
            {
                var match = new MatchInfo();
                int x = at.x;
                int y = at.y;
                var gem = board[Index(x, y)];
                // find matches in the 4 directions
                for (int cy = y - 1; cy >= 0 && board[Index(x, cy)].Equals(gem); cy--)
                    match.MatchDown++;
                for (int cy = y + 1; cy < height && board[Index(x, cy)].Equals(gem); cy++)
                    match.MatchUp++;
                for (int cx = x - 1; cx >= 0 && board[Index(cx, y)].Equals(gem); cx--)
                    match.MatchLeft++;
                for (int cx = x + 1; cx < width && board[Index(cx, y)].Equals(gem); cx++)
                    match.MatchRight++;
                return match;
            }

            int Index(int x, int y) => x * height + y;
        }

        public static void ScoreSwapMoves(int width, NativeArray<GemType> board, NativeArray<SwapMatches> matches)
        {
            int height = board.Length / width;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var match = new SwapMatches();
                    if (y < height - 1)
                    {
                        Swap(x, y, x, y + 1);
                        match.SwapUp1.Match = FindMatch(width, height, board, new int2(x, y));
                        match.SwapUp1.Gem = board[Index(x, y)];
                        match.SwapUp2.Match = FindMatch(width, height, board, new int2(x, y + 1));
                        match.SwapUp2.Gem = board[Index(x, y + 1)];
                        Swap(x, y, x, y + 1);
                    }
                    if (x < width - 1)
                    {
                        Swap(x, y, x + 1, y);
                        match.SwapRight1.Match = FindMatch(width, height, board, new int2(x, y));
                        match.SwapRight1.Gem = board[Index(x, y)];
                        match.SwapRight2.Match = FindMatch(width, height, board, new int2(x + 1, y));
                        match.SwapRight2.Gem = board[Index(x + 1, y)];
                        Swap(x, y, x + 1, y);
                    }
                    matches[Index(x, y)] = match;
                }
            }

            void Swap(int x1, int y1, int x2, int y2)
            {
                int idx1 = Index(x1, y1);
                int idx2 = Index(x2, y2);
                GemType tmp = board[idx1];
                board[idx1] = board[idx2];
                board[idx2] = tmp;
            }
            int Index(int x, int y) => x * height + y;
        }
    }
}
