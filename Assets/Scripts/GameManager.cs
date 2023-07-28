using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private Level _level;
    [SerializeField] private SpriteRenderer _bgPrefab;
    [SerializeField] private GamePiece _piecePrefab;
    [SerializeField] private GamePiece _winPrefab;

    private bool hasGameFinished;
    private List<GamePiece> gamePieces;
    private GamePiece winPiece;
    private GamePiece currentPiece;
    private Vector2 currentPos, previousPos;
    private List<Vector2> offsets;
    private bool[,] pieceCollision;

    private void Awake()
    {
        Instance = this;
        hasGameFinished = false;
        SpawnLevel();
    }

    private void SpawnLevel()
    {
        //Set Up BG
        SpriteRenderer bg = Instantiate(_bgPrefab);
        bg.size = new Vector2(_level.Columns, _level.Rows);
        bg.transform.position = new Vector3(_level.Columns, _level.Rows, 0) * 0.5f;

        gamePieces = new List<GamePiece>();

        //Spawn Win Piece
        winPiece = Instantiate(_winPrefab);
        Vector3 spawnPos = new Vector3(
            _level.WinPiece.Start.y + 0.5f,
            _level.WinPiece.Start.x + 0.5f,
            0);
        winPiece.transform.position = spawnPos;
        winPiece.Init(_level.WinPiece);
        gamePieces.Add(winPiece);

        //Spawn All Pieces
        foreach (var piece in _level.Pieces)
        {
            GamePiece temp = Instantiate(_piecePrefab);
            spawnPos = new Vector3(
                piece.Start.y + 0.5f,
                piece.Start.x + 0.5f, 0
                );
            temp.transform.position = spawnPos;
            temp.Init(piece);
            gamePieces.Add(temp);
        }

        //Set Up Camera
        Camera.main.orthographicSize = Mathf.Max(_level.Columns, _level.Rows) * 1.2f + 2f;
        Vector3 camPos = Camera.main.transform.position;
        camPos.x = _level.Columns * 0.5f;
        camPos.y = _level.Rows * 0.5f;
        Camera.main.transform.position = camPos;
    }

    private void Update()
    {
        if (hasGameFinished) return;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
        RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

        if (Input.GetMouseButtonDown(0))
        {
            if (!hit || !hit.collider.transform.parent.TryGetComponent(out currentPiece))
            {
                return;
            }

            currentPos = mousePos2D;
            previousPos = currentPos;

            //Calculate Collision
            pieceCollision = new bool[_level.Rows, _level.Columns];
            for (int i = 0; i < _level.Rows; i++)
            {
                for (int j = 0; j < _level.Columns; j++)
                {
                    pieceCollision[i, j] = false;
                }
            }

            foreach (var piece in gamePieces)
            {
                for (int i = 0; i < piece.Size; i++)
                {
                    pieceCollision[
                        piece.CurrentGridPos.x + (piece.IsVertical ? i : 0),
                        piece.CurrentGridPos.y + (piece.IsVertical ? 0 : i)
                        ] = true;
                }
            }

            for (int i = 0; i < currentPiece.Size; i++)
            {
                pieceCollision[
                    currentPiece.CurrentGridPos.x + (currentPiece.IsVertical ? i : 0),
                    currentPiece.CurrentGridPos.y + (currentPiece.IsVertical ? 0 : i)
                    ] = false;

            }

            offsets = new List<Vector2>();
        }

        else if (Input.GetMouseButton(0) && currentPiece != null)
        {
            currentPos = mousePos;
            Vector2 offset = currentPos - previousPos;
            offsets.Add(offset);
            bool isMovingOpposite = IsMovingOpposite();
            if (currentPiece.IsVertical)
            {
                Vector2 piecePos = currentPiece.CurrentPos;
                piecePos.y += (isMovingOpposite ? -0.5f : 0.5f);
                Vector2Int pieceGridPos = new Vector2Int(
                    Mathf.FloorToInt(piecePos.y),
                    Mathf.FloorToInt(piecePos.x)
                    );
                if (!CanMovePiece(pieceGridPos)) return;
                currentPiece.CurrentGridPos = pieceGridPos;
                currentPiece.UpdatePos(offset.y);
            }
            else
            {
                Vector2 piecePos = currentPiece.CurrentPos;
                piecePos.x += (isMovingOpposite ? -0.5f : 0.5f);
                Vector2Int pieceGridPos = new Vector2Int(
                    Mathf.FloorToInt(piecePos.y),
                    Mathf.FloorToInt(piecePos.x)
                    );
                if (!CanMovePiece(pieceGridPos)) return;
                currentPiece.CurrentGridPos = pieceGridPos;
                currentPiece.UpdatePos(offset.x);
            }
            previousPos = currentPos;
        }

        else if (Input.GetMouseButtonUp(0) && currentPiece != null)
        {
            currentPiece.transform.position = new Vector3(
                currentPiece.CurrentGridPos.y + 0.5f,
                currentPiece.CurrentGridPos.x + 0.5f,
                0);
            currentPiece = null;
            currentPos = Vector2.zero;
            previousPos = Vector2.zero;
            CheckWin();
        }
    }

    private bool CanMovePiece(Vector2Int pieceGridPos)
    {
        List<Vector2Int> piecePos = new List<Vector2Int>();
        for (int i = 0; i < currentPiece.Size; i++)
        {
            piecePos.Add(pieceGridPos +
                (currentPiece.IsVertical ? Vector2Int.right : Vector2Int.up) * i
                );
        }

        foreach (var pos in piecePos)
        {
            if (!IsValidPos(pos) || pieceCollision[pos.x, pos.y])
            {
                return false;
            }
        }

        return true;
    }

    private bool IsMovingOpposite()
    {
        Vector2 result = Vector2.zero;
        for (int i = Mathf.Max(0, offsets.Count - 20); i < offsets.Count; i++)
        {
            result += offsets[i];
        }
        float val = currentPiece.IsVertical ? result.y : result.x;
        if (Mathf.Abs(val) > 0.2f)
        {
            return val < 0;
        }

        return true;
    }

    private bool IsValidPos(Vector2Int pos)
    {
        return pos.x >= 0 && pos.y >= 0 && pos.x < _level.Rows && pos.y < _level.Columns;
    }

    public void CheckWin()
    {
        if (winPiece.CurrentGridPos.y + winPiece.Size < _level.Columns)
        {
            return;
        }

        hasGameFinished = true;
        StartCoroutine(GameFinished());
    }

    private IEnumerator GameFinished()
    {
        yield return new WaitForSeconds(2f);
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}
