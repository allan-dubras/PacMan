using System.Collections;
using System.Collections.Generic;
using Ny;
using UnityEngine;

public class Clyde : GhostController1
{
    [SerializeField] private GameObject PointBlock;
    protected override Vector3Int CalculateTargetCell(Vector3Int cell)
    {
        return CollisionMap.WorldToCell(pacman.transform.position);
    }
}
