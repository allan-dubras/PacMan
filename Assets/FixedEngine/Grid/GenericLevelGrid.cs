// File: GenericLevelGrid.cs
using FixedEngine;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Fournit une grille de collision flexible pour divers types de jeux 2D basés sur Tilemap.
/// Supporte :
/// - Plusieurs Tilemaps de collision
/// - Overrides dynamiques (obstacles mobiles)
/// - Checkers personnalisés (delegate Func<int,int,bool>)
/// - Conversion World <-> Grid
/// </summary>
[DisallowMultipleComponent]
public class GenericLevelGrid : MonoBehaviour
{
    [Tooltip("Liste des Tilemaps pour collisions. Les tiles non-marquetables rendent la case non walkable.")]
    public Tilemap[] collisionTilemaps;

    // Delegate pour tester la walkabilité custom
    private readonly List<Func<int, int, bool>> customCheckers = new List<Func<int, int, bool>>();

    // Overrides explicites pour cases dynamiques
    private readonly Dictionary<Vector2Int, bool> overrides = new Dictionary<Vector2Int, bool>();

    // Grille statique générée à partir des tilemaps
    private bool[,] baseGrid;
    private int xMin, yMin, width, height;

    private void Awake()
    {
        if (collisionTilemaps == null || collisionTilemaps.Length == 0)
        {
            collisionTilemaps = GetComponents<Tilemap>();
        }

        // Calcule les bornes communes manuellement
        var b0 = collisionTilemaps[0].cellBounds;
        int xMinTmp = b0.xMin, xMaxTmp = b0.xMax;
        int yMinTmp = b0.yMin, yMaxTmp = b0.yMax;
        for (int i = 1; i < collisionTilemaps.Length; i++)
        {
            var b = collisionTilemaps[i].cellBounds;
            xMinTmp = Math.Min(xMinTmp, b.xMin);
            yMinTmp = Math.Min(yMinTmp, b.yMin);
            xMaxTmp = Math.Max(xMaxTmp, b.xMax);
            yMaxTmp = Math.Max(yMaxTmp, b.yMax);
        }

        xMin = xMinTmp;
        yMin = yMinTmp;
        width = xMaxTmp - xMinTmp;
        height = yMaxTmp - yMinTmp;

        baseGrid = new bool[width, height];

        // Remplit la grille statique (plus performant avec for)
        for (int ti = 0; ti < collisionTilemaps.Length; ti++)
        {
            var tm = collisionTilemaps[ti];
            var bounds = tm.cellBounds;
            for (int cx = bounds.xMin; cx < bounds.xMax; cx++)
            {
                for (int cy = bounds.yMin; cy < bounds.yMax; cy++)
                {
                    Vector3Int cell = new Vector3Int(cx, cy, bounds.zMin);
                    var tile = tm.GetTile<WalkableTile>(cell);
                    if (tile != null)
                    {
                        baseGrid[cx - xMin, cy - yMin] = true;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Ajoute un override pour une case spécifique (utile pour obstacles mobiles).
    /// </summary>
    public void SetOverride(int gridX, int gridY, bool isWalkable)
    {
        overrides[new Vector2Int(gridX, gridY)] = isWalkable;
    }

    /// <summary>
    /// Supprime l'override pour une case.
    /// </summary>
    public void RemoveOverride(int gridX, int gridY)
    {
        overrides.Remove(new Vector2Int(gridX, gridY));
    }

    /// <summary>
    /// Enregistre un checker personnalisé (ex. zone spéciale, autre logique).
    /// Doit renvoyer false pour rendre la case non walkable.
    /// </summary>
    public void AddCustomChecker(Func<int, int, bool> checker)
    {
        if (checker != null)
            customCheckers.Add(checker);
    }

    /// <summary>
    /// Vérifie si une case (gridX, gridY) est walkable en tenant compte :
    /// 1) Overrides dynamiques
    /// 2) Checkers personnalisés
    /// 3) Grille statique de tilemaps
    /// </summary>
    public bool IsWalkable(int gridX, int gridY)
    {
        var key = new Vector2Int(gridX, gridY);
        if (overrides.TryGetValue(key, out bool ov))
        {
            return ov;
        }

        foreach (var chk in customCheckers)
        {
            if (!chk(gridX, gridY))
            {
                return false;
            }
        }

        int lx = gridX - xMin;
        int ly = gridY - yMin;
        if (lx < 0 || lx >= width || ly < 0 || ly >= height)
        {
            return false;
        }

        return baseGrid[lx, ly];
    }

    /// <summary>
    /// Overload : de la position world à la grille
    /// </summary>
    public bool IsWalkable(Vector3 worldPos)
    {
        Vector3Int cell = collisionTilemaps[0].WorldToCell(worldPos);
        return IsWalkable(cell.x, cell.y);
    }

    /// <summary>
    /// Overload générique : position FixedVector2 en arithmétique fixe
    /// </summary>
    public bool IsWalkable<TFormat>(FixedVector2<TFormat> pos)
        where TFormat : struct, IFixedPointFormat
    {
        int shift = FixedFormatUtil<TFormat>.FractionBits;
        int gx = pos.x.Raw >> shift;
        int gy = pos.y.Raw >> shift;
        return IsWalkable(gx, gy);
    }

    /// <summary>
    /// Dimensions de la grille (en cases)
    /// </summary>
    public Vector2Int GridSize => new Vector2Int(width, height);

    /// <summary>
    /// Offset (coordonnées du coin bas-gauche)
    /// </summary>
    public Vector2Int GridOffset => new Vector2Int(xMin, yMin);
}
