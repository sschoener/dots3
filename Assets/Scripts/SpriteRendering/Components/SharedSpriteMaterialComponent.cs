using System;
using Unity.Entities;
using UnityEngine;

namespace Match3Game
{
    public struct SharedSpriteMaterialComponent : ISharedComponentData, IEquatable<SharedSpriteMaterialComponent>
    {
        public Material Material;
        public int Layer;

        bool IEquatable<SharedSpriteMaterialComponent>.Equals(SharedSpriteMaterialComponent other) =>
            Layer == other.Layer && Material == other.Material;

        public override int GetHashCode() => Material.GetHashCode() ^ Layer.GetHashCode();
    }
}
