using Godot;

public partial class LeopardPiece : PieceInstance
{
  public LeopardPiece() : base(PieceType.LEOPARD) { }

  public override void CreateHighlights()
  {
    CreateHighLightsPartial(_gridPosition + Vector2I.Down);
    CreateHighLightsPartial(_gridPosition + Vector2I.Up);
    CreateHighLightsPartial(_gridPosition + Vector2I.Left);
    CreateHighLightsPartial(_gridPosition + Vector2I.Right);
  }
  private void CreateHighLightsPartial(Vector2I dest,bool paramMode=false)
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
          if (instance.Player == _player)
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
        if(paramMode)
          CreateHighlight(dest, HighlightType.SECOND);
        else
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
            if(paramMode)
              ((PieceSecondHighlight)CreateHighlight(dest, HighlightType.SECOND))
                .SetPseudo();
            else
              CreateHighlight(dest, HighlightType.PSEUDO);
            return;
          }
        }
        if(paramMode)
          CreateHighlight(dest, HighlightType.SECOND);
        else
          CreateHighlight(dest);
        return;
    }
  }

  public override void UpdateDisplay()
  {

  }

  public override void CreateParamHighlights()
  {
    if( !IsSkillAllowedPartial(_gridPosition + Vector2I.Down) ||
        !IsSkillAllowedPartial(_gridPosition + Vector2I.Up) ||
        !IsSkillAllowedPartial(_gridPosition + Vector2I.Left) ||
        !IsSkillAllowedPartial(_gridPosition + Vector2I.Right))
    {
      SkipSubmitParam();
      return;
    }

    CreateHighlight(_gridPosition,HighlightType.PARAM_CANCEL);
    CreateHighLightsPartial(_gridPosition + Vector2I.Down, true);
    CreateHighLightsPartial(_gridPosition + Vector2I.Up, true);
    CreateHighLightsPartial(_gridPosition + Vector2I.Left, true);
    CreateHighLightsPartial(_gridPosition + Vector2I.Right, true);
  }

  public bool IsSkillAllowedPartial(Vector2I pos)
  {
    int x = pos.X;
    int y = pos.Y;
    PieceInstance instance = _system.PieceLayer[x][y];
    if (instance == null)
      return true;
    if (instance.Player == _player)
      return true;
    return false;
  }
}
