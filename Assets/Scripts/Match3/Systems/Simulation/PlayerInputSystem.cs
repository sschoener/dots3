using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Match3Game
{
    [UpdateInGroup(typeof(Match3SimulationGroup))]
    public class PlayerInputSystem : ComponentSystem
    {
        private Camera _camera;
        private EntityQuery _boardSelectionQuery;
        protected override void OnCreate()
        {
            RequireSingletonForUpdate<GameComponent>();
            RequireSingletonForUpdate<BoardStateComponent>();
            RequireSingletonForUpdate<TurnComponent>();
            _boardSelectionQuery = Entities.WithAll<BoardSelectionComponent>().ToEntityQuery();
        }

        protected override void OnStartRunning()
        {
            _camera = Camera.main;
            Debug.Assert(_camera != null);
        }

        private bool GetCell(Ray r, in BoardComponent board, out int2 cell)
        {
            Plane p = new Plane(new Vector3(0, 0, 1), new Vector3(0, 0, 0));
            if (p.Raycast(r, out float t))
            {
                float3 point3 = r.GetPoint(t);
                float2 point = new float2(point3.x, point3.y);
                var inGrid = (point - board.MinGridPosition) / board.CellSize;
                int x = Mathf.FloorToInt(inGrid.x);
                int y = Mathf.FloorToInt(inGrid.y);
                cell = new int2(x, y);
                return true;
            }
            cell = new int2();
            return false;
        }

        protected override void OnUpdate()
        {
            if (!Input.GetMouseButtonDown(0))
                return;
            {
                var turn = GetSingleton<TurnComponent>();
                if (turn.State != TurnState.ReceiveAction)
                    return;
            }

            var board = GetSingleton<BoardComponent>();

            var ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (!GetCell(ray, in board, out int2 cell))
            {
                EntityManager.DestroyEntity(_boardSelectionQuery);
                return;
            }
            bool validCell = cell.x >= 0 && cell.x < board.Size.x && cell.y >= 0 && cell.y < board.Size.y;
            if (!validCell)
            {
                EntityManager.DestroyEntity(_boardSelectionQuery);
                return;
            }

            if (!HasSingleton<BoardSelectionComponent>())
            {
                var selection = EntityManager.InstantiatePrefabSingleton<SelectionMarkerComponent>();
                EntityManager.SetComponentData(selection, new Translation
                {
                    Value = new float3(board.GridCellCenter(cell.x, cell.y), -2)
                });
                EntityManager.AddComponentData(selection, new BoardSelectionComponent
                {
                    Selection = cell
                });
                return;
            }

            var boardSelection = GetSingletonEntity<BoardSelectionComponent>();
            var s = EntityManager.GetComponentData<BoardSelectionComponent>(boardSelection);
            if (math.lengthsq(cell - s.Selection) == 1)
            {
                var move = EntityManager.CreateEntity(
                    typeof(MoveComponent),
                    typeof(SwapMoveComponent)
                );
                EntityManager.SetComponentData(move, new SwapMoveComponent
                {
                    From = s.Selection,
                    To = cell
                });
            }
            EntityManager.DestroyEntity(boardSelection);
        }
    }
}
