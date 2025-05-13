using FixedEngine;
using Ny;
using UnityEngine;

public class Blinky : GhostController1
{
    
    protected override Vector3Int CalculateTargetCell(Vector3Int cell)
    {
        return CollisionMap.WorldToCell(pacman.transform.position);
    }
}
