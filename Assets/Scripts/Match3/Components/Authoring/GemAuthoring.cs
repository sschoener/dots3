using Unity.Entities;
using UnityEngine;

namespace Match3Game
{
    public class GemAuthoring : MonoBehaviour, IConvertGameObjectToEntity {
#pragma warning disable 649
        [SerializeField]
        private GemType _gemType;
        public GemType GemType => _gemType;
#pragma warning restore 649

        void IConvertGameObjectToEntity.Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new GemComponent{
                GemType = _gemType
            });
        }
    }
}
