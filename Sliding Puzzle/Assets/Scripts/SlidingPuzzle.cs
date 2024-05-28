using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HutongGames.PlayMaker;

public class SlidingPuzzle : MonoBehaviour
{
    public int M = 3; // Number of rows
    public int N = 3; // Number of columns
    public float K = 2.0f; // Seconds to wait before shuffling
    public Sprite puzzleImage; // Image to use for the puzzle
    public GameObject cellPrefab; // Prefab for the individual game cells
    public PlayMakerFSM fsm; // Reference to the PlayMakerFSM component
    public float space = 0.1f; // Space between cells

    private GameObject[,] gameCells;
    private Vector2[,] initialPositions;
    private Vector2[,] currentPositions;
    private GameObject draggingCell = null;
    private Vector2 draggingCellStartPos;
    private Vector2 mouseStartPos;
    private BoxCollider2D boxCollider;

    void Start()
    {
        if (M < 3 || N < 3)
        {
            Debug.LogError("M and N must be at least 3");
            return;
        }

        boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider == null)
        {
            Debug.LogError("GameBoard must have a BoxCollider2D component.");
            return;
        }

        InitializeGameBoard();
        StartCoroutine(ShuffleBoardAfterDelay(K));
    }

    void InitializeGameBoard()
    {
        gameCells = new GameObject[M, N];
        initialPositions = new Vector2[M, N];
        currentPositions = new Vector2[M, N];
        Texture2D texture = puzzleImage.texture;

        // Calculate the size of each cell based on the BoxCollider2D size and the space
        float boardWidth = boxCollider.bounds.size.x;
        float boardHeight = boxCollider.bounds.size.y;
        float cellWidth = (boardWidth - space * (M - 1)) / M;
        float cellHeight = (boardHeight - space * (N - 1)) / N;

        Vector2 cellSize = new Vector2(cellWidth, cellHeight);

        float startX = boxCollider.bounds.min.x + cellWidth / 2;
        float startY = boxCollider.bounds.max.y - cellHeight / 2;

        for (int i = 0; i < M; i++)
        {
            for (int j = 0; j < N; j++)
            {
                GameObject newCell = Instantiate(cellPrefab, transform);
                newCell.name = $"Game Cell_{i * N + j + 1}";
                gameCells[i, j] = newCell;

                // Set the sprite for the cell
                Rect spriteRect = new Rect(i * (texture.width / M), j * (texture.height / N), texture.width / M, texture.height / N);
                Sprite cellSprite = Sprite.Create(texture, spriteRect, new Vector2(0.5f, 0.5f));
                newCell.GetComponent<SpriteRenderer>().sprite = cellSprite;

                // Scale the cell to fit within the calculated size
                newCell.transform.localScale = new Vector3(cellWidth / cellSprite.bounds.size.x, cellHeight / cellSprite.bounds.size.y, 1);

                // Position the cell
                Vector2 cellPos = new Vector2(startX + i * (cellWidth + space), -1*(startY - j * (cellHeight + space)));
                newCell.transform.position = cellPos;
                initialPositions[i, j] = cellPos;
                currentPositions[i, j] = cellPos;
            }
        }
    }

    IEnumerator ShuffleBoardAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        List<GameObject> cells = new List<GameObject>(gameCells.Length);
        foreach (var cell in gameCells)
        {
            cells.Add(cell);
        }

        for (int i = 0; i < M; i++)
        {
            for (int j = 0; j < N; j++)
            {
                int randomIndex = Random.Range(0, cells.Count);
                gameCells[i, j] = cells[randomIndex];
                gameCells[i, j].transform.position = initialPositions[i, j];
                currentPositions[i, j] = initialPositions[i, j];
                cells.RemoveAt(randomIndex);
            }
        }

        // Send the shuffle complete event to Playmaker
        fsm.SendEvent("ShuffleComplete");
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            OnMouseDown();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            OnMouseUp();
        }

        if (draggingCell != null)
        {
            OnMouseDrag();
        }
    }

    void OnMouseDown()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hit = Physics2D.OverlapPoint(mousePos);

        if (hit != null && hit.transform.parent == transform)
        {
            draggingCell = hit.gameObject;
            draggingCellStartPos = draggingCell.transform.position;
            mouseStartPos = mousePos;
        }
    }

    void OnMouseDrag()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 delta = mousePos - mouseStartPos;
        draggingCell.transform.position = draggingCellStartPos + delta;

        // Check for swap
        for (int i = 0; i < M; i++)
        {
            for (int j = 0; j < N; j++)
            {
                if (gameCells[i, j] == draggingCell)
                    continue;

                Vector2 cellPos = currentPositions[i, j];
                if (Vector2.Distance(draggingCell.transform.position, cellPos) < (cellPrefab.GetComponent<SpriteRenderer>().bounds.size.x / 2))
                {
                    SwapCells(draggingCell, gameCells[i, j]);
                    return;
                }
            }
        }
    }

    void OnMouseUp()
    {
        if (draggingCell != null)
        {
            draggingCell.transform.position = draggingCellStartPos;
            draggingCell = null;

            // Check for stage clear
            if (IsStageClear())
            {
                fsm.SendEvent("StageClear");
            }
        }
    }

    void SwapCells(GameObject cellA, GameObject cellB)
    {
        Vector2 tempPos = cellA.transform.position;
        cellA.transform.position = cellB.transform.position;
        cellB.transform.position = tempPos;

        // Update current positions
        for (int i = 0; i < M; i++)
        {
            for (int j = 0; j < N; j++)
            {
                if (gameCells[i, j] == cellA)
                {
                    currentPositions[i, j] = cellA.transform.position;
                }
                else if (gameCells[i, j] == cellB)
                {
                    currentPositions[i, j] = cellB.transform.position;
                }
            }
        }
    }

    bool IsStageClear()
    {
        for (int i = 0; i < M; i++)
        {
            for (int j = 0; j < N; j++)
            {
                if (currentPositions[i, j] != initialPositions[i, j])
                {
                    return false;
                }
            }
        }
        return true;
    }
}
