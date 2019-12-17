using Unity.Entities;

namespace Match3Game
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(Match3SimulationGroup))]
    public class Match3InitGroup : ComponentSystemGroup
    {
    }
}
