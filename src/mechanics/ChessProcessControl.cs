using System;
using Godot;

public partial class ChessProcessControl : Node
{
    public PlayerStatusDisplay PlayerStatusDisplayP1;
    public PlayerStatusDisplay PlayerStatusDisplayP2;

    private bool _ended = false;

    [Export]
    private ChessSystem _system;
    private MultiplayerManager _multiplayer;

    public TurnStage Stage = TurnStage.ENDED;

    public void GameInit()
    {
        Stage = TurnStage.INITWAIT;
    }

    public override void _Ready()
    {
        base._Ready();

        _multiplayer = (MultiplayerManager)GetNode("/root/MultiplayerManager");
        // rand = new Random((int)Time.GetUnixTimeFromSystem());
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if(_ended)
            return;
        
        switch (Stage)
        {
            case TurnStage.ENDED:
                return;
            case TurnStage.INITWAIT:
                if (_multiplayer.GetPlayerCount() >= 2)
                {
                    Stage = TurnStage.ARRANGEWAIT;
                    // SubmitCommonRandomIntRequest(65537);
                }
                break;
            case TurnStage.ARRANGEWAIT:
                break;
            case TurnStage.INIT:
                var opponentId = _multiplayer.GetConnectedPlayers()[0];
                var selfId = _multiplayer.GetUniqueId();
                var commonInt = opponentId * selfId;

                if (commonInt % 2 == ((_system.PlayerRole == RoleType.P1) ? 0 : 1))
                {
                    SwitchStageMove();
                }
                else
                {
                    SwitchStageWait();
                }
                break;
            case TurnStage.LOOPEND:
                SwitchStageMove();
                break;
            case TurnStage.ERROR:
                GD.Print("?");
                Stage = TurnStage.ENDED;
                return;
        }
    }

    public void SwitchStageInit()
    {
        Stage = TurnStage.INIT;
    }

    public void SwitchStageEnd()
    {
        Stage = TurnStage.ENDED;
        _ended = true;
    }

    public void SwitchStageMove()
    {
        Stage = TurnStage.MOVE_DECISION;
        _system.CurrentlyPlaying = true;

        if (_system.PlayerRole == RoleType.P2)
        {
            PlayerStatusDisplayP1.SetWait();
            PlayerStatusDisplayP2.SetPlayerPlay();
        }
        else
        {
            PlayerStatusDisplayP2.SetWait();
            PlayerStatusDisplayP1.SetPlayerPlay();
        }
    }

    ChessMove tempMove;
    public void SwitchStageParam(ChessMove move,bool pass=false)
    {
        Stage = TurnStage.PARAM_DECISION;
        tempMove = move;

        if(pass)
        {
            SwitchStageWait(Vector4I.Zero);
            return;
        }
        
        PieceInstance instance = _system.PieceLayer[move.To.X][move.To.Y];
        instance.CreateParamHighlights();
    }

    public void SwitchStageWait(Vector4I param)
    {
        _system.HandleOperation(new ChessOperation(tempMove.From, tempMove.To, param));
        SwitchStageWait();
    }
    public void SwitchStageWait()
    {
        Stage = TurnStage.WAITING;
        _system.CurrentlyPlaying = false;

        if (_system.PlayerRole == RoleType.P1)
        {
            PlayerStatusDisplayP1.SetWait();
            PlayerStatusDisplayP2.SetOtherPlay();
        }
        else
        {
            PlayerStatusDisplayP2.SetWait();
            PlayerStatusDisplayP1.SetOtherPlay();
        }
    }


    //Common Random Integer Func
    // public int RandomIntSyncSymbol = 1;
    // private int _CRIsym;
    // private int _CRIa;
    // private int _CRIb;
    // Random rand;
    // public void SubmitCommonRandomIntRequest(int syncSymbol)
    // {
    //     _CRIsym = syncSymbol;
    //     RandomIntSyncSymbol *= -syncSymbol;
    //     _CRIa = rand.Next(1048576010) + 1;
    //     Rpc(nameof(CommonRandomIntInternal), _CRIa);
    // }
    // public int GetCommonRandomInt()
    // {
    //     RandomIntSyncSymbol /= _CRIsym;
    //     UInt64 cria = (ulong)_CRIa;
    //     UInt64 crib = (ulong)_CRIb;
    //     return (int)(cria * crib % 104857601) - 1;
    // }
    // [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    // public void CommonRandomIntInternal(int CRIb)
    // {
    //     _CRIb = CRIb;
    //     RandomIntSyncSymbol *= -1;
    // }
}