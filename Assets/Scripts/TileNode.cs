using UnityEngine;

public class TileNode : MonoBehaviour
{
    [Header("Grid Info")]
    public int x;
    public int y;

    [Header("Tile Settings")]
    public int moveCost = 1;
    public bool isBlocked = false;

    [Header("Optional Visuals")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color blockedColor = Color.black;
    [SerializeField] private Color pathColor = Color.green;
    [SerializeField] private Color startColor = Color.blue;
    [SerializeField] private Color goalColor = Color.red;

    public void Setup(int gridX, int gridY)
    {
        x = gridX;
        y = gridY;
        UpdateVisual();
    }

    public void UpdateVisual()
    {
        if (spriteRenderer == null) return;

        spriteRenderer.color = isBlocked ? blockedColor : normalColor;
    }

    public void SetPathVisual()
    {
        if (spriteRenderer != null && !isBlocked)
            spriteRenderer.color = pathColor;
    }

    public void SetStartVisual()
    {
        if (spriteRenderer != null)
            spriteRenderer.color = startColor;
    }

    public void SetGoalVisual()
    {
        if (spriteRenderer != null)
            spriteRenderer.color = goalColor;
    }

    public void ResetToBaseVisual()
    {
        UpdateVisual();
    }
}
