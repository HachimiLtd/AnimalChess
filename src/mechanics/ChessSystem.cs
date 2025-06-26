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
    //private static PackedScene _resPieceInstance = (PackedScene)GD.Load("res://scenes/piece_instance.tscn");
    private static Dictionary<PieceType,PackedScene> _resPieceInstances;
    static ChessSystem()
    {
        _resPieceInstances = new Dictionary<PieceType, PackedScene>
        {
            [PieceType.CAT] = (PackedScene)GD.Load("res://scenes/pieces/piece_cat.tscn")
        };
    }

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

    public bool HightlightsExist = false;

    ChessSystem()
    {
        _pieceLayer = new PieceInstance[24][];
        _groundLayer = new GroundType[24][];
        for(int i=0;i<24;i++)
        {
            _pieceLayer[i] = new PieceInstance[24];
            _groundLayer[i] = new GroundType[24];
        }
    }

    public override void _EnterTree()
    {
        base._EnterTree();
        MountHightlights = (Node2D)GetNode("MountHightlights");
        MountPieces = (Node2D)GetNode("MountPieces");
        _chessBoard = (ChessBoard)GetNode("ChessBoard");
    }

    public void GameInit(ChessPieceInitialArrangement pieceArrangement,RoleType player)
    {
        for(int i=1;i<=_groundSize.X;i++)
            for(int j=1;j<=_groundSize.Y;j++)
            {
                DestroyPieceInstance(new Vector2I(i,j));
            }
        
        _chessBoard.LoadLayers(this);
        for(int i=0;i<_groundSize.X;i++)
            for(int j=0;j<_groundSize.Y;i++)
            {
                if(pieceArrangement.typeMap[i][j]==PieceType.EMPTY)
                    continue;
                CreatePieceInstance(new Vector2I(i+1,j+1),pieceArrangement.roleMap[i][j],pieceArrangement.typeMap[i][j]);
            }
        
        PlayerRole = player;
        HightlightsExist = false;
    }
    private void CreatePieceInstance(Vector2I pos,RoleType player,PieceType type)
    {
        if(_pieceLayer[pos.X][pos.Y] != null)
            //wtf
            return;
        PieceInstance instance = (PieceInstance)_resPieceInstances[type].Instantiate();
        instance.Player = player;
        instance.GridPosition = pos;

        MountPieces.AddChild(instance);
        _pieceLayer[pos.X][pos.Y] = instance;
        instance.Choosable = PlayerRole==player;
        //_pieceInstanceList.Add(instance);
    }
    private void DestroyPieceInstance(Vector2I pos)
    {
        if(_pieceLayer[pos.X][pos.Y] != null)
            _pieceLayer[pos.X][pos.Y].Destroy();
        _pieceLayer[pos.X][pos.Y]=null;
    }
    
    public void HandleOperation(ChessOperation operation)
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
        HightlightsExist = false;
    }

    /*
    public void SyncStatusBetweenPlayers(){}
    */

    public void HandlePieceSelection(Vector2I position)
    {
        PieceInstance inst = _pieceLayer[position.X][position.Y];
        if(inst == null || inst.Player != PlayerRole)
            return;
        
        if(!HightlightsExist)
            inst.CreateHighLights();
        else
            inst.ClearHighlights();
    }

}