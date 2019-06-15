using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Level : MonoBehaviour
{
    // Change this to false to hide animation
    public bool visualize = true;
    [Range(0, .2f)]
    public float delayAfterPaint = 0;
    [Range(.1f, 10)]
    public float delayBetweenGenerations = .5f;

    int width, height;

    public Tilemap tilemap;
    public Tile grassTile;
    public Tile roadTile;

    uint pathLoops = 0;
    uint pathMaxLoops = 10000000;

    bool pathResult; // Only used for the visual version

    List<Vector3Int> path = new List<Vector3Int>();
    List<List<Vector3Int>> pathAvailableDirections = new List<List<Vector3Int>>();

    // HashSets are much faster to check than Lists
    HashSet<Vector3Int> pathPoints = new HashSet<Vector3Int>();
    HashSet<Vector3Int> pointsChecked = new HashSet<Vector3Int>();

    private void Start()
    {
        StartCoroutine(StartInfiniteGenerate());
    }
    
    IEnumerator StartInfiniteGenerate()
    {
        // Keep regenerating levels
        while (true)
        {
            yield return Generate(Random.Range(int.MinValue, int.MaxValue));
            yield return new WaitForSeconds(delayBetweenGenerations);
        }
    }

    void AddPath(Vector3Int point)
    {
        path.Add(point);
        pathPoints.Add(point);
        pathAvailableDirections.Add(new List<Vector3Int>(Direction.All4Directions));
    }

    void RemovePath(int index)
    {
        path.RemoveAt(index);
        pathAvailableDirections.RemoveAt(index);
    }

    void PaintTile(Vector3Int point, Tile tile)
    {
        // Random tile rotation (Anyone have a better solution?)
        tile.transform = Matrix4x4.Rotate(Quaternion.Euler(0, 0, Random.Range(0, 4) * 90));

        // Paint tile
        tilemap.SetTile(point, tile);
    }

    public IEnumerator Generate(int seed)
    {
        // Reset
        tilemap.ClearAllTiles();
        path.Clear();
        pathLoops = 0;

        // Set random seed
        Random.InitState(seed);

        // Level height and width
        width = 25; // Random.Range(5, 25);
        height = 20; // Random.Range(5, 20);

        // Fill tilemap with grass
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                PaintTile(new Vector3Int(x, y, 0), grassTile);
            }
        }

        // Select a random starting point
        AddPath(new Vector3Int(Random.Range(1, width - 2), height - 1, 0));

        // Generate a path
        if (visualize)
            PaintTile(path[0], roadTile);

        var startTime = Time.realtimeSinceStartup;

        // Loop until we reach the goal (the bottom)
        bool goalReached = false;
        while (!goalReached)
        {
            var index = path.Count - 1;
            var currentPoint = path[index];
            var availableDirections = pathAvailableDirections[index];

            pathLoops++;

            // Loop until there are no more directions to check on the current point
            while (availableDirections.Count > 0)
            {
                // Get random direction
                var randomDirection = availableDirections[Random.Range(0, availableDirections.Count)];

                // Remove direction from the path's available directions
                availableDirections.Remove(randomDirection);

                // Get the new point
                var newPoint = currentPoint + randomDirection;

                // If max loops reached, then go down until we reach the end
                if (pathLoops >= pathMaxLoops)
                {
                    availableDirections.Clear();
                    newPoint = currentPoint + Direction.Down;
                }

                // Check if we reached our goal
                if (newPoint.y == 0)
                {
                    path.Add(newPoint);
                    goalReached = true;

                    if (visualize) PaintTile(newPoint, roadTile);

                    break;
                }

                // Check if outside allowed width
                if (newPoint.x != Mathf.Clamp(newPoint.x, 1, width - 2)) continue;

                // Check if outside allowed height
                if (newPoint.y != Mathf.Clamp(newPoint.y, 1, height - 2)) continue;

                // If checked before
                if (pointsChecked.Contains(newPoint)) continue;
                pointsChecked.Add(newPoint);

                // If there are more than 2 neighbors, then the path cannot be used.
                var neighborCount = 0;
                if (pathPoints.Contains(newPoint + Direction.Up)) neighborCount++;
                if (pathPoints.Contains(newPoint + Direction.Down)) neighborCount++;
                if (pathPoints.Contains(newPoint + Direction.Left)) neighborCount++;
                if (pathPoints.Contains(newPoint + Direction.Right)) neighborCount++;
                if (neighborCount > 1) continue; // 1 is where we came from

                // Prevent trapping itself (Cannot go up if path is too narrow to turn)
                if (randomDirection.y == 1 && (newPoint.x < 3 || newPoint.x > width - 4)) continue;

                // Check if the new point has not been used before
                if (pathPoints.Contains(newPoint)) continue;

                // We could use the point, break to the next point
                AddPath(newPoint);
                if (visualize)
                {
                    PaintTile(newPoint, roadTile);
                    yield return new WaitForSeconds(delayAfterPaint);
                }

                break;
            }

            // If another path point was not added, then this point could not be used
            if (index == path.Count - 1)
            {
                RemovePath(index);

                if (visualize)
                {
                    PaintTile(currentPoint, grassTile);
                    yield return new WaitForSeconds(delayAfterPaint);
                }
            }
        }

        Debug.Log("Path generation took " + ((Time.realtimeSinceStartup - startTime) * 1000) + "ms");

        // Paint road
        if (!visualize)
            foreach (var p in path)
                PaintTile(p, roadTile);

        // Cleanup
        pathPoints.Clear();
        pointsChecked.Clear();
        pathAvailableDirections.Clear();

        yield break;
    }

    
}