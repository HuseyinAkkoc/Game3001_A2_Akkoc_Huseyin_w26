using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class GridPathfindingManager : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField] private int rows = 10;
    [SerializeField] private int cols = 10;
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private Transform gridParent;

    [Header("Visual Prefabs")]
    [SerializeField] private GameObject obstaclePrefab;
    [SerializeField] private GameObject routePrefab;

    [Header("Actor / Start / Goal")]
    [SerializeField] private ActorMover actor;
    [SerializeField] private Vector2Int startCoords = new Vector2Int(0, 0);
    [SerializeField] private Vector2Int goalCoords = new Vector2Int(9, 9);

    [Header("UI")]
    [SerializeField] private TMP_Text instructionText;
    [SerializeField] private TMP_Text totalCostText;

    [Header("Map Obstacles")]
    [SerializeField] private List<Vector2Int> blockedTiles = new List<Vector2Int>();

    [Header("Special Tile Costs")]
    [SerializeField] private List<TileCostData> customTileCosts = new List<TileCostData>();

    private TileNode[,] grid;
    private List<TileNode> currentPath = new List<TileNode>();

    private List<GameObject> spawnedObstacles = new List<GameObject>();
    private List<GameObject> spawnedRoutes = new List<GameObject>();

    [System.Serializable]
    public class TileCostData
    {
        public Vector2Int coords;
        public int moveCost = 1;
    }

    private void Start()
    {
        BuildGrid();
        ApplyMapData();
        SpawnObstacleVisuals();
        UpdateSceneVisuals();

        if (instructionText != null)
        {
            instructionText.text =
                "Left Click = Set Start\n" +
                "Right Click = Set Goal\n" +
                "F = Find Shortest Path\n" +
                "R = Reset Scene";
        }

        if (actor != null)
        {
            actor.transform.position = grid[startCoords.x, startCoords.y].transform.position;
            actor.transform.rotation = Quaternion.identity;
        }

        UpdateCostText(0);
    }

    private void Update()
    {
        HandleMouseSelection();

        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            if (actor != null && !actor.IsMoving)
            {
                FindAndMove();
            }
        }

        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            ResetScene();
        }
    }

    private void HandleMouseSelection()
    {
        if (actor != null && actor.IsMoving)
            return;

        if (Mouse.current == null)
            return;

        bool leftClicked = Mouse.current.leftButton.wasPressedThisFrame;
        bool rightClicked = Mouse.current.rightButton.wasPressedThisFrame;

        if (!leftClicked && !rightClicked)
            return;

        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        Vector2 mousePos2D = new Vector2(mouseWorldPos.x, mouseWorldPos.y);

        RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

        if (!hit.collider)
            return;

        TileNode clickedTile = hit.collider.GetComponent<TileNode>();

        if (clickedTile == null)
            return;

        if (clickedTile.isBlocked)
            return;

        Vector2Int clickedCoords = new Vector2Int(clickedTile.x, clickedTile.y);

        if (leftClicked)
        {
            if (clickedCoords != goalCoords)
            {
                startCoords = clickedCoords;

                if (actor != null)
                {
                    actor.transform.position = clickedTile.transform.position;
                    actor.transform.rotation = Quaternion.identity;
                    actor.ForceStopMovement();
                }

                ClearPathOnly();
                UpdateSceneVisuals();
                UpdateCostText(0);
            }
        }

        if (rightClicked)
        {
            if (clickedCoords != startCoords)
            {
                goalCoords = clickedCoords;

                ClearPathOnly();
                UpdateSceneVisuals();
                UpdateCostText(0);
            }
        }
    }

    private void BuildGrid()
    {
        grid = new TileNode[rows, cols];

        float totalWidth = cols * cellSize;
        float totalHeight = rows * cellSize;

        for (int x = 0; x < rows; x++)
        {
            for (int y = 0; y < cols; y++)
            {
                Vector3 worldPos = new Vector3(
                    y * cellSize - totalWidth / 2f + cellSize / 2f,
                    x * cellSize - totalHeight / 2f + cellSize / 2f,
                    0f
                );

                GameObject tileObj = Instantiate(tilePrefab, worldPos, Quaternion.identity, gridParent);
                TileNode tile = tileObj.GetComponent<TileNode>();

                if (tile == null)
                {
                    Debug.LogError("Tile prefab must have TileNode attached.");
                    return;
                }

                tile.Setup(x, y);
                grid[x, y] = tile;
            }
        }
    }

    private void ApplyMapData()
    {
        for (int i = 0; i < blockedTiles.Count; i++)
        {
            Vector2Int pos = blockedTiles[i];
            if (IsInBounds(pos.x, pos.y))
            {
                grid[pos.x, pos.y].isBlocked = true;
            }
        }

        for (int i = 0; i < customTileCosts.Count; i++)
        {
            TileCostData data = customTileCosts[i];
            if (IsInBounds(data.coords.x, data.coords.y))
            {
                grid[data.coords.x, data.coords.y].moveCost = Mathf.Max(1, data.moveCost);
            }
        }

        grid[startCoords.x, startCoords.y].isBlocked = false;
        grid[goalCoords.x, goalCoords.y].isBlocked = false;
    }

    private void SpawnObstacleVisuals()
    {
        ClearObstacleVisuals();

        if (obstaclePrefab == null)
            return;

        for (int x = 0; x < rows; x++)
        {
            for (int y = 0; y < cols; y++)
            {
                if (grid[x, y].isBlocked)
                {
                    GameObject obstacle = Instantiate(
                        obstaclePrefab,
                        grid[x, y].transform.position,
                        Quaternion.identity,
                        grid[x, y].transform
                    );

                    spawnedObstacles.Add(obstacle);
                }
            }
        }
    }

    private void ClearObstacleVisuals()
    {
        for (int i = 0; i < spawnedObstacles.Count; i++)
        {
            if (spawnedObstacles[i] != null)
            {
                Destroy(spawnedObstacles[i]);
            }
        }

        spawnedObstacles.Clear();
    }

    private void SpawnRouteVisuals(List<TileNode> path)
    {
        ClearRouteVisuals();

        if (routePrefab == null || path == null || path.Count == 0)
            return;

        TileNode startTile = grid[startCoords.x, startCoords.y];
        TileNode goalTile = grid[goalCoords.x, goalCoords.y];

        for (int i = 0; i < path.Count; i++)
        {
            TileNode tile = path[i];

            if (tile == startTile || tile == goalTile)
                continue;

            GameObject route = Instantiate(
                routePrefab,
                tile.transform.position,
                Quaternion.identity,
                tile.transform
            );

            spawnedRoutes.Add(route);
        }
    }

    private void ClearRouteVisuals()
    {
        for (int i = 0; i < spawnedRoutes.Count; i++)
        {
            if (spawnedRoutes[i] != null)
            {
                Destroy(spawnedRoutes[i]);
            }
        }

        spawnedRoutes.Clear();
    }

    private void UpdateSceneVisuals()
    {
        for (int x = 0; x < rows; x++)
        {
            for (int y = 0; y < cols; y++)
            {
                grid[x, y].ResetToBaseVisual();
            }
        }

        grid[startCoords.x, startCoords.y].SetStartVisual();
        grid[goalCoords.x, goalCoords.y].SetGoalVisual();
    }

    private void FindAndMove()
    {
        UpdateSceneVisuals();

        TileNode startTile = grid[startCoords.x, startCoords.y];
        TileNode goalTile = grid[goalCoords.x, goalCoords.y];

        currentPath = FindPathAStar(startTile, goalTile);

        if (currentPath == null || currentPath.Count == 0)
        {
            Debug.Log("No path found.");
            ClearRouteVisuals();
            UpdateCostText(0);
            return;
        }

        int totalCost = 0;

        for (int i = 0; i < currentPath.Count; i++)
        {
            if (currentPath[i] != startTile)
            {
                totalCost += currentPath[i].moveCost;
            }
        }

        SpawnRouteVisuals(currentPath);

        startTile.SetStartVisual();
        goalTile.SetGoalVisual();

        UpdateCostText(totalCost);

        StartCoroutine(MoveActorAlongPath(currentPath));
    }

    private IEnumerator MoveActorAlongPath(List<TileNode> path)
    {
        List<TileNode> movementPath = new List<TileNode>(path);

        if (movementPath.Count > 0)
        {
            movementPath.RemoveAt(0);
        }

        yield return StartCoroutine(actor.FollowPath(movementPath));
    }

    private List<TileNode> FindPathAStar(TileNode startTile, TileNode goalTile)
    {
        List<PathNode> openList = new List<PathNode>();
        HashSet<TileNode> closedSet = new HashSet<TileNode>();
        Dictionary<TileNode, PathNode> allNodes = new Dictionary<TileNode, PathNode>();

        PathNode startNode = new PathNode(startTile);
        startNode.gCost = 0;
        startNode.hCost = GetHeuristic(startTile, goalTile);

        openList.Add(startNode);
        allNodes[startTile] = startNode;

        while (openList.Count > 0)
        {
            PathNode currentNode = openList[0];

            for (int i = 1; i < openList.Count; i++)
            {
                if (openList[i].fCost < currentNode.fCost ||
                    (openList[i].fCost == currentNode.fCost && openList[i].hCost < currentNode.hCost))
                {
                    currentNode = openList[i];
                }
            }

            openList.Remove(currentNode);
            closedSet.Add(currentNode.tile);

            if (currentNode.tile == goalTile)
            {
                return ReconstructPath(currentNode);
            }

            List<TileNode> neighbors = GetNeighbors(currentNode.tile);

            for (int i = 0; i < neighbors.Count; i++)
            {
                TileNode neighbor = neighbors[i];

                if (neighbor.isBlocked || closedSet.Contains(neighbor))
                    continue;

                int tentativeGCost = currentNode.gCost + neighbor.moveCost;

                if (!allNodes.ContainsKey(neighbor))
                {
                    PathNode neighborNode = new PathNode(neighbor);
                    neighborNode.gCost = tentativeGCost;
                    neighborNode.hCost = GetHeuristic(neighbor, goalTile);
                    neighborNode.parent = currentNode;

                    allNodes[neighbor] = neighborNode;
                    openList.Add(neighborNode);
                }
                else
                {
                    PathNode existingNode = allNodes[neighbor];

                    if (tentativeGCost < existingNode.gCost)
                    {
                        existingNode.gCost = tentativeGCost;
                        existingNode.parent = currentNode;

                        if (!openList.Contains(existingNode))
                        {
                            openList.Add(existingNode);
                        }
                    }
                }
            }
        }

        return null;
    }

    private List<TileNode> ReconstructPath(PathNode endNode)
    {
        List<TileNode> path = new List<TileNode>();
        PathNode current = endNode;

        while (current != null)
        {
            path.Add(current.tile);
            current = current.parent;
        }

        path.Reverse();
        return path;
    }

    private int GetHeuristic(TileNode a, TileNode b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private List<TileNode> GetNeighbors(TileNode tile)
    {
        List<TileNode> neighbors = new List<TileNode>();

        int x = tile.x;
        int y = tile.y;

        if (IsInBounds(x - 1, y)) neighbors.Add(grid[x - 1, y]);
        if (IsInBounds(x + 1, y)) neighbors.Add(grid[x + 1, y]);
        if (IsInBounds(x, y - 1)) neighbors.Add(grid[x, y - 1]);
        if (IsInBounds(x, y + 1)) neighbors.Add(grid[x, y + 1]);

        return neighbors;
    }

    private bool IsInBounds(int x, int y)
    {
        return x >= 0 && x < rows && y >= 0 && y < cols;
    }

    private void ResetScene()
    {
        StopAllCoroutines();

        if (actor != null)
        {
            actor.ForceStopMovement();
        }

        ClearPathOnly();
        UpdateSceneVisuals();

        if (actor != null)
        {
            actor.transform.position = grid[startCoords.x, startCoords.y].transform.position;
            actor.transform.rotation = Quaternion.identity;
        }

        UpdateCostText(0);
    }

    private void ClearPathOnly()
    {
        currentPath.Clear();
        ClearRouteVisuals();

        for (int x = 0; x < rows; x++)
        {
            for (int y = 0; y < cols; y++)
            {
                grid[x, y].ResetToBaseVisual();
            }
        }
    }

    private void UpdateCostText(int totalCost)
    {
        if (totalCostText != null)
        {
            totalCostText.text = "Total Path Cost: " + totalCost;
        }
    }
}