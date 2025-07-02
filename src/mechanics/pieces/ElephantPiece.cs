using Godot;

public partial class ElephantPiece : PieceInstance
{
    public ElephantPiece() : base(PieceType.ELEPHANT) { }
    private Vector4I _tempParams;
    private bool skillUsed = false;

    public override void CreateHighlights()
    {
        CreateHighLightsPartial(_gridPosition + Vector2I.Down);
        CreateHighLightsPartial(_gridPosition + Vector2I.Up);
        CreateHighLightsPartial(_gridPosition + Vector2I.Left);
        CreateHighLightsPartial(_gridPosition + Vector2I.Right);
    }
    private void CreateHighLightsPartial(Vector2I dest)
    {
        _tempParams = Vector4I.Zero;

        int x = dest.X;
        int y = dest.Y;

        if (!skillUsed && _system.GroundLayer[x][y] != GroundType.BOUNDARY)
        {
            Vector2I atkPos = -_gridPosition + 2 * dest;

#pragma warning disable CS0642 // Possible mistaken empty statement
            if (_system.GroundLayer[atkPos.X][atkPos.Y] == GroundType.BOUNDARY ||
                _system.GroundLayer[atkPos.X][atkPos.Y] == GroundType.FLOODED) ;
#pragma warning restore CS0642 // Possible mistaken empty statement

            else if (!_system.IsGridVisible(atkPos) ||
                (_system.PieceLayer[atkPos.X][atkPos.Y] != null &&
                  _system.PieceLayer[atkPos.X][atkPos.Y].Player != _player &&
                  _system.PieceLayer[atkPos.X][atkPos.Y].Type != PieceType.RAT))
            {
                PieceAttackHighlight highlight = (PieceAttackHighlight)CreateHighlight(atkPos, HighlightType.ATTACK);
                highlight.PAHInitialize((Vector4I param) =>
                {
                    _tempParams = param;
                    skillUsed = true;
                }, _system.PieceLayer[atkPos.X][atkPos.Y] == null ||
                   _system.PieceLayer[atkPos.X][atkPos.Y].Type == PieceType.RAT);
            }
        }

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
                    if (_system.RoleArrangement[x - 1][y - 1] != _player && (instance.Type > _type || instance.Type == PieceType.RAT) && _system.IsGridKnown(dest))
                        return;
                    if (_system.RoleArrangement[x - 1][y - 1] != _player && (instance.Type > _type || instance.Type == PieceType.RAT) && !_system.IsGridKnown(dest))
                    {
                        CreateHighlight(dest, HighlightType.PSEUDO);
                        return;
                    }
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
                    if (_system.IsGridKnown(dest) && (instance.Type > _type || instance.Type == PieceType.RAT))
                        return;
                    if (!_system.IsGridKnown(dest) && (instance.Type > _type || instance.Type == PieceType.RAT))
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
        HandleSubmitParam(_tempParams);
    }
}
