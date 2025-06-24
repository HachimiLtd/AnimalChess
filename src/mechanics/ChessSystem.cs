using System;
using System.Collections.Generic;
using Godot;
/*
//whether the chess piece is free to move here
//used in movement range deciding
public enum MoveValidation
{
    ACCEPT,
    BLOCK,
    SPECIAL0,
    //the moves that capture may have a special visual effect
}
*/

public partial class ChessSystem : Node2D
{
    private static PackedScene _resPieceInstance = (PackedScene)GD.Load("res://scenes/piece_instance.tscn");

    private Vector2I _groundSize;
    private GroundType[][] _groundLayer;
    private PieceInstance[][] _pieceLayer;
    //private List<PieceInstance> _pieceInstanceList;
    public Vector2I GroundSize{get{return _groundSize;} set{_groundSize=value;}}
    public GroundType[][] GroundLayer{get{return _groundLayer;}}
    public PieceInstance[][] PieceLayer{get{return _pieceLayer;}}

    private ChessBoard _chessBoard;
    public Node2D MountHightlights;
    public Node2D MountPieces;
    
    public RoleType PlayerRole;
    public bool CurrentlyPlaying = false;

    public ChessSystem(){
        _chessBoard.LoadLayers(this);
    }
    public override void _EnterTree()
    {
        base._EnterTree();
        MountHightlights = (Node2D)GetNode("MountHightlights");
        MountPieces = (Node2D)GetNode("MountPieces");
        _chessBoard = (ChessBoard)GetNode("ChessBoard");
    }

    public void GameInit()
    {
        //foreach(PieceInstance piece in _pieceInstanceList)
        //{
        //    piece.QueueFree();
        //}
        //_pieceInstanceList.Clear();
        for(int i=1;i<=_groundSize.X;i++)
            for(int j=1;j<=_groundSize.Y;j++)
            {
                DestroyPieceInstance(new Vector2I(i,j));
            }

        //////////////////////////////////
    }
    private void CreatePieceInstance(Vector2I pos,RoleType player,PieceType type)
    {
        if(_pieceLayer[pos.X][pos.Y] != null)
            //wtf
            return;
        PieceInstance instance = (PieceInstance)_resPieceInstance.Instantiate();
        instance.Player = player;
        instance.GridPosition = pos;

        MountPieces.AddChild(instance);
        _pieceLayer[pos.X][pos.Y] = instance;
        //_pieceInstanceList.Add(instance);
    }
    private void DestroyPieceInstance(Vector2I pos)
    {
        if(_pieceLayer[pos.X][pos.Y] != null)
            _pieceLayer[pos.X][pos.Y].Destroy();
        _pieceLayer[pos.X][pos.Y]=null;
    }
    
    public void SubmitOperation(ChessOperation operation)
    {
        Vector2I from = operation.From;
        Vector2I to = operation.To;
        PieceInstance instance = _pieceLayer[from.X][from.Y];
        if( instance == null )
            ///////////////////THIS SHOULDN'T HAPPEN//////////////////////
            return;
        
        if( _pieceLayer[to.X][to.Y]!=null )
        {
            _pieceLayer[to.X][to.Y].Destroy();
        }
        instance.GridPosition = to;
        _pieceLayer[from.X][from.Y] = null;
        _pieceLayer[to.X][to.Y] = instance;
    }

    /*
    public void SyncStatusBetweenPlayers(){}
    */

    public void SubmitPieceSelection(Vector2I position)
    {
        PieceInstance inst = _pieceLayer[position.X][position.Y];
        if(inst == null || inst.Player != PlayerRole)
            return;
        
        inst.CreateHighLights();
    }

}