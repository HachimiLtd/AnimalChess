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
    [Export]
    private PlayerStatusDisplay _playerStatusDisplayP1;
    [Export]
    private PlayerStatusDisplay _playerStatusDisplayP2;

    // Constants for board dimensions
    private const int BOARD_WIDTH = 12;
    private const int BOARD_HEIGHT = 12;

    //private static PackedScene _resPieceInstance = (PackedScene)GD.Load("res://scenes/piece_instance.tscn");
    private static Dictionary<PieceType, PackedScene> _resPieceInstances;
    static ChessSystem()
    {
        _resPieceInstances = new Dictionary<PieceType, PackedScene>
        {
            [PieceType.RAT] = (PackedScene)GD.Load("res://scenes/pieces/piece_rat.tscn"),
            [PieceType.CAT] = (PackedScene)GD.Load("res://scenes/pieces/piece_cat.tscn"),
            [PieceType.DOG] = (PackedScene)GD.Load("res://scenes/pieces/piece_dog.tscn"),
            [PieceType.WOLF] = (PackedScene)GD.Load("res://scenes/pieces/piece_wolf.tscn"),
            [PieceType.LEOPARD] = (PackedScene)GD.Load("res://scenes/pieces/piece_leopard.tscn"),
            [PieceType.TIGER] = (PackedScene)GD.Load("res://scenes/pieces/piece_tiger.tscn"),
            [PieceType.LION] = (PackedScene)GD.Load("res://scenes/pieces/piece_lion.tscn"),
            [PieceType.ELEPHANT] = (PackedScene)GD.Load("res://scenes/pieces/piece_elephant.tscn")
        };
    }

    private Vector2I _groundSize;
    private GroundType[][] _groundLayer;
    private PieceInstance[][] _pieceLayer;
    private RoleType[][] _roleArrangement;
    //private List<PieceInstance> _pieceInstanceList;
    public Vector2I GroundSize { get { return _groundSize; } set { _groundSize = value; } }
    public GroundType[][] GroundLayer { get { return _groundLayer; } set { _groundLayer = value; } }
    public PieceInstance[][] PieceLayer { get { return _pieceLayer; } set { _pieceLayer = value; } }
    public RoleType[][] RoleArrangement { get { return _roleArrangement; } set { _roleArrangement = value; } }

    private ChessBoard _chessBoard;
    private FogControl _fog;
    private ChessProcessControl _control;
    public Node2D MountHightlights;
    public Node2D MountPieces;

    public RoleType PlayerRole;
    public bool CurrentlyPlaying = false;

    //public bool HightlightsExist = false;
    public PieceInstance HighlightOwner = null;


    ChessSystem()
    {
        _pieceLayer = new PieceInstance[24][];
        _groundLayer = new GroundType[24][];
        _roleArrangement = new RoleType[24][];
        for (int i = 0; i < 24; i++)
        {
            _pieceLayer[i] = new PieceInstance[24];
            _groundLayer[i] = new GroundType[24];
            _roleArrangement[i] = new RoleType[24];
        }
    }

    public override void _Ready()
    {
        base._Ready();
        MountHightlights = (Node2D)GetNode("MountHightlights");
        MountPieces = (Node2D)GetNode("MountPieces");
        _chessBoard = (ChessBoard)GetNode("ChessBoard");
        _fog = (FogControl)GetNode("FogPanel");
        _control = (ChessProcessControl)GetNode("Controler");
        _control.PlayerStatusDisplayP1 = _playerStatusDisplayP1;
        _control.PlayerStatusDisplayP2 = _playerStatusDisplayP2;


        ChessPieceInitialArrangement arr = CreateInitialArrangement();
        RoleType role = IsMultiplayerAuthority() ? RoleType.P1 : RoleType.P2;
        GD.Print(role);

        GameInit(arr, role);
    }

    private ChessPieceInitialArrangement CreateInitialArrangement()
    {
        var arrangement = new ChessPieceInitialArrangement();

        arrangement.typeMap = new PieceType[BOARD_HEIGHT][];
        for (int i = 0; i < BOARD_HEIGHT; i++)
        {
            arrangement.typeMap[i] = new PieceType[BOARD_WIDTH];
            for (int j = 0; j < BOARD_WIDTH; j++)
            {
                arrangement.typeMap[i][j] = PieceType.EMPTY;
            }
        }

        for (int i = 0; i < BOARD_HEIGHT; i++)
        {
            for (int j = 0; j < BOARD_WIDTH; j++)
            {
                _roleArrangement[i][j] = i >= 6 ? RoleType.P2 : RoleType.P1;
            }
        }
        arrangement.roleMap = _roleArrangement;

        arrangement.typeMap[0][0] = PieceType.WOLF;
        arrangement.typeMap[2][0] = PieceType.LION;
        arrangement.typeMap[1][1] = PieceType.DOG;
        arrangement.typeMap[2][2] = PieceType.RAT;
        arrangement.typeMap[2][4] = PieceType.LEOPARD;
        arrangement.typeMap[2][7] = PieceType.CAT;
        arrangement.typeMap[2][9] = PieceType.ELEPHANT;
        arrangement.typeMap[1][10] = PieceType.DOG;
        arrangement.typeMap[0][11] = PieceType.WOLF;
        arrangement.typeMap[2][11] = PieceType.TIGER;
        // and the corresponding ones on the other side
        arrangement.typeMap[11][11] = PieceType.WOLF;
        arrangement.typeMap[9][11] = PieceType.LION;
        arrangement.typeMap[10][10] = PieceType.DOG;
        arrangement.typeMap[9][9] = PieceType.RAT;
        arrangement.typeMap[9][7] = PieceType.LEOPARD;
        arrangement.typeMap[9][4] = PieceType.CAT;
        arrangement.typeMap[9][2] = PieceType.ELEPHANT;
        arrangement.typeMap[10][1] = PieceType.DOG;
        arrangement.typeMap[11][0] = PieceType.WOLF;
        arrangement.typeMap[9][0] = PieceType.TIGER;

        return arrangement;
    }

    public void GameInit(ChessPieceInitialArrangement pieceArrangement, RoleType player)
    {
        for (int i = 1; i <= _groundSize.X; i++)
            for (int j = 1; j <= _groundSize.Y; j++)
            {
                DestroyPieceInstance(new Vector2I(i, j));
            }
        foreach (var child in MountHightlights.GetChildren())
            child.QueueFree();

        _chessBoard.LoadLayers(this);
        PlayerRole = player;
        for (int i = 0; i < _groundSize.X; i++)
            for (int j = 0; j < _groundSize.Y; j++)
            {
                if (pieceArrangement.typeMap[i][j] == PieceType.EMPTY)
                    continue;
                CreatePieceInstance(new Vector2I(i + 1, j + 1), pieceArrangement.roleMap[i][j], pieceArrangement.typeMap[i][j]);
            }

        HighlightOwner = null;
        _fog.UpdateFogData();
        _fog.UpdateFog();
        _control.GameInit();
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

    public void ActChessMove(Vector2I from, Vector2I to)
    {
        PieceInstance instance = _pieceLayer[from.X][from.Y];
        if (instance == null)
            ///////////////////THIS SHOULDN'T HAPPEN//////////////////////
            return;
        int callerId = Multiplayer.GetRemoteSenderId();
        
        if(instance == HighlightOwner)
        {
            instance.ClearHighlights();
            HighlightOwner = null;
        }

        if(from != to)
        {
            if (_pieceLayer[to.X][to.Y] != null)
            {
                if(_pieceLayer[to.X][to.Y]==HighlightOwner)
                    HighlightOwner = null;
                _pieceLayer[to.X][to.Y].Destroy();
            }
            _pieceLayer[from.X][from.Y] = null;
            _pieceLayer[to.X][to.Y] = instance;
            _fog.UpdateFogData();

            instance.GridPosition = to;
        }
        else
        {
            _fog.UpdateFogData();
        }

        for(int i=1;i<=_groundSize.X;i++)
            for(int j=1;j<=_groundSize.Y;j++)
            {
                PieceInstance inst = _pieceLayer[i][j];
                if(inst==null || !inst.Visible)
                    continue;
                if(inst.Visible == true && !CheckVisibility(new Vector2I(i,j)))
                    continue;
                inst.UnknownStat = false;
            }

        Tween tween = GetTree().CreateTween();
        tween.TweenCallback(Callable.From(() => { _fog.UpdateFog(); })).SetDelay(ACGlobal.ANIMATION_TIME_1 / 2.0);
    }
    public void HandleChessMove(ChessMove move)
    {
        ActChessMove(move.From, move.To);
        _control.SwitchStageParam(move);
    }

    public void HandleChessParam(Vector4I param)
    {
        _control.SwitchStageWait(param);
    }

    //////////////// TODO: Special skill effects(param) should ALWAYS be processed in this func. //////////////.
    public void HandleOperation(ChessOperation operation)
    {
        Vector2I from = operation.From;
        Vector2I to = operation.To;
        Vector4I param = operation.param;
        
        if(_control.Stage == TurnStage.WAITING)
        {
            //PieceInstance instance = _pieceLayer[to.X][to.Y];
            ActChessMove(from,to);
            _control.SwitchStageMove();
        }
        else
        {
            PieceInstance instance = _pieceLayer[from.X][from.Y];
            Rpc(nameof(HandleOperationEncode), from, to, param);
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    public void HandleOperationEncode(Vector2I from, Vector2I to, Vector4I param)
    {
        HandleOperation(new ChessOperation(from,to,param));
    }
    public bool CheckVisibility(Vector2I pos)
    {
        return _fog.CheckVisibility(pos);
    }

    public void HandlePieceSelection(Vector2I position)
    {
        PieceInstance inst = _pieceLayer[position.X][position.Y];
        if ( inst == null ||
            _control.Stage != TurnStage.MOVE_DECISION ||
            !CurrentlyPlaying ||
            inst.Player != PlayerRole )
                return;

        if (HighlightOwner == null)
            inst.CreateHighlights();
        else
        {
            HighlightOwner.ClearHighlights();
            HighlightOwner = null;
        }
    }

    public bool IsGridKnown(Vector2I pos)
    {
        PieceInstance instance = _pieceLayer[pos.X][pos.Y];
        if(instance == null)
            return false;
        if(_fog.CheckVisibility(pos))
            return !instance.UnknownStat;
        return false;
    }
}