using Godot;

public partial class TigerPiece : PieceInstance
{
    public TigerPiece() : base(PieceType.TIGER) { }

    public override void CreateHighlights()
    {
        CreateHighLightsPartial(Vector2I.Down);
        CreateHighLightsPartial(Vector2I.Up);
        CreateHighLightsPartial(Vector2I.Left);
        CreateHighLightsPartial(Vector2I.Right);
    }
    private void CreateHighLightsPartial(Vector2I offset)
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
                CreateHighlight(_gridPosition + offset);
                return;
            case GroundType.FLOODED:
                if (_system.PieceLayer[x][y] != null)
                {
                    return;
                }
                CreateHighLightsPartial(offset + offset.Clamp(-1, 1));
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
                CreateHighlight(_gridPosition + offset);
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
                CreateHighlight(_gridPosition + offset);
                return;
        }
    }

    public override void UpdateDisplay()
    {

    }
}
