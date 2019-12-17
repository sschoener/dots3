using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;

namespace Match3Game
{
    public struct ChunkSpriteMeshComponent : IComponentData
    {
        public int ChunkMeshId;
    }

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class SpriteDisplaySystem : ComponentSystem
    {
        private BatchRendererGroup _renderGroup;
        private readonly Dictionary<int, ChunkMeshData> _meshById = new Dictionary<int, ChunkMeshData>();
        private readonly List<int> _meshIds = new List<int>();
        private int _nextMeshId = 1;

        private EntityQuery _spriteEntities;
        private EntityQuery _spriteWithMesh;
        private EntityQuery _spriteMissingMesh;
        private struct ChunkMeshData
        {
            public SharedSpriteMaterialComponent Material;
            public uint MeshVersion;
            public uint MaterialVersion;
            public Mesh Mesh;
        }

        protected override void OnCreate()
        {
            _renderGroup = new BatchRendererGroup(OnPerformCulling);
            _spriteEntities = Entities.WithAllReadOnly<SpriteComponent, LocalToWorld, SharedSpriteMaterialComponent>().ToEntityQuery();
            _spriteWithMesh = GetEntityQuery(new EntityQueryDesc
            {
                All = new[] {
                    ComponentType.ReadOnly<SharedSpriteMaterialComponent>(),
                    ComponentType.ChunkComponentReadOnly<ChunkSpriteMeshComponent>()
                },
            });
            _spriteMissingMesh = GetEntityQuery(new EntityQueryDesc
            {
                All = new[] { ComponentType.ReadOnly<SharedSpriteMaterialComponent>() },
                None = new[] { ComponentType.ChunkComponent<ChunkSpriteMeshComponent>() }
            });
        }

        protected override void OnDestroy()
        {
            _renderGroup.Dispose();
            foreach (var kvp in _meshById)
            {
                UnityEngine.Object.Destroy(kvp.Value.Mesh);
            }
        }

        private void UpdateMeshes()
        {
            EntityManager.AddChunkComponentData(_spriteMissingMesh, new ChunkSpriteMeshComponent
            {
                ChunkMeshId = -1
            });

            using (var chunks = _spriteEntities.CreateArchetypeChunkArray(Allocator.TempJob))
            {
                var materialType = GetArchetypeChunkSharedComponentType<SharedSpriteMaterialComponent>();
                var chunkSpriteMeshType = GetArchetypeChunkComponentType<ChunkSpriteMeshComponent>();
                var spriteComponentType = GetArchetypeChunkComponentType<SpriteComponent>(true);
                var localToWorldType = GetArchetypeChunkComponentType<LocalToWorld>(true);
                for (int chunkIdx = 0; chunkIdx < chunks.Length; chunkIdx++)
                {
                    var chunk = chunks[chunkIdx];

                    var spriteMaterial = chunk.GetSharedComponentData(materialType, EntityManager);

                    // find mesh
                    ChunkMeshData meshData;
                    {
                        var chunkSpriteMesh = chunk.GetChunkComponentData(chunkSpriteMeshType);
                        if (chunkSpriteMesh.ChunkMeshId <= 0)
                        {
                            chunkSpriteMesh.ChunkMeshId = _nextMeshId++;
                            chunk.SetChunkComponentData(chunkSpriteMeshType, chunkSpriteMesh);
                            meshData = new ChunkMeshData
                            {
                                Material = chunk.GetSharedComponentData(materialType, EntityManager),
                                Mesh = new Mesh(),
                                MaterialVersion = LastSystemVersion,
                            };
                        }
                        else
                        {
                            meshData = _meshById[chunkSpriteMesh.ChunkMeshId];
                            bool hasMaterialChanged = chunk.DidChange(materialType, meshData.MaterialVersion) && !spriteMaterial.Equals(meshData.Material);
                            if (!chunk.DidChange(localToWorldType, meshData.MeshVersion) &&
                                !chunk.DidChange(spriteComponentType, meshData.MeshVersion) &&
                                !hasMaterialChanged)
                            {
                                continue;
                            }
                            if (hasMaterialChanged) {
                                meshData.Material = spriteMaterial;
                                meshData.MaterialVersion = LastSystemVersion;
                            }
                        }
                        meshData.MeshVersion = LastSystemVersion;
                        _meshById[chunkSpriteMesh.ChunkMeshId] = meshData;
                    }

                    // generate mesh
                    int numVertices = 4 * chunk.Count;
                    var vertices = new NativeArray<float3>(numVertices, Allocator.Temp);
                    var triangles = new NativeArray<int>(2 * 3 * chunk.Count, Allocator.Temp);
                    var uvs = new NativeArray<float2>(numVertices, Allocator.Temp);
                    var colors = new NativeArray<float4>(numVertices, Allocator.Temp);
                    var localToWorld = chunk.GetNativeArray(localToWorldType);
                    var sprites = chunk.GetNativeArray(spriteComponentType);

                    for (int i = 0; i < chunk.Count; i++)
                    {
                        float4 size = new float4(-sprites[i].HalfSize, sprites[i].HalfSize);
                        vertices[4 * i + 0] = math.mul(localToWorld[i].Value, new float4(size.xy, 0, 1)).xyz;
                        vertices[4 * i + 1] = math.mul(localToWorld[i].Value, new float4(size.zy, 0, 1)).xyz;
                        vertices[4 * i + 2] = math.mul(localToWorld[i].Value, new float4(size.zw, 0, 1)).xyz;
                        vertices[4 * i + 3] = math.mul(localToWorld[i].Value, new float4(size.xw, 0, 1)).xyz;
                    }

                    for (int i = 0; i < chunk.Count; i++)
                    {
                        triangles[6 * i + 0] = 4 * i + 0;
                        triangles[6 * i + 1] = 4 * i + 2;
                        triangles[6 * i + 2] = 4 * i + 1;
                        triangles[6 * i + 3] = 4 * i + 0;
                        triangles[6 * i + 4] = 4 * i + 3;
                        triangles[6 * i + 5] = 4 * i + 2;
                    }

                    for (int i = 0; i < chunk.Count; i++)
                    {
                        uvs[4 * i + 0] = sprites[i].MinUV;
                        uvs[4 * i + 1] = new float2(sprites[i].MaxUV.x, sprites[i].MinUV.y);
                        uvs[4 * i + 2] = sprites[i].MaxUV;
                        uvs[4 * i + 3] = new float2(sprites[i].MinUV.x, sprites[i].MaxUV.y);

                        colors[4 * i + 0] = sprites[i].Color;
                        colors[4 * i + 1] = sprites[i].Color;
                        colors[4 * i + 2] = sprites[i].Color;
                        colors[4 * i + 3] = sprites[i].Color;
                    }

                    Mesh mesh = meshData.Mesh;
                    mesh.Clear();
                    mesh.SetVertices(vertices);
                    mesh.SetIndices(triangles, MeshTopology.Triangles, 0);
                    mesh.SetUVs(0, uvs);
                    mesh.SetColors(colors);
                    vertices.Dispose();
                    triangles.Dispose();
                    uvs.Dispose();
                    colors.Dispose();
                }
            }
        }

        private void CleanupMeshes()
        {
            using (var missing = new NativeHashMap<int, bool>(_meshById.Count, Allocator.Temp))
            using (var chunks = _spriteWithMesh.CreateArchetypeChunkArray(Allocator.TempJob))
            {
                foreach (var meshId in _meshById.Keys)
                {
                    missing.TryAdd(meshId, true);
                }

                var chunkSpriteMeshType = GetArchetypeChunkComponentType<ChunkSpriteMeshComponent>(true);
                for (int i = 0; i < chunks.Length; i++)
                {
                    var mesh = chunks[i].GetChunkComponentData(chunkSpriteMeshType);
                    missing.Remove(mesh.ChunkMeshId);
                }

                using (var keys = missing.GetKeyArray(Allocator.Temp))
                {
                    for (int i = 0; i < keys.Length; i++)
                    {
                        if (_meshById.TryGetValue(keys[i], out var mesh))
                        {
                            UnityEngine.Object.Destroy(mesh.Mesh);
                        }
                        int renderIndex = _meshIds.IndexOf(keys[i]);
                        _renderGroup.RemoveBatch(renderIndex);
                        _meshIds.RemoveAtSwapBack(renderIndex);
                        _meshById.Remove(keys[i]);
                    }
                }
            }
        }

        private void RenderMeshes()
        {
            using (var chunks = _spriteWithMesh.CreateArchetypeChunkArray(Allocator.TempJob))
            {
                var sharedSpriteMaterialType = GetArchetypeChunkSharedComponentType<SharedSpriteMaterialComponent>();
                var chunkSpriteMeshType = GetArchetypeChunkComponentType<ChunkSpriteMeshComponent>(true);
                for (int chunkIdx = 0; chunkIdx < chunks.Length; chunkIdx++)
                {
                    var chunk = chunks[chunkIdx];
                    var chunkSpriteMesh = chunk.GetChunkComponentData(chunkSpriteMeshType);
                    int chunkMeshId = chunkSpriteMesh.ChunkMeshId;
                    var meshData = _meshById[chunkMeshId];

                    if (meshData.MaterialVersion != LastSystemVersion)
                        continue;

                    int renderIndex = _meshIds.IndexOf(chunkMeshId);
                    if (renderIndex >= 0)
                    {
                        _renderGroup.RemoveBatch(renderIndex);
                        _meshIds.RemoveAtSwapBack(renderIndex);
                    }

                    var sharedSprite = chunk.GetSharedComponentData(sharedSpriteMaterialType, EntityManager);
                    renderIndex = _renderGroup.AddBatch(
                        meshData.Mesh,
                        subMeshIndex: 0,
                        sharedSprite.Material,
                        sharedSprite.Layer,
                        ShadowCastingMode.Off,
                        receiveShadows: false,
                        invertCulling: false,
                        meshData.Mesh.bounds,
                        instanceCount: 1,
                        customProps: null,
                        associatedSceneObject: null
                    );
                    Debug.Assert(renderIndex == _meshIds.Count);
                    _meshIds.Add(chunkMeshId);
                    var mvp = _renderGroup.GetBatchMatrices(renderIndex);
                    mvp[0] = float4x4.identity;
                }
            }
        }

        protected override void OnUpdate()
        {
            CleanupMeshes();
            UpdateMeshes();
            RenderMeshes();
        }

        private JobHandle OnPerformCulling(BatchRendererGroup rendererGroup, BatchCullingContext cullingContext)
        {
            // no culling at all! this just sets up everything as visible.
            for (int batch = 0; batch < cullingContext.batchVisibility.Length; batch++)
            {
                var bv = cullingContext.batchVisibility[batch];
                bv.visibleCount = bv.instancesCount;
                cullingContext.batchVisibility[batch] = bv;
                for (int instance = 0; instance < bv.instancesCount; instance++)
                {
                    cullingContext.visibleIndices[bv.offset + instance] = instance;
                }
            }
            return new JobHandle();
        }
    }
}
