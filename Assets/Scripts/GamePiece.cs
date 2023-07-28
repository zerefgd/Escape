using UnityEngine;

public class GamePiece : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _pieceRenderer;
    [SerializeField] private BoxCollider2D _pieceCollider;

    [HideInInspector] public bool IsVertical;
    [HideInInspector] public int Size;
    [HideInInspector] public Vector2Int CurrentGridPos;
    [HideInInspector] public Vector2 CurrentPos;

    public void Init(Piece piece)
    {
        CurrentGridPos = piece.Start;
        IsVertical = piece.IsVertical;
        Size = piece.Size;
        CurrentPos = new Vector2(CurrentGridPos.y + 0.5f, CurrentGridPos.x + 0.5f);

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

    public void UpdatePos(float offset)
    {
        CurrentPos += (IsVertical ? Vector2.up : Vector2.right) * offset;
        transform.position = CurrentPos;
    }
}
