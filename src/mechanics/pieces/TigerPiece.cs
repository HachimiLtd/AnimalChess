using Godot;

public partial class TigerPiece : PieceInstance
{
    public TigerPiece() : base(PieceType.TIGER) { }

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
                CreateHighlight(_gridPosition + offset);
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
                CreateHighlight(_gridPosition + offset);
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
                        CreateHighlight(_gridPosition + offset, HighlightType.PSEUDO);
                        return;
                    }
                }
                CreateHighlight(_gridPosition + offset, type);
                return;
        }
    }

    public override void UpdateDisplay()
    {

    }

    public override void CreateParamHighlights()
    {
        if(_skillUsed){
            SkipSubmitParam();
            return;
        }
        Vector2I offset = (_gridPosition - _tempLastPosition).Clamp(-1, 1);
        Vector2I tar = _gridPosition + offset;
        Vector2I chkBorder = _gridPosition + offset*2;
        if (_system.GroundLayer[tar.X][tar.Y] == GroundType.BOUNDARY || 
            _system.GroundLayer[chkBorder.X][chkBorder.Y] == GroundType.BOUNDARY){
            SkipSubmitParam();
            return;
        }
        if (_system.PieceLayer[tar.X][tar.Y] != null){
            SkipSubmitParam();
            return;
        }
        PieceDetectHighlight highlight = (PieceDetectHighlight)CreateHighlight(tar, HighlightType.DETECT);
        highlight.PDHInitialize(() =>
        {
            _skillUsed = true;
        }, _system);
        CreateHighlight(_gridPosition, HighlightType.PARAM_CANCEL);
    }
}
