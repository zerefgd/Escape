using UnityEngine;

public class LevelPiece : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _pieceRenderer;
    [SerializeField] private BoxCollider2D _pieceCollider;

    [HideInInspector] public int Id;
    [HideInInspector] public bool IsVertical;
    [HideInInspector] public int Size;
    [HideInInspector] public Vector2Int CurrentGridPos;

    public void Init(Piece piece)
    {
        Id = piece.Id;
        CurrentGridPos = piece.Start;
        IsVertical = piece.IsVertical;
        Size = piece.Size;

        if (piece.IsVertical)
        {
            _pieceRenderer.transform.localPosition = new Vector3(0, Size - 1, 0) * 0.5f;
            _pieceRenderer.size = new Vector2(1, Size);
            _pieceCollider.size = new Vector2(1, Size);
        }
        else
        {
            _pieceRenderer.transform.localPosition = new Vector3(Size - 1, 0, 0) * 0.5f;
            _pieceRenderer.size = new Vector2(Size, 1);
            _pieceCollider.size = new Vector2(Size, 1);
        }
    }
}
