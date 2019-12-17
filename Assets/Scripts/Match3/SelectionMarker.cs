using Unity.Entities;
using UnityEngine;

namespace Match3Game
{
    public class SelectionMarker : MonoBehaviour, IConvertGameObjectToEntity
    {
        void IConvertGameObjectToEntity.Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponent<SelectionMarkerComponent>(entity);
        }
    }
}
