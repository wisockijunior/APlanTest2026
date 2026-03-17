using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Grid Settings")]
    public int Width = 5;
    public int Height = 6;
    
    public float CellSizeHorizontal = 1.0f;
    public float CellSizeVertical = 1.0f;
    public GameObject BlockPrefab;
    public Sprite[] BlockSprites;

    private Block[,] grid;
    private Transform gridParent;

    private void Awake()
    {
        Instance = this;
        SetupGridParent();
    }

    private void OnValidate()
    {
        // Don't regenerate if we're playing, it might mess up game state
        if (Application.isPlaying) return;

#if UNITY_EDITOR
        // Wait one frame to avoid internal Unity warnings during OnValidate
        UnityEditor.EditorApplication.delayCall += () => {

            if (this == null) return;
            SetupGridParent();
            GenerateGrid();
            
        };
#endif
    }

    private void SetupGridParent()
    {
        if (gridParent == null)
        {
            gridParent = transform.Find("GridParent");
            if (gridParent == null)
            {
                gridParent = new GameObject("GridParent").transform;
                gridParent.SetParent(transform);
            }
        }
    }

    private void Start()
    {
        if (!Application.isPlaying) return;

        if (GameManager.Instance != null)
            GameManager.Instance.OnGameRestarted += GenerateGrid;
            
        GenerateGrid();
    }

    public void GenerateGrid()
    {
        if (BlockPrefab == null || BlockSprites == null || BlockSprites.Length == 0) return;

        ClearGrid();
        grid = new Block[Width, Height];

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                SpawnBlock(x, y);
            }
        }
        
    }

    private void ClearGrid()
    {
        if (gridParent != null)
        {
            // Destroy all children of gridParent
            List<GameObject> children = new List<GameObject>();
            foreach (Transform child in gridParent) children.Add(child.gameObject);
            
            foreach (var child in children)
            {
                if (Application.isPlaying) Destroy(child);
                else DestroyImmediate(child);
            }
        }
        grid = null;
    }

    private void SpawnBlock(int x, int y)
    {
        int spriteIndex = Random.Range(0, BlockSprites.Length);
        Vector3 newpos = GetWorldPosition(x, y);
        GameObject go = Instantiate(BlockPrefab, newpos, Quaternion.identity, gridParent);
        go.transform.localScale = Vector3.one;
        Block block = go.GetComponent<Block>();
        block.Initialize(x, y, spriteIndex, BlockSprites[spriteIndex]);
        
        if (grid != null && x < Width && y < Height)
            grid[x, y] = block;
    }

    private Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x * CellSizeHorizontal, y * CellSizeVertical, 0);
    }

    public void HandleBlockTap(Block tappedBlock)
    {
        if (!GameManager.Instance.IsPlaying) return;
        if (waiting) return; // ignore user clicks white waiting for Refill

        List<Block> connected = GetConnectedBlocks(tappedBlock);

        // according to documentation: 
        // Points are awarded based on the number of blocks collected (+1 for 1 block, +2 for 2, etc.)
        int score = 0;//connected.Count

        // remove connected blocks, safe count for score
        foreach (var block in connected)
        {
            grid[block.X, block.Y] = null;
            Destroy(block.gameObject);
            score++;
        }
        
        GameManager.Instance.AddScore(score);
        GameManager.Instance.DoMakeMove();

        // apply gravity immediate, wait 1s for refill
        ApplyGravity();

        // 1s delay --> RefillGrid
        //RefillGrid();
        StartCoroutine(WaitAndRefill());
    }
    
    private bool waiting;
    IEnumerator WaitAndRefill()
    {
        waiting = true;
        // wait 1 second 
        yield return new WaitForSeconds(1f);

        // finally - refill the grid
        RefillGrid();
        waiting = false;
    }

    // easier to implement - Breadth-First Search (BFS)
    private List<Block> GetConnectedBlocks(Block start)
    {
        List<Block> result = new List<Block>();
        Queue<Block> queue = new Queue<Block>();
        HashSet<Block> visited = new HashSet<Block>();

        queue.Enqueue(start);
        visited.Add(start);

        int targetColor = start.ColorType;

        while (queue.Count > 0)
        {
            Block current = queue.Dequeue();
            result.Add(current);

            // Neighbors: left, right, up, down
            int[,] dirs = { { -1, 0 }, { 1, 0 }, { 0, -1 }, { 0, 1 } };
            for (int i = 0; i < 4; i++)
            {
                int nx = current.X + dirs[i, 0];
                int ny = current.Y + dirs[i, 1];

                // valid range check
                if (nx >= 0 && nx < Width && ny >= 0 && ny < Height)
                {
                    Block neighbor = grid[nx, ny];
                    if (neighbor == null)
                        continue;

                    // same color?
                    if (neighbor.ColorType == targetColor)
                    {
                        // dont count twice
                        bool dontCountTwice = !visited.Contains(neighbor);
                        if (dontCountTwice)
                        {
                            visited.Add(neighbor);
                            queue.Enqueue(neighbor);
                        }
                    }
                    
                }
            }
        }

        return result;
    }

    private void ApplyGravity()
    {
        for (int x = 0; x < Width; x++)
        {
            int emptyY = -1;
            for (int y = 0; y < Height; y++)
            {
                if (grid[x, y] == null)
                {
                    if (emptyY == -1) emptyY = y;
                }
                else if (emptyY != -1)
                {
                    // Move block down to empty spot
                    Block block = grid[x, y];
                    grid[x, emptyY] = block;
                    grid[x, y] = null;
                    block.SetGridPosition(x, emptyY);
                    block.MoveToPosition(GetWorldPosition(x, emptyY));
                    
                    // After moving, find the next empty spot
                    y = emptyY;
                    emptyY = -1;
                }
            }
        }
    }

    private void RefillGrid()
    {
        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
            {
                if (grid[x, y] == null)
                    SpawnBlock(x, y);
            }
    }
}
