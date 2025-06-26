using System;
using Godot;

public partial class CatPiece : PieceInstance
{
    public override void CreateHighLights()
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
        switch(_system.GroundLayer[x][y])
        {
            case GroundType.BOUNDARY:
                return;
            
        }
    }

    public override void UpdateDisplay()
    {
        
    }
}