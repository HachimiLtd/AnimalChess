using Godot;

public partial class LionPiece : PieceInstance
{
  public LionPiece() : base(PieceType.LION) { }

  private bool _skillUsed = false;
  
  public override void CreateHighlights()
  {
    CreateHighLightsPartial(Vector2I.Down);
    CreateHighLightsPartial(Vector2I.Up);
    CreateHighLightsPartial(Vector2I.Left);
    CreateHighLightsPartial(Vector2I.Right);
  }
  private void CreateHighLightsPartial(Vector2I offset, HighlightType type = HighlightType.NORMAL)
  {
    int x = _gridPosition.X + offset.X;
    int y = _gridPosition.Y + offset.Y;
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
        CreateHighlightsAtomic(_gridPosition + offset, type);
        return;
      case GroundType.FLOODED:
        if (_system.PieceLayer[x][y] != null)
        {
          if (_system.PieceLayer[x][y].Visible)
            return;
          else
            CreateHighLightsPartial(offset + offset.Clamp(-1, 1), HighlightType.PSEUDO);
            return;
        }
        CreateHighLightsPartial(offset + offset.Clamp(-1, 1), type);
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
        CreateHighlightsAtomic(_gridPosition + offset, type);
        return;
      default:
        if (_system.PieceLayer[x][y] != null)
        {
          PieceInstance instance = _system.PieceLayer[x][y];
          if (instance.Player == _player)
            return;
          if (_system.IsGridKnown(_gridPosition + offset) && instance.Type > _type)
            return;
          if (!_system.IsGridKnown(_gridPosition + offset) && instance.Type > _type)
          {
            CreateHighlightsAtomic(_gridPosition + offset, type, true);
            return;
          }
        }
        CreateHighlightsAtomic(_gridPosition + offset, type);
        return;
    }
  }

  private void CreateHighlightsAtomic( Vector2I pos, HighlightType type, bool pseudo = false)
  {
    if(type == HighlightType.NORMAL)
    {
      if(pseudo)
        CreateHighlight(pos, HighlightType.PSEUDO);
      else
        CreateHighlight(pos);
    }
    else if(type == HighlightType.SECOND_AFTERATTACK)
    {
      PieceSecondAfterattackHighlight highlight = (PieceSecondAfterattackHighlight)CreateHighlight(pos, HighlightType.SECOND_AFTERATTACK);
      highlight.PSAHInitialize(() => {
        _skillUsed = true;
      }, pseudo);
    }
    else
    {
      CreateHighlight(pos, type);
    }
  }

  public override void UpdateDisplay()
  {

  }

  public override void CreateParamHighlights()
  {
    if (_skillUsed || !_tempCapturedPiece)
    {
      SkipSubmitParam();
      return;
    }
    
    CreateHighlight(_gridPosition,HighlightType.PARAM_CANCEL);
    CreateHighLightsPartial(Vector2I.Down, HighlightType.SECOND_AFTERATTACK);
    CreateHighLightsPartial(Vector2I.Up, HighlightType.SECOND_AFTERATTACK);
    CreateHighLightsPartial(Vector2I.Left, HighlightType.SECOND_AFTERATTACK);
    CreateHighLightsPartial(Vector2I.Right, HighlightType.SECOND_AFTERATTACK);
  }
}
