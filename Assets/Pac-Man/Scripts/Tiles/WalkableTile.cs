using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "New Walkable Tile", menuName = "PacMan/Walkable Tile")]
public class WalkableTile : Tile
{
    [Tooltip("Définit la nature logique de la tuile pour la grille de jeu.")]
    public TileType tileType = TileType.Empty;

    [Tooltip("Identifiant libre pour debug ou scripts (ex: GATE_LEFT, SPAWN_POINT, etc.)")]
    public string id = "";
}
