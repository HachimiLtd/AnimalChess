using System;
using Godot;

public partial class WolfPiece : PieceInstance
{
  public WolfPiece() : base(PieceType.WOLF) { }
  public override void CreateHighlights()
  {
    CreateHighLightsPartial(_gridPosition + Vector2I.Down);
    CreateHighLightsPartial(_gridPosition + Vector2I.Up);
    CreateHighLightsPartial(_gridPosition + Vector2I.Left);
    CreateHighLightsPartial(_gridPosition + Vector2I.Right);
  }
  private void CreateHighLightsPartial(Vector2I dest)
  {
    int x = dest.X;
    int y = dest.Y;
    switch (_system.GroundLayer[x][y])
    {
      case GroundType.BOUNDARY:
        return;
      default:
        if (_system.PieceLayer[x][y] != null)
        {
          PieceInstance instance = _system.PieceLayer[x][y];
          if (instance.Player == _player)
            return;
          if (instance.Type > _type)
            return;
        }
        CreateHighlight(dest);
        return;
    }
  }

  public override void UpdateDisplay()
  {

  }
}
