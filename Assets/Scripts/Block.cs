using UnityEngine;

public class Block : MonoBehaviour
{
    public int X { get; private set; }
    public int Y { get; private set; }
    public int ColorType { get; private set; }

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(int x, int y, int colorType, Sprite sprite)
    {
        X = x;
        Y = y;
        ColorType = colorType;
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.color = Color.white; // Ensure color is white to show sprite correctly
        name = $"Block_{x}_{y}_{colorType}";
    }

    public void SetGridPosition(int x, int y)
    {
        X = x;
        Y = y;
    }

    public void MoveToPosition(Vector3 worldPos)
    {
        // Simple teleport for now, could be animated later
        transform.position = worldPos;
    }

    private void OnMouseDown()
    {
        if (GridManager.Instance != null)
        {
            GridManager.Instance.HandleBlockTap(this);
        }
    }


}
