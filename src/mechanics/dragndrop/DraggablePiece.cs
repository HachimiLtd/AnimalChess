using Godot;

public partial class DraggablePiece : Control
{
  private PieceType _type;
  public PieceType Type { get { return _type; } set { _type = value; } }
  private PackedScene _draggablePieceScene = (PackedScene)GD.Load("res://scenes/draggable_piece.tscn");
  private Label _label;
  private Vector2 _gridPosition = new Vector2(-1, -1);
  public Vector2 GridPosition { get { return _gridPosition; } set { _gridPosition = value; } }

  private string _TranslatePieceName(PieceType pieceType)
  {
    switch (pieceType)
    {
      case PieceType.CAT:
        return "CAT";
      case PieceType.DOG:
        return "DOG";
      case PieceType.RAT:
        return "RAT";
      case PieceType.WOLF:
        return "WOLF";
      case PieceType.LEOPARD:
        return "LEO\nPARD";
      case PieceType.TIGER:
        return "TIGER";
      case PieceType.LION:
        return "LION";
      case PieceType.ELEPHANT:
        return "ELEP\nHANT";
      default:
        return "UNK";
    }
  }

  public DraggablePiece() : base()
  {
  }

  public DraggablePiece(PieceType type) : base()
  {
    _type = type;
  }

  public override void _Ready()
  {
    _label = GetNode<Label>("Label");
    _label.Text = _TranslatePieceName(_type);
  }

  public override void _Process(double delta)
  {
    // Restore full opacity if not currently dragging (mouse button released)
    if (!Input.IsMouseButtonPressed(MouseButton.Left) && Modulate.A < 1.0f)
    {
      Modulate = new Color(1, 1, 1, 1);
    }
  }

  public override Variant _GetDragData(Vector2 atPosition)
  {
    DraggablePiece piece = _draggablePieceScene.Instantiate<DraggablePiece>();
    piece.Type = _type;

    // Center the drag preview on the cursor
    Control preview = new Control();
    preview.AddChild(piece);
    piece.Position = -Size / 2; // Offset by half the size to center
    SetDragPreview(preview);

    // Hide original piece during drag
    Modulate = new Color(1, 1, 1, 0.5f); // Make semi-transparent instead of invisible

    var dragData = new Godot.Collections.Dictionary
    {
        { "type", (int)_type },
        { "original_piece", this },
        { "grid_position", _gridPosition }
    };
    return dragData;
  }
}