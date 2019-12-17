using Unity.Entities;
using UnityEngine;

namespace Match3Game
{
    public class GameBoard : MonoBehaviour, IConvertGameObjectToEntity
    {
#pragma warning disable 649

        [Header("Board Setup")]
        [Tooltip("The size of the board. Must be > 0 in both dimensions.")]
        [SerializeField]
        private Vector2Int _size;

        [Tooltip("The size of a cell on the board. Must be > 0.")]
        [SerializeField]
        private float _cellSize = 1;

        [SerializeField]
        private SpriteRenderer _backgroundTile;

#pragma warning restore 649

        private Vector2 MinGrid => new Vector2(
            -.5f * (_size.x - 1) * _cellSize - _cellSize / 2,
            -.5f * (_size.y - 1) * _cellSize - _cellSize / 2
        );
        private Vector2 MaxGrid => new Vector2(
            .5f * (_size.x - 1) * _cellSize + _cellSize / 2,
            .5f * (_size.y - 1) * _cellSize + _cellSize / 2
        );

        private Vector2 GetLocalCellCenter(int x, int y) => MinGrid + _cellSize * new Vector2(x + .5f, y + .5f);

        private void Start()
        {
            Application.targetFrameRate = 60;

            var tile = Instantiate(_backgroundTile, transform.position + Vector3.forward, Quaternion.identity);
            tile.transform.localScale = new Vector3(_cellSize, _cellSize, _cellSize);
            tile.size = _size;
        }

        private void OnDrawGizmosSelected()
        {
            Vector3[] corners = new Vector3[4] {
                transform.TransformPoint(MinGrid),
                transform.TransformPoint(GetLocalCellCenter(_size.x - 1, 0) + new Vector2(_cellSize/2, -_cellSize/2)),
                transform.TransformPoint(MaxGrid),
                transform.TransformPoint(GetLocalCellCenter(0, _size.y - 1) + new Vector2(-_cellSize/2, _cellSize/2)),
            };
            for (int i = 0; i < corners.Length; i++)
            {
                Gizmos.DrawLine(corners[i], corners[(i + 1) % corners.Length]);
            }

            for (int x = 0; x < _size.x; x++)
            {
                for (int y = 0; y < _size.y; y++)
                {
                    Gizmos.color = Color.yellow;
                    Vector3 point = transform.TransformPoint(GetLocalCellCenter(x, y));
                    Gizmos.DrawSphere(point, 0.1f);
                    GUI.contentColor = Color.red;
                    UnityEditor.Handles.Label(point, $"({x}, {y})", GizmoStyles.Label);
                }
            }
        }

        void IConvertGameObjectToEntity.Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new BoardComponent{
                CellSize = _cellSize,
                Size = new Unity.Mathematics.int2(_size.x, _size.y)
            });
        }
    }
}
