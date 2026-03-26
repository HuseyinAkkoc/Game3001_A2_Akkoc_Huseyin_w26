public class PathNode
{
    public TileNode tile;
    public PathNode parent;

    public int gCost;
    public int hCost;
    public int fCost => gCost + hCost;

    public PathNode(TileNode tile)
    {
        this.tile = tile;
    }
}