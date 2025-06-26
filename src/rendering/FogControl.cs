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
                        i+j*sx, _system.PieceLayer[i+1][j+1], 
                        i, j, sx, sy);
    }
    public void UpdateFog()
    {
        _fogMaterial.SetShaderParameter("light_status",_lightStatus);
        
        int sx = _system.GroundSize.X;
        int sy = _system.GroundSize.Y;
        for(int i=0;i<sx;i++)
            for(int j=0;j<sy;j++)
                if(_system.PieceLayer[i+1][j+1]!=null)
                    _system.PieceLayer[i+1][j+1].Visible = CheckVisibility(new Vector2I(i+1,j+1));
    }

    public bool CheckVisibility(Vector2I pos)
    {
        return _visibilityStatus[pos.X][pos.Y];
    }

    private bool DecidePieceVisibility(int index,PieceInstance instance,int i,int j,int sx,int sy)
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
