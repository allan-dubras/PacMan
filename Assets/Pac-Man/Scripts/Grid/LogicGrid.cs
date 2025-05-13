using UnityEngine;
using UnityEngine.Tilemaps;

public class LogicGrid : MonoBehaviour
{
    [SerializeField] private Tilemap collisionMap;

    public LogicCell[,] grid;
    public Vector2Int origin;
    public Vector2Int size;

    private void Awake()
    {
        BuildGrid();
    }
    
    public void BuildGrid()
    {
        var bounds = collisionMap.cellBounds;

        size = new Vector2Int(bounds.size.x, bounds.size.y);
        origin = new Vector2Int(bounds.xMin, bounds.yMin);
        grid = new LogicCell[size.x, size.y];

        foreach (var pos in bounds.allPositionsWithin)
        {
            var tile = collisionMap.GetTile(pos) as WalkableTile;
            if (tile == null) continue;

            int x = pos.x - origin.x;
            int y = pos.y - origin.y;

            grid[x, y] = new LogicCell
            {
                tileType = tile.tileType,
                id = tile.id
            };
        }

        Debug.Log($"[LogicGrid] Table logique générée ({size.x}×{size.y}).");
    }

    public LogicCell GetCell(Vector3Int cell)
    {
        int x = cell.x - origin.x;
        int y = cell.y - origin.y;

        if (x < 0 || x >= size.x || y < 0 || y >= size.y)
            return null;

        return grid[x, y];
    }
}
