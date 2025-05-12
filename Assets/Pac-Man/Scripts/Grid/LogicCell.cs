using UnityEngine;

public class LogicCell
{
    public Vector3Int coord;    // (x,y) de la cellule
    public TileType tileType;   // Path / Wall / Warp / Portal / GhostGate / etc.
    public string id;           // ton id libre éventuel

    // propriétés calculées
    public bool isWalkable =>
        tileType == TileType.Path
     || tileType == TileType.Warp
     || tileType == TileType.Intersection;

    public bool isGhostWalkable =>
        tileType == TileType.Path
        || tileType == TileType.Intersection;

    public bool isWall =>
        tileType == TileType.Wall;

    public bool isPortal =>
        tileType == TileType.Portal;

    public bool isWarp =>
        tileType == TileType.Warp;

    public bool isGhostGate =>
        tileType == TileType.GhostGate;

    public bool isIntersection =>
        tileType == TileType.Intersection;
}
