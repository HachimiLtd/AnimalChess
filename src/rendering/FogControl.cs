using Godot;
using Godot.NativeInterop;
using System;

public partial class FogControl : Panel
{
    private int[] _lightStatus;
    private bool[][] _visibilityStatus;

    private ShaderMaterial _fogMaterial;
    [Export]
    private ChessSystem _system;

    public FogControl()
    {
        _lightStatus = new int[1024];
        _visibilityStatus = new bool[24][];
        for(int i=0;i<24;i++)
            _visibilityStatus[i] = new bool[24];
    }
    public override void _Ready()
    {
        base._Ready();

        _fogMaterial = (ShaderMaterial)Material;
    }

    public void UpdateFogData(bool wipeDetection=true)
    {
        int sx = _system.GroundSize.X;
        int sy = _system.GroundSize.Y;
        int index;
        for(int i=0;i<sx;i++)
            for(int j=0;j<sy;j++)
            {
                index = i+j*sx;
                PieceInstance instance = _system.PieceLayer[i+1][j+1];
                if(instance!=null && instance.Player==_system.PlayerRole)
                    _lightStatus[index] = 1;
                else if(((_lightStatus[index] & 2) == 0) || wipeDetection)
                    _lightStatus[index] = 0;
            }
        for(int i=0;i<sx;i++)
            for(int j=0;j<sy;j++)
                if(_system.PieceLayer[i+1][j+1]!=null)
                    _visibilityStatus[i+1][j+1] = DecidePieceVisibility(
                        i+j*sx, i, j, sx, sy);
    }
    public void UpdateFog()
    {
        _fogMaterial.SetShaderParameter("light_status",_lightStatus);
        
        int sx = _system.GroundSize.X;
        int sy = _system.GroundSize.Y;
        for(int i=0;i<sx;i++)
            for(int j=0;j<sy;j++)
                if(_system.PieceLayer[i+1][j+1]!=null)
                    {
                        bool tarV = CheckVisibility(new Vector2I(i+1,j+1));
                        if(_system.PieceLayer[i+1][j+1].Visible && !tarV)
                            _system.PieceLayer[i+1][j+1].forceToBeKnown();
                        _system.PieceLayer[i+1][j+1].Visible = tarV;
                    }
    }

    public void ForceSet2(Vector2I gridPosition)
    {
        gridPosition -= new Vector2I(1,1);
        int sx = _system.GroundSize.X;
        int sy = _system.GroundSize.Y;
        if(gridPosition.X<0 || gridPosition.X>=sx || gridPosition.Y<0 || gridPosition.Y>=sy)
            return;

        int index = gridPosition.X + gridPosition.Y * sx;
        if(_lightStatus[index]>0)
            return;
        _lightStatus[index] = 2;

        _visibilityStatus[gridPosition.X+1][gridPosition.Y+1] = true;
    }

    public bool CheckVisibility(Vector2I pos)
    {
        return _visibilityStatus[pos.X][pos.Y];
    }

    public bool DecidePieceVisibility(int i,int j)
    {
        int sx = _system.GroundSize.X;
        int sy = _system.GroundSize.Y;
        if(i<0 || i>=sx || j<0 || j>=sy)
            return false;

        int index = i + j * sx;
        return DecidePieceVisibility(index, i, j, sx, sy);
    }
    private bool DecidePieceVisibility(int index,int i,int j,int sx,int sy)
    {
        if(_lightStatus[index]!=0)
            return true;

        if(i>0 && _lightStatus[index-1]==1) return true;
        if(j>0 && _lightStatus[index-sx]==1) return true;
        if(i<sx-1 && _lightStatus[index+1]==1) return true;
        if(j<sy-1 && _lightStatus[index+sx]==1) return true;

        return false;
    }

}
