using Godot;

public partial class CatPiece : PieceInstance
{
    public CatPiece() : base(PieceType.CAT) { }

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

    public override void CreateParamHighlights()
    {
        
    }
}