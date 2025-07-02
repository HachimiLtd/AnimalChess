using Godot;

public partial class RatPiece : PieceInstance
{
  private bool _skillUsed = false;

  public RatPiece() : base(PieceType.RAT) { }

  public override void CreateHighlights()
  {
    CreateHighLightsPartial(_gridPosition + Vector2I.Down);
    CreateHighLightsPartial(_gridPosition + Vector2I.Up);
    CreateHighLightsPartial(_gridPosition + Vector2I.Left);
    CreateHighLightsPartial(_gridPosition + Vector2I.Right);
    if (!_skillUsed && _system.GroundLayer[_gridPosition.X][_gridPosition.Y] == GroundType.FLOODED)
      CreateHighlightsRat();
  }

  public override void _Ready()
  {
    base._Ready();
    visited = new bool[_system.GroundSize.X + 2, _system.GroundSize.Y + 2];
  }

  bool[,] visited;
  private void CreateHighlightsRat()
  {
    for (int i = 1; i <= _system.GroundSize.X; i++)
      for (int j = 1; j <= _system.GroundSize.Y; j++)
        visited[i, j] = false;
    IterCreateHR(_gridPosition);
  }
  private void IterCreateHR(Vector2I pos)
  {
    if (visited[pos.X, pos.Y] || _system.GroundLayer[pos.X][pos.Y] != GroundType.FLOODED)
      return;
    visited[pos.X, pos.Y] = true;
    if ((pos - _gridPosition).Length() > 1.00001)
      CreateHighLightsRatPartial(pos);
    if (pos.X > 1)
      IterCreateHR(pos + Vector2I.Left);
    if (pos.X < _system.GroundSize.X)
      IterCreateHR(pos + Vector2I.Right);
    if (pos.Y > 1)
      IterCreateHR(pos + Vector2I.Up);
    if (pos.Y < _system.GroundSize.Y)
      IterCreateHR(pos + Vector2I.Down);
  }

  private void CreateHighLightsRatPartial(Vector2I pos)
  {
    int x = pos.X;
    int y = pos.Y;
    PieceRatTeleHighlight highlight;
    bool pseudo = false;
    if (_system.PieceLayer[x][y] != null)
    {
      PieceInstance instance = _system.PieceLayer[x][y];
      if (instance.Player == _player)
        return;
      if (_system.IsGridKnown(pos) && instance.Type > _type && instance.Type != PieceType.ELEPHANT)
        return;
      if (!_system.IsGridKnown(pos) && instance.Type > _type && instance.Type != PieceType.ELEPHANT)
      {
        pseudo = true;
      }
    }
    highlight = (PieceRatTeleHighlight)CreateHighlight(pos, HighlightType.RAT_TELE);
    highlight.PRTHInitialize(() =>
    {
      _skillUsed = true;
    }, pseudo);
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
          if (instance.Player == _player || instance.Type == PieceType.DOG) //Dogs can't be trapped
            return;
          if (_system.RoleArrangement[x - 1][y - 1] != _player && instance.Type > _type && instance.Type != PieceType.ELEPHANT && _system.IsGridKnown(dest))
            return;
          if (_system.RoleArrangement[x - 1][y - 1] != _player && instance.Type > _type && instance.Type != PieceType.ELEPHANT && !_system.IsGridKnown(dest))
          {
            CreateHighlight(dest, HighlightType.PSEUDO);
            return;
          }
        }
        CreateHighlight(dest);
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
          if (_system.IsGridKnown(dest) && instance.Type > _type && instance.Type != PieceType.ELEPHANT)
            return;
          if (!_system.IsGridKnown(dest) && instance.Type > _type && instance.Type != PieceType.ELEPHANT)
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

  public override void CreateParamHighlights()
  {
    SkipSubmitParam();
  }
}
