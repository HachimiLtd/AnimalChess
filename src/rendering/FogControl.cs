using Godot;
using Godot.NativeInterop;
using System;

public partial class FogControl : Panel
{
    private int[] _lightStatus;

    private ShaderMaterial _fogMaterial;
    [Export]
    private ChessSystem _system;

    public FogControl()
    {
        _lightStatus = new int[1024];
    }
    public override void _Ready()
    {
        base._Ready();

        _fogMaterial = (ShaderMaterial)Material;
    }

    public void UpdateFog(bool wipeDetection=true)
    {
        int sx = _system.GroundSize.X;
        int sy = _system.GroundSize.Y;
        int index;
        for(int i=0;i<sx;i++)
            for(int j=0;j<sy;j++)
            {
                index = i+j*sx;
                if(_system.PieceLayer[i+1][j+1]!=null)
                    _lightStatus[index] = 1;
                else
                    _lightStatus[index] &= wipeDetection?0:2;
            }
        _fogMaterial.SetShaderParameter("light_status",_lightStatus);
    }

}
