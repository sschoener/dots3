using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Match3Game
{
    [UpdateInGroup(typeof(Match3SimulationGroup))]
    public class MatchAnimationSystem : ComponentSystem
    {
        private EntityQuery _query;
        protected override void OnCreate()
        {
            _query = Entities.WithAll<GemMatchAnimationComponent, NonUniformScale>().ToEntityQuery();
            RequireForUpdate(_query);
        }

        protected override void OnUpdate()
        {
            const float animationLength = .2f;
            float deltaTime = UnityEngine.Time.deltaTime;
            Entities.With(_query).ForEach((Entity e, ref GemMatchAnimationComponent match, ref NonUniformScale scale) =>
            {
                match.Progress = math.clamp(match.Progress + deltaTime / animationLength, 0, 1);
                scale.Value = scale.Value * (1 - match.Progress) + 1.4f * match.Progress;
                if (match.Progress >= 1)
                {
                    EntityManager.DestroyEntity(e);
                }
            });
        }
    }
}
