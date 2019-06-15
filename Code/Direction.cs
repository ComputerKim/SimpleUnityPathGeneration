using UnityEngine;

public static class Direction
{
    public static Vector3Int Up = new Vector3Int(0, 1, 0);
    public static Vector3Int Down = new Vector3Int(0, -1, 0);
    public static Vector3Int Left = new Vector3Int(-1, 0, 0);
    public static Vector3Int Right = new Vector3Int(1, 0, 0);

    public static Vector3Int[] All4Directions { get { return new Vector3Int[] { Up, Down, Left, Right }; } }
}