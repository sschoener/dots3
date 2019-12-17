
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Match3Game
{
    class Prefabs : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs {

        #pragma warning disable 0649
        public GemPrefabs Gems;
        public GameObject SelectionMarker;
        #pragma warning restore 0649

        void IConvertGameObjectToEntity.Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
        }

        void IDeclareReferencedPrefabs.DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            foreach (var g in Gems.Gems) 
                referencedPrefabs.Add(g.gameObject);
            referencedPrefabs.Add(SelectionMarker);
        }
    }
}