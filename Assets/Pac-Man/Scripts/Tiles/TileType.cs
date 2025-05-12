public enum TileType
{
    Empty,
    Path,
    Wall,
    Warp,
    Portal, //Entrée spéciale vers la maison ou autre zone logique (téléportation ou trigger IA)
    GhostGate, //Barrière devant la maison. Cellule qui bloque Pac-Man à l'entrée du repaire
    Intersection
}
