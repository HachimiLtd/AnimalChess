using System;
using Godot;

public partial class ChessBoard : TileMapLayer
{
    private Vector2I _groundSize;
    public void LoadLayers(ChessSystem system)
    {
        _groundSize = new Vector2I(12,12);
        system.GroundSize = _groundSize;

        PieceInstance[][] pieceLayer = system.PieceLayer;
        GroundType[][] groundLayer = system.GroundLayer;

        groundLayer = new GroundType[_groundSize.X+2][];
        pieceLayer = new PieceInstance[_groundSize.X+2][];
        for( int i=0; i<_groundSize.X+2; i++ )
        {
            groundLayer[i] = new GroundType[_groundSize.Y+2];
            pieceLayer[i] = new PieceInstance[_groundSize.Y+2];
        }
        for( int i=0; i<_groundSize.X+2; i++ )
        {
            groundLayer[i][0] = GroundType.BOUNDARY;
            groundLayer[i][_groundSize.Y+1] = GroundType.BOUNDARY;
        }
        for( int i=1; i<_groundSize.Y+1; i++ )
        {
            groundLayer[0][i] = GroundType.BOUNDARY;
            groundLayer[_groundSize.X+1][i] = GroundType.BOUNDARY;
        }



        ///TODO///
    }

    public static GroundType groundTypeFromString(String s){
        switch(s)
        {
            case "NORMAL":
                return GroundType.NORMAL;
            case "FLOODED":
                return GroundType.FLOODED;
            case "TRAP":
                return GroundType.TRAP;
            case "NEST":
                return GroundType.NEST;
            default:
                return GroundType.BOUNDARY;
        }
    }
}