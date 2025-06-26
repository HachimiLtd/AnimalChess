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
    private static Dictionary<PieceType, PackedScene> _resPieceInstances;
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
    public Vector2I GroundSize { get { return _groundSize; } set { _groundSize = value; } }
    public GroundType[][] GroundLayer { get { return _groundLayer; } set { _groundLayer = value; } }
    public PieceInstance[][] PieceLayer { get { return _pieceLayer; } set { _pieceLayer = value; } }

    private ChessBoard _chessBoard;
    private FogControl _fog;
    public Node2D MountHightlights;
    public Node2D MountPieces;

    public RoleType PlayerRole;
    public bool CurrentlyPlaying = false;

    public bool HightlightsExist = false;

    ChessSystem()
    {
        _pieceLayer = new PieceInstance[24][];
        _groundLayer = new GroundType[24][];
        for (int i = 0; i < 24; i++)
        {
            _pieceLayer[i] = new PieceInstance[24];
            _groundLayer[i] = new GroundType[24];
        }
    }

    public override void _Ready()
    {
        base._Ready();
        MountHightlights = (Node2D)GetNode("MountHightlights");
        MountPieces = (Node2D)GetNode("MountPieces");
        _chessBoard = (ChessBoard)GetNode("ChessBoard");
        _fog = (FogControl)GetNode("FogPanel");


        ChessPieceInitialArrangement arr = new ChessPieceInitialArrangement();
        arr.typeMap =
        [
            [PieceType.CAT, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY],
            [PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY,],
            [PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY,],
            [PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY,],
            [PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY,],
            [PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY,],
            [PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY,],
            [PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY,],
            [PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY,],
            [PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY,],
            [PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY,],
            [PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY, PieceType.EMPTY,],
        ];
        arr.roleMap =
        [
            [RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1],
            [RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1,],
            [RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1,],
            [RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1,],
            [RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1],
            [RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1,],
            [RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1,],
            [RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1,],
            [RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1],
            [RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1,],
            [RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1,],
            [RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1, RoleType.P1,],
        ];

        GameInit(arr, RoleType.P1);
    }

    public void GameInit(ChessPieceInitialArrangement pieceArrangement, RoleType player)
    {
        for (int i = 1; i <= _groundSize.X; i++)
            for (int j = 1; j <= _groundSize.Y; j++)
            {
                DestroyPieceInstance(new Vector2I(i, j));
            }

        _chessBoard.LoadLayers(this);
        for (int i = 0; i < _groundSize.X; i++)
            for (int j = 0; j < _groundSize.Y; j++)
            {
                if (pieceArrangement.typeMap[i][j] == PieceType.EMPTY)
                    continue;
                CreatePieceInstance(new Vector2I(i + 1, j + 1), pieceArrangement.roleMap[i][j], pieceArrangement.typeMap[i][j]);
            }

        PlayerRole = player;
        HightlightsExist = false;
        _fog.UpdateFog();
    }
    private void CreatePieceInstance(Vector2I pos, RoleType player, PieceType type)
    {
        if (_pieceLayer[pos.X][pos.Y] != null)
            //wtf
            return;
        PieceInstance instance = (PieceInstance)_resPieceInstances[type].Instantiate();
        instance.Player = player;
        instance.GridPosition = pos;

        MountPieces.AddChild(instance);
        _pieceLayer[pos.X][pos.Y] = instance;
        instance.Choosable = PlayerRole == player;
        //_pieceInstanceList.Add(instance);
    }
    private void DestroyPieceInstance(Vector2I pos)
    {
        if (_pieceLayer[pos.X][pos.Y] != null)
            _pieceLayer[pos.X][pos.Y].Destroy();
        _pieceLayer[pos.X][pos.Y] = null;
    }

    public void HandleLocalOperation(ChessOperation operation)
    {
        HandleOperation(operation.From, operation.To);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    public void HandleOperation(Vector2I from, Vector2I to)
    {
        PieceInstance instance = _pieceLayer[from.X][from.Y];
        if (instance == null)
            ///////////////////THIS SHOULDN'T HAPPEN//////////////////////
            return;

        int callerId = Multiplayer.GetRemoteSenderId();
        if (callerId != 0)
        {
            instance.ClearHighlights();
        }

        if (_pieceLayer[to.X][to.Y] != null)
        {
            _pieceLayer[to.X][to.Y].Destroy();
        }
        instance.GridPosition = to;
        _pieceLayer[from.X][from.Y] = null;
        _pieceLayer[to.X][to.Y] = instance;
        HightlightsExist = false;

        Tween tween = GetTree().CreateTween();
        tween.TweenCallback(Callable.From(()=>{_fog.UpdateFog();})).SetDelay(ACGlobal.ANIMATION_TIME_1/2.0);

        Rpc(nameof(HandleOperation), from, to);
    }

    /*
    public void SyncStatusBetweenPlayers(){}
    */

    public void HandlePieceSelection(Vector2I position)
    {
        PieceInstance inst = _pieceLayer[position.X][position.Y];
        if (inst == null || inst.Player != PlayerRole)
            return;

        if (!HightlightsExist)
            inst.CreateHighLights();
        else
            inst.ClearHighlights();
    }

}