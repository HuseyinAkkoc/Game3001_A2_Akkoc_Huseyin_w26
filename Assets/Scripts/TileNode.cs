using UnityEngine;

public class TileNode : MonoBehaviour
{
    [Header("Grid Info")]
    public int x;
    public int y;

    [Header("Tile Settings")]
    public int moveCost = 1;
    public bool isBlocked = false;

    [Header("Base Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color startColor = Color.blue;
    [SerializeField] private Color goalColor = Color.red;

    public void Setup(int gridX, int gridY)
    {
        x = gridX;
        y = gridY;
        ResetToBaseVisual();
    }

    public void ResetToBaseVisual()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = normalColor;
        }
    }

    public void SetStartVisual()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = startColor;
        }
    }

    public void SetGoalVisual()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = goalColor;
        }
    }
}