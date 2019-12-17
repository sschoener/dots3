using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Match3Game
{
    public class SpriteRendererConversionSystem : GameObjectConversionSystem
    {
        private const string SpriteShaderName = "Shader Graphs/SpriteShader";
        private readonly Dictionary<Texture2D, Material> _textureMaterial = new Dictionary<Texture2D, Material>();
        private Shader _spriteShader;

        protected override void OnCreate()
        {
            base.OnCreate();
            _spriteShader = Shader.Find(SpriteShaderName);
            Debug.Assert(_spriteShader != null, $"The specified sprite shader \"{SpriteShaderName}\" does not exist.");
        }

        protected override void OnUpdate()
        {
            Entities.ForEach((SpriteRenderer sr) =>
            {
                var entity = GetPrimaryEntity(sr);

                if (sr.sprite.uv.Length != 4)
                {
                    Debug.LogError("Non-rect sprites are not supported right now.", sr);
                    return;
                }
                if (sr.drawMode != SpriteDrawMode.Simple)
                {
                    Debug.LogError("Non-simple sprites are not supported right now.", sr);
                    return;
                }
                if (sr.maskInteraction != SpriteMaskInteraction.None)
                {
                    Debug.LogError("Sprite mask interactions are not supported right now.", sr);
                    return;
                }
                if (sr.sortingOrder != 0)
                {
                    Debug.LogError("Sprite sorting order is not supported right now.", sr);
                    return;
                }

                float2 minUV = sr.sprite.uv[0];
                float2 maxUV = sr.sprite.uv[0];
                for (int i = 0; i < 4; i++)
                {
                    minUV = math.min(minUV, sr.sprite.uv[i]);
                    maxUV = math.max(maxUV, sr.sprite.uv[i]);
                }

                if (sr.flipX)
                {
                    float tmp = minUV.x;
                    minUV.x = maxUV.x;
                    maxUV.x = tmp;
                }
                if (sr.flipY)
                {
                    float tmp = minUV.y;
                    minUV.y = maxUV.y;
                    maxUV.y = tmp;
                }

                DstEntityManager.AddComponentData(entity, new SpriteComponent
                {
                    MinUV = minUV,
                    MaxUV = maxUV,
                    Color = (Vector4)sr.color,
                    HalfSize = (Vector2)sr.sprite.bounds.extents,
                });

                if (!_textureMaterial.TryGetValue(sr.sprite.texture, out Material mat))
                {
                    mat = new Material(_spriteShader)
                    {
                        mainTexture = sr.sprite.texture
                    };
                    _textureMaterial[sr.sprite.texture] = mat;
                }

                DstEntityManager.AddSharedComponentData(entity, new SharedSpriteMaterialComponent
                {
                    Layer = sr.sortingLayerID,
                    Material = mat
                });
            });
        }
    }
}
