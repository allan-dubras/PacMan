using System.Collections;
using System.Collections.Generic;
using Ny;
using UnityEngine;

public class pinky : GhostController1
{
    protected override Vector3Int CalculateTargetCell(Vector3Int cell)
    {
        var RawCell = CollisionMap.WorldToCell(pacman.transform.position + new Vector3(pacman.CurrentDir.x, pacman.CurrentDir.y, 0) * 4 * TileSize);


        //4) Wrap-Around modulo taille de la tilemap (comme l'original)
        var bounds = CollisionMap.cellBounds;
        int w = bounds.size.x, h = bounds.size.y;
        int x0 = bounds.xMin, y0 = bounds.yMin;

        int tx = (RawCell.x - x0) % w;
        if (tx < 0) tx += w;
        tx += x0;

        int ty = (RawCell.y - y0) % h;
        if (ty < 0) ty += h;
        ty += y0;

        return new Vector3Int(tx, ty, 0);
    }
}
