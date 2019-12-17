using Unity.Entities;
using UnityEngine;

namespace Match3Game
{
    public class Bootstrap : MonoBehaviour
    {
        private void Awake() {
            var world = World.DefaultGameObjectInjectionWorld;
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            var conversation = em.CreateEntity(
                typeof(GameComponent),
                typeof(TurnComponent),
                typeof(BoardStateComponent)
            );
            em.SetComponentData(conversation, new GameComponent
            {

            });

            world.GetExistingSystem<EventSystem>().PostOneShot(new GameStartEvent());
        }
    }
}