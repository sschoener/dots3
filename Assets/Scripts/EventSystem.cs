using Unity.Entities;

namespace Match3Game
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class EventSystem : BovineLabs.Event.EntityEventSystem { }
}
