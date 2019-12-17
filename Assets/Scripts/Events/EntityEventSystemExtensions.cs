using BovineLabs.Event;
using Unity.Entities;

namespace Match3Game
{
    public static class EntityEventSystemExtensions {
        public static void PostOneShot<T>(this EntityEventSystem events, T eventData) where T : struct, IComponentData {
            events.CreateEventQueue<T>().Enqueue(eventData);
        }
    }
}