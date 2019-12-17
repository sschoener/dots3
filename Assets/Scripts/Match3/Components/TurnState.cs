namespace Match3Game
{
    public enum TurnState : byte {
        Begin = 0,
        ReceiveAction,
        PerformAction,
        FinishAction,
        End
    }
}