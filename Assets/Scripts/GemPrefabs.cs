using Match3Game;
using UnityEngine;

namespace Match3Game
{
    [CreateAssetMenu(menuName="Match3/GemPrefab",fileName="GemPrefabs")]
    class GemPrefabs : ScriptableObject {
        #pragma warning disable 0649
        public GemAuthoring[] Gems;
        #pragma warning restore 0694
    }
}