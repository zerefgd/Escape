using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    public static LevelGenerator Instance;
    public static int PieceId = 0;

    [SerializeField] private int _rows;
    [SerializeField] private int _columns;
    [SerializeField] private Level _level;
    [SerializeField] private SpriteRenderer _bgPrefab;
    [SerializeField] private LevelPiece _piecePrefab;
    [SerializeField] private LevelPiece _winPrefab;

    private bool isStartPiece;
    private LevelPiece winPiece;
    private LevelPiece currentPiece;

    private void Awake()
    {
        Instance = this;
        isStartPiece = true;
        CreateLevel();
        SpawnLevel();
    }

    private void CreateLevel()
    {
        if (_rows == _level.Rows && _columns == _level.Columns)
        {
            return;
        }

        _level.Rows = _rows;
        _level.Columns = _columns;
        _level.Pieces = new List<Piece>();
        _level.WinPiece = new Piece();
        EditorUtility.SetDirty(_level);
    }

    private void SpawnLevel()
    {
        //Set Up BG
        SpriteRenderer bg = Instantiate(_bgPrefab);
        bg.size = new Vector2(_level.Columns, _level.Rows);
        bg.transform.position = new Vector3(_level.Columns, _level.Rows, 0) * 0.5f;

        SpawnWinPiece();

        //Spawn All Pieces
        for (int i = 0; i < _level.Pieces.Count; i++)
        {
            Piece piece = _level.Pieces[i];
            piece.Id = PieceId++;
            _level.Pieces[i] = piece;
            Vector3 spawnPos = new Vector3(
                piece.Start.y + 0.5f,
                piece.Start.x + 0.5f, 0f
                );
            LevelPiece temp = Instantiate(_piecePrefab);
            temp.transform.position = spawnPos;
            temp.Init(piece);
        }

        //Set Up Camera
        Camera.main.orthographicSize = Mathf.Max(_level.Columns, _level.Rows) * 1.2f + 2f;
        Vector3 camPos = Camera.main.transform.position;
        camPos.x = _level.Columns * 0.5f;
        camPos.y = _level.Rows * 0.5f;
        Camera.main.transform.position = camPos;
    }

    private void SpawnWinPiece()
    {
        winPiece = Instantiate(_winPrefab);
        Vector3 spawnPos = new Vector3(
            _level.WinPiece.Start.y + 0.5f,
            _level.WinPiece.Start.x + 0.5f, 0
            );
        winPiece.transform.position = spawnPos;
        _level.WinPiece.Id = 0;
        winPiece.Init(_level.WinPiece);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isStartPiece = !isStartPiece;
        }

        if (isStartPiece)
        {
            UpdateStartPiece();
        }
        else
        {
            UpdateGamePiece();
        }
    }

    private void UpdateStartPiece()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2Int mouseGrid = new Vector2Int(
                Mathf.FloorToInt(mousePos.y),
                Mathf.FloorToInt(mousePos.x)
                );
            if (!IsValidPos(mouseGrid))
            {
                return;
            }

            if (winPiece != null)
            {
                Destroy(winPiece.gameObject);
            }

            _level.WinPiece = new Piece()
            {
                Id = 0,
                IsVertical = false,
                Size = 1,
                Start = mouseGrid,
            };

            EditorUtility.SetDirty(_level);
            SpawnWinPiece();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            if (winPiece != null)
            {
                Destroy(winPiece.gameObject);
                winPiece = null;
            }

            _level.WinPiece = new Piece();
            EditorUtility.SetDirty(_level);
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            if (winPiece == null)
            {
                return;
            }

            Destroy(winPiece.gameObject);
            _level.WinPiece.Size++;
            SpawnWinPiece();
            EditorUtility.SetDirty(_level);
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            if (winPiece == null)
            {
                return;
            }

            Destroy(winPiece.gameObject);
            _level.WinPiece.Size--;
            if (_level.WinPiece.Size == 0)
            {
                _level.WinPiece.Size = 1;
            }

            SpawnWinPiece();
            EditorUtility.SetDirty(_level);
        }
    }

    private void UpdateGamePiece()
    {
        if (!Input.anyKeyDown)
        {
            return;
        }

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
        Vector2Int mouseGrid = new Vector2Int(
                Mathf.FloorToInt(mousePos.y),
                Mathf.FloorToInt(mousePos.x)
                );

        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
            if (!hit || !IsValidPos(mouseGrid)) return;
            if (hit.collider.transform.parent.TryGetComponent(out currentPiece))
            {
            }
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (!IsValidPos(mouseGrid)) return;
            SpawnGamePiece(true);
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            if (!IsValidPos(mouseGrid)) return;
            SpawnGamePiece(false);
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            if (currentPiece == null)
            {
                return;
            }

            int removeId = -1;

            for (int i = 0; i < _level.Pieces.Count; i++)
            {
                if (_level.Pieces[i].Id == currentPiece.Id)
                {
                    removeId = i;
                    break;
                }
            }
            Debug.Log(currentPiece.Id);
            Debug.Log(removeId);
            if (removeId != -1)
            {
                _level.Pieces.RemoveAt(removeId);
            }

            if (currentPiece != null)
            {
                Destroy(currentPiece.gameObject);
                currentPiece = null;
            }

            EditorUtility.SetDirty(_level);
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            if (currentPiece == null)
            {
                return;
            }

            for (int i = 0; i < _level.Pieces.Count; i++)
            {
                Piece piece = _level.Pieces[i];
                if (piece.Id == currentPiece.Id)
                {
                    piece.Size++;
                    _level.Pieces[i] = piece;
                    currentPiece.Init(piece);
                    break;
                }
            }

            EditorUtility.SetDirty(_level);
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            if (currentPiece == null)
            {
                return;
            }

            for (int i = 0; i < _level.Pieces.Count; i++)
            {
                Piece piece = _level.Pieces[i];
                if (piece.Id == currentPiece.Id)
                {
                    piece.Size--;
                    if (piece.Size == 0)
                    {
                        piece.Size = 1;
                    }
                    _level.Pieces[i] = piece;
                    currentPiece.Init(piece);
                    break;
                }
            }

            EditorUtility.SetDirty(_level);
        }
    }

    private void SpawnGamePiece(bool vertical)
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int mouseGrid = new Vector2Int(
                Mathf.FloorToInt(mousePos.y),
                Mathf.FloorToInt(mousePos.x)
                );
        Piece spawnPiece = new Piece();
        spawnPiece.Id = PieceId++;
        spawnPiece.IsVertical = vertical;
        spawnPiece.Start = mouseGrid;
        spawnPiece.Size = 1;
        _level.Pieces.Add(spawnPiece);
        SpawnGamePiece(spawnPiece);
        EditorUtility.SetDirty(_level);
    }

    private void SpawnGamePiece(Piece piece)
    {
        Vector3 spawnPos = new Vector3(
            piece.Start.y + 0.5f,
            piece.Start.x + 0.5f, 0f
            );
        LevelPiece temp = Instantiate(_piecePrefab);
        temp.transform.position = spawnPos;
        temp.Init(piece);
        currentPiece = temp;
    }

    private bool IsValidPos(Vector2Int pos)
    {
        return pos.x >= 0 && pos.y >= 0 && pos.x < _rows && pos.y < _columns;
    }
}
