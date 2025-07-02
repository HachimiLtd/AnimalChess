using Godot;

public partial class WolfPiece : PieceInstance
{
  private static PackedScene _resWHighlight = (PackedScene)GD.Load("res://scenes/wolf_highlighter.tscn");
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
      case GroundType.TRAP:
        if (_system.PieceLayer[x][y] != null)
        {
          PieceInstance instance = _system.PieceLayer[x][y];
          if (instance.Player == _player || instance.Type == PieceType.DOG)
            return;
        }
        CreateHighlight(dest);
        return;
      case GroundType.FLOODED:
        return;
      case GroundType.NEST:
      case GroundType.NEST_REAL:
      case GroundType.NEST_FAKE:
        if (_system.RoleArrangement[x - 1][y - 1] == _player)
          return;
        if (_system.PieceLayer[x][y] != null)
        {
          PieceInstance instance = _system.PieceLayer[x][y];
          if (instance.Player == _player)
            return;
        }
        CreateHighlight(dest);
        return;
      default:
        if (_system.PieceLayer[x][y] != null)
        {
          PieceInstance instance = _system.PieceLayer[x][y];
          if (instance.Player == _player)
            return;
          if (_system.IsGridKnown(dest) && instance.Type > _type)
            return;
          if (!_system.IsGridKnown(dest) && instance.Type > _type)
          {
            CreateHighlight(dest, HighlightType.PSEUDO);
            return;
          }
        }
        CreateHighlight(dest);
        return;
    }
  }

  public override void UpdateDisplay()
  {

  }

  WolfHighlighter highlighter;
  public override void CreateParamHighlights()
  {
    highlighter = (WolfHighlighter)_resWHighlight.Instantiate();
    highlighter.Initialize(_system,_gridPosition);
    _system.MountHightlights.AddChild(highlighter);
    highlighter.SubmitParam += HandleSubmitParam;
    PieceHighlight highlight = CreateHighlight(_gridPosition, HighlightType.PARAM_CANCEL);
    highlight.MouseEntered += highlighter.NoIndicator;
  }

  public override void ClearAdditionalParamHighlights()
  {
    base.ClearAdditionalParamHighlights();
    if(highlighter != null)
    {
      highlighter.Destroy();
      highlighter = null;
    }
  }
}
