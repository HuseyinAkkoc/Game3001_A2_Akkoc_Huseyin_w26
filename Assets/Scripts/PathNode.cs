using UnityEngine;

public class PathNode
{
    public TileNode tile;
    public PathNode parent;

    public int gCost; // cost from start
    public int hCost; // heuristic to goal
    public int fCost => gCost + hCost;

    public PathNode(TileNode tile)
    {
        this.tile = tile;
    }
}
