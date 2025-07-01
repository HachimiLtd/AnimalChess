using Godot;

public partial class MountPieces : Control
{
  private PackedScene _draggablePieceScene = (PackedScene)GD.Load("res://scenes/draggable_piece.tscn");
  private Node2D _chessMount;

  [Export]
  private Arrange _arrange;

  [Signal]
  public delegate void ChessPlacedEventHandler();

  public override void _Ready()
  {
    base._Ready();

    _chessMount = GetNode<Node2D>("ChessMount");

    _arrange.DraggablePieces = new DraggablePiece[12][];
    for (int i = 0; i < 12; i++)
    {
      _arrange.DraggablePieces[i] = new DraggablePiece[12];
    }
  }

  private Vector2 SnapToGrid(Vector2 position)
  {
    const int gridSize = 48;
    return new Vector2(
      Mathf.Floor(position.X / gridSize) * gridSize,
      Mathf.Floor(position.Y / gridSize) * gridSize
    );
  }

  public override bool _CanDropData(Vector2 atPosition, Variant data)
  {
    if (data.VariantType == Variant.Type.Dictionary)
    {
      var dict = data.AsGodotDictionary();
      if (!dict.ContainsKey("original_piece"))
      {
        return false;
      }

      Vector2 snappedPosition = SnapToGrid(atPosition);
      int x = (int)(snappedPosition.X / 48);
      int y = (int)(snappedPosition.Y / 48);

      if (!_arrange.AllowedDropPositions[_arrange.Player].Contains(new Vector2(x, y)))
      {
        return false;
      }

      if (x >= 0 && x < 12 && y >= 0 && y < 12)
      {
        if (_arrange.DraggablePieces[x][y] == null)
        {
          return true;
        }
        Vector2 originalGridPosition = dict["grid_position"].AsVector2();
        if (originalGridPosition != new Vector2(-1, -1))
        {
          return true;
        }
      }
    }
    return false;
  }

  public override void _DropData(Vector2 atPosition, Variant data)
  {
    if (data.VariantType == Variant.Type.Dictionary)
    {
      var dict = data.AsGodotDictionary();
      var pieceType = (PieceType)(int)dict["type"];
      var originalPiece = dict["original_piece"].AsGodotObject() as DraggablePiece;

      Vector2 originalGridPosition = dict["grid_position"].AsVector2();
      if (originalGridPosition != new Vector2(-1, -1))
      {
        int originalX = (int)originalGridPosition.X;
        int originalY = (int)originalGridPosition.Y;

        if (originalX >= 0 && originalX < 12 && originalY >= 0 && originalY < 12)
        {
          _arrange.DraggablePieces[originalX][originalY] = null;
        }
      }

      DraggablePiece newPiece = _draggablePieceScene.Instantiate<DraggablePiece>();
      newPiece.Type = pieceType;

      Vector2 snappedPosition = SnapToGrid(atPosition);
      int x = (int)(snappedPosition.X / 48);
      int y = (int)(snappedPosition.Y / 48);

      if (x < 0 || x >= 12 || y < 0 || y >= 12)
      {
        GD.PrintErr("Invalid drop position: ", snappedPosition);
        return;
      }

      newPiece.Position = snappedPosition;

      if (_arrange.DraggablePieces[x][y] != null)
      {
        DraggablePiece occupyingPiece = _arrange.DraggablePieces[x][y];
        _arrange.DraggablePieces[x][y] = newPiece;
        if (originalGridPosition != new Vector2(-1, -1))
        {
          int originalX = (int)originalGridPosition.X;
          int originalY = (int)originalGridPosition.Y;

          if (originalX >= 0 && originalX < 12 && originalY >= 0 && originalY < 12)
          {
            _arrange.DraggablePieces[originalX][originalY] = occupyingPiece;
            occupyingPiece.Position = new Vector2(originalX * 48, originalY * 48);
            occupyingPiece.GridPosition = new Vector2(originalX, originalY);
          }
        }
      }
      else
      {
        _arrange.DraggablePieces[x][y] = newPiece;
      }

      newPiece.GridPosition = new Vector2(x, y);

      EmitSignal(SignalName.ChessPlaced);
      _chessMount.AddChild(newPiece);
      originalPiece?.QueueFree();
    }
  }
}