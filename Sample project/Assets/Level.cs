using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

public class Level : MonoBehaviour
{
    // Change this to false to hide animation
    bool visualize = true;

    int width, height;

    public Tilemap tilemap;
    public Tile grassTile;
    public Tile roadTile;

    List<Vector3Int> path = new List<Vector3Int>();

    uint loops = 0, maxLoops = 1000;

    bool pathResult;

    private void Start()
    {
        StartCoroutine(StartInfiniteGenerate());
    }

    void PaintTile(Vector3Int point, Tile tile)
    {
        // Random tile rotation (Anyone have a better solution?)
        tile.transform = Matrix4x4.Rotate(Quaternion.Euler(0, 0, UnityEngine.Random.Range(0, 4) * 90));

        // Paint tile
        tilemap.SetTile(point, tile);
    }

    IEnumerator StartInfiniteGenerate()
    {
        while (true)
        {
            yield return Generate(UnityEngine.Random.Range(int.MinValue, int.MaxValue));
            yield return new WaitForSeconds(1);
        }
    }

    public IEnumerator Generate(int seed)
    {
        tilemap.ClearAllTiles();
        path.Clear();
        loops = 0;

        UnityEngine.Random.InitState(seed);

        width = UnityEngine.Random.Range(5, 25);
        height = UnityEngine.Random.Range(5, 20);

        // Fill tilemap with grass
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                PaintTile(new Vector3Int(x, y, 0), grassTile);
            }
        }

        // Select a random starting point
        path.Add(new Vector3Int(UnityEngine.Random.Range(1, width - 1), height - 1, 0));

        // Generate a path
        if (visualize)
        {
            PaintTile(path[0], roadTile);
            yield return StartCoroutine(GeneratePathVisual(path[0] + Direction.Down));
        }
        else
        {
            GeneratePath(path[0] + Direction.Down);

            // Paint road
            foreach (var p in path)
                PaintTile(p, roadTile);
        }
    }

    bool GeneratePath(Vector3Int point)
    {
        Debug.Log(point);

        // Mistake helper :)
        if (loops >= maxLoops) { throw new Exception("Path calculations exceeded. (LOOP1)"); } else loops++;

        // Check if already used
        if (path.Contains(point)) return false;

        // Check if outside allowed width
        if (point.x != Mathf.Clamp(point.x, 1, width - 2)) return false;

        // Check if we reached the goal (reach the bottom)
        if (point.y == 0)
        {
            // Add the final point to the path and return that the correct path was found
            path.Add(point);
            return true;
        }

        // Check if outside allowed height
        if (point.y != Mathf.Clamp(point.y, 1, height - 2)) return false;

        // If there are more than 2 neighbors, then the path cannot be used.
        var neighborCount = 0;
        if (path.Contains(point + Direction.Up)) neighborCount++;
        if (path.Contains(point + Direction.Down)) neighborCount++;
        if (path.Contains(point + Direction.Left)) neighborCount++;
        if (path.Contains(point + Direction.Right)) neighborCount++;
        if (neighborCount > 1) return false; // 1 is where we came from

        // We think this is the right path, so add it to the path
        path.Add(point);

        // Create a list with all directions
        var directions = new List<Vector3Int>(Direction.All4Directions);

        // Loop until there are no more directions to use
        while (directions.Count > 0)
        {
            // Mistake helper :)
            if (loops >= maxLoops) { throw new Exception("Path calculations exceeded. (LOOP2)"); } else loops++;

            // Get random direction
            var randomDirection = directions[UnityEngine.Random.Range(0, directions.Count)];

            // Get the new point
            var newPoint = point + randomDirection;

            // Prevent trapping itself (Cannot go up if path is too narrow to turn)
            if (randomDirection.y == 1 && (newPoint.x < 3 || newPoint.x > height - 4))
            {
                directions.Remove(randomDirection);
                continue;
            }

            // If the new point has not been used and we can generate a path, return that the correct path was found
            if (!path.Contains(newPoint) && GeneratePath(newPoint))
                return true;

            // Remove direction and continue to the next
            directions.Remove(randomDirection);
            continue;
        }

        // Not the correct path, remove point from the path and return that the incorrect path was found
        path.Remove(point);
        return false;
    }

    IEnumerator GeneratePathVisual(Vector3Int point)
    {
        // Mistake helper :)
        if (loops >= maxLoops) { throw new Exception("Path calculations exceeded. (LOOP1)"); } else loops++;

        // Check if already used
        if (path.Contains(point))
        {
            yield return StartCoroutine(SetPathResult(point, false));
            yield break;
        }

        // Check if outside allowed width
        if (point.x != Mathf.Clamp(point.x, 1, width - 2))
        {
            yield return StartCoroutine(SetPathResult(point, false));
            yield break;
        }

        // Check if we reached the goal (reach the bottom)
        if (point.y == 0)
        {
            // Add the final point to the path
            path.Add(point);

            // Paint tile and return
            yield return StartCoroutine(SetPathResult(point, true));
            yield break;
        }

        // Check if outside allowed height
        if (point.y != Mathf.Clamp(point.y, 1, height - 2))
        {
            yield return StartCoroutine(SetPathResult(point, false));
            yield break;
        }

        // If we collide with a neighbor, then the path cannot be used.
        var neighborCount = 0;
        if (path.Contains(point + Direction.Up)) neighborCount++;
        if (path.Contains(point + Direction.Down)) neighborCount++;
        if (path.Contains(point + Direction.Left)) neighborCount++;
        if (path.Contains(point + Direction.Right)) neighborCount++;
        if (neighborCount > 1) // (1 is where we came from)
        {
            yield return StartCoroutine(SetPathResult(point, false));
            yield break;
        }

        // We think this is the right path, so add it to the path
        yield return StartCoroutine(SetPathResult(point, true));
        path.Add(point);

        // Create a list with all directions
        var directions = new List<Vector3Int>(Direction.All4Directions);

        // Loop until there are no more directions to use
        while (directions.Count > 0)
        {
            // Mistake helper :)
            if (loops >= maxLoops) { throw new Exception("Path calculations exceeded. (LOOP2)"); } else loops++;

            // Get random direction
            var randomDirection = directions[UnityEngine.Random.Range(0, directions.Count)];

            // Get the new point
            var newPoint = point + randomDirection;

            // Prevent trapping itself (Cannot go up if path is too narrow to turn)
            if (randomDirection.y == 1 && (newPoint.x < 3 || newPoint.x > height - 4))
            {
                yield return StartCoroutine(SetPathResult(newPoint, false));
                directions.Remove(randomDirection);
                continue;
            }

            // If we can generate a path, return that the correct path was found
            yield return StartCoroutine(GeneratePathVisual(newPoint));
            if (pathResult)
            {
                //yield return StartCoroutine(SetPathResult(point, true));
                yield break;
            }

            // Remove direction and continue to the next
            directions.Remove(randomDirection);
            continue;
        }

        // Not the correct path, remove point from the path and return that the incorrect path was found
        path.Remove(point);
        yield return StartCoroutine(SetPathResult(point, false));
    }

    IEnumerator SetPathResult(Vector3Int p, bool ok)
    {
        // Paint tile
        if (ok)
        {
            PaintTile(p, roadTile);
            yield return new WaitForSeconds(.01f);
        }
        else if (!ok && !path.Contains(p))
        {
            PaintTile(p, grassTile);
            yield return new WaitForSeconds(.01f);
        }

        // Set result
        pathResult = ok;
    }
}