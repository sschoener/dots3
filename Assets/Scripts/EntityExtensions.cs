using Unity.Entities;

namespace Match3Game
{
    public static class EntityExtensions {
        public static Entity InstantiatePrefabSingleton<T>(this EntityManager manager) =>
            manager.Instantiate(
                manager.CreateEntityQuery(new EntityQueryDesc {
                    All = new ComponentType[] { typeof(Prefab), typeof(T) },
                    Options = EntityQueryOptions.IncludePrefab
                }).GetSingletonEntity()
            );
    }
}
