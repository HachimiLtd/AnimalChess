using System;
using System.Collections.Generic;
using Godot;

public abstract partial class PieceInstance : Button
{
    public enum HighlightType
    {
        ERROR,
        NORMAL,
        PSEUDO,
        RAT_TELE,
        PARAM_CANCEL,
        SECOND,
        DETECT,
        SECOND_AFTERATTACK,
        ATTACK,
    }
    protected static Dictionary<HighlightType,PackedScene> _resHighlights = new Dictionary<HighlightType,PackedScene>{
        {HighlightType.NORMAL,(PackedScene)GD.Load("res://scenes/piece_highlight.tscn")},
        {HighlightType.PSEUDO,(PackedScene)GD.Load("res://scenes/piece_pseudo_highlight.tscn")},
        {HighlightType.RAT_TELE,(PackedScene)GD.Load("res://scenes/piece_rat_tele_highlight.tscn")},
        {HighlightType.PARAM_CANCEL,(PackedScene)GD.Load("res://scenes/piece_cancel_highlight.tscn")},
        {HighlightType.SECOND,(PackedScene)GD.Load("res://scenes/piece_second_highlight.tscn")},
        {HighlightType.DETECT,(PackedScene)GD.Load("res://scenes/piece_detect_highlight.tscn")},
        {HighlightType.SECOND_AFTERATTACK,(PackedScene)GD.Load("res://scenes/piece_second_afterattack_highlight.tscn")},
        {HighlightType.ATTACK,(PackedScene)GD.Load("res://scenes/piece_attack_highlight.tscn")},
    };

    protected ChessSystem _system;

    protected readonly PieceType _type; //THIS MUST GET A VALUE IN THE CONSTRUCTOR
    protected RoleType _player;
    protected Vector2I _gridPosition;

    protected List<PieceHighlight> highlights;

    public RoleType Player{ get{return _player;} set{_player=value;} }
    public PieceType Type{ get{return _type;} }
    public Vector2I GridPosition
    {
        get{ return _gridPosition; }
        set
        {
            if(_gridPosition == value)
                return;
            if(IsNodeReady())
            {
                if(Player == _system.PlayerRole)
                    Move(value);
                else if(_system.CheckVisibility(value))
                {
                    if(Visible)
                        Move(value);
                    else
                        MoveInSecretly(value);
                }
                else
                    MoveOutSecretly(value);
            }
            else
                Position = GridSystem.GridToWorld(value);
            _gridPosition = value;
        }
    }

    private bool _unknownStat = false;
    private Tween tweenUnknownStat;
    public bool UnknownStat
    {
        get{return _unknownStat;}
        set
        {
            if(_unknownStat==value)
                return;
            if(tweenUnknownStat != null)
                tweenUnknownStat.Kill();
            if(value)
            {
                _markSpr.Visible = true;
                _animalSpr.Visible = false;
                _markSpr.Modulate = new Color(1.0f,1.0f,1.0f,1.0f);
            }
            else
            {
                _animalSpr.Visible = true;
                _animalSpr.Modulate = new Color(1.0f,1.0f,1.0f,.0f);
                tweenUnknownStat = GetTree().CreateTween();
                tweenUnknownStat.TweenProperty(_animalSpr,"modulate",new Color(1.0f,1.0f,1.0f,1.0f),ACGlobal.ANIMATION_TIME_1);
                tweenUnknownStat.TweenProperty(_markSpr,"modulate",new Color(1.0f,1.0f,1.0f,.0f),ACGlobal.ANIMATION_TIME_1);
                tweenUnknownStat.TweenCallback(Callable.From(()=>{ _markSpr.Visible = false; }));
            }
            _unknownStat = value;
        }
    }
    public void forceToBeKnown()
    {
        _unknownStat = false;
        _markSpr.Visible = false;
        _animalSpr.Visible = true;
        _animalSpr.Modulate = new Color(1.0f,1.0f,1.0f,1.0f);
    }

    private bool _choosable = false;
    public bool Choosable
    {
        get{return _choosable;}
        set
        {
            Disabled = !value;
            _choosable = value;
        }
    }

    private CanvasItem _animalSpr;
    private CanvasItem _markSpr;

    //Temp: Between SubmitMove and SubmitParam
    protected Vector2I _tempLastPosition = Vector2I.Zero;
    protected bool _tempCapturedPiece = false;

    public abstract void CreateHighlights();
    public abstract void CreateParamHighlights();
        //Use CreateHighlight to create special PieceHighlight that emits SubmitParam signal
        //Or, use SkipSubmitParam
    
    public abstract void UpdateDisplay();

    public virtual void ClearAdditionalParamHighlights(){}

    protected PieceHighlight CreateHighlight(Vector2I at,HighlightType type = HighlightType.NORMAL)
    {
        _system.HighlightOwner = this;
        PieceHighlight highlight = (PieceHighlight)_resHighlights[type].Instantiate();

        highlights.Add(highlight);
        _system.MountHightlights.AddChild(highlight);
        highlight.Initialize(_gridPosition,at);
        highlight.SubmitMove += HandleSubmitMove;
        highlight.SubmitParam += HandleSubmitParam;
        return highlight;
    }

    public void ClearHighlights()
    {
        if(_system.Stage == TurnStage.PARAM_DECISION)
            ClearAdditionalParamHighlights();
        foreach(PieceHighlight child in highlights)
        {
            child.ForceDestroy();
        }
        highlights.Clear();
    }


    //Interact with Godot System
    public override void _Ready()
    {
        base._Ready();
        _system = (ChessSystem)GetNode("../..");
        Pressed += HandlePressed;
        _animalSpr = (CanvasItem)GetNode("Label");
        _markSpr = (CanvasItem)GetNode("Mark");
    }

    public void HandlePressed()
    {
        if(!_system.CurrentlyPlaying || !Choosable)
            return;
        _system.HandlePieceSelection(_gridPosition);
    }


    //Interact with ChessSystem
    public void HandleSubmitMove(Vector2I dest)
    {
        ClearHighlights();
        _tempLastPosition = _gridPosition;
        if (_system.PieceLayer[dest.X][dest.Y] != null && _system.PieceLayer[dest.X][dest.Y].Player != Player)
            _tempCapturedPiece = true;
        else
            _tempCapturedPiece = false;
        _system.HandleChessMove(new ChessMove(_gridPosition,dest));
    }

    public void SkipSubmitParam()
    {
        HandleSubmitParam(Vector4I.Zero);
    }
    public void HandleSubmitParam(Vector4I param)
    {
        ClearHighlights();
        _system.HandleChessParam(param);
    }
    
    //Animation
    private Tween _moveTween;
    public void Move(Vector2I to)
    {
        Vector2 toPos = GridSystem.GridToWorld(to);
        if(_moveTween != null)
            _moveTween.Kill();
        _moveTween = GetTree().CreateTween();
        _moveTween.TweenProperty(this, "position", toPos, ACGlobal.ANIMATION_TIME_1);
        _moveTween.TweenCallback(Callable.From(UpdateDisplay)).SetDelay(ACGlobal.ANIMATION_TIME_1/2);
        _gridPosition = to;
    }
    public void MoveOutSecretly(Vector2I to)
    {
        /*if(_unknownStat)
        {
            _animalSpr.Visible = false;
        }*/
        Vector2 toPos = GridSystem.GridToWorld(to);
        if(_moveTween != null)
            _moveTween.Kill();
        _moveTween = GetTree().CreateTween();
        _moveTween.TweenProperty(this, "modulate", new Color(1.0f,1.0f,1.0f,0.0f), ACGlobal.ANIMATION_TIME_1/2);
        _moveTween.TweenCallback(Callable.From(()=>
            {
                Position = toPos;
                Modulate = new Color(1.0f,1.0f,1.0f);
            })).SetDelay(ACGlobal.ANIMATION_TIME_1/1.95);
        /*tween.TweenCallback(Callable.From(()=>
            {
                _animalSpr.Visible = true;
            })).SetDelay(ACGlobal.ANIMATION_TIME_1/0.8);*/
        _gridPosition = to;
    }
    public void MoveInSecretly(Vector2I to)
    {
        UnknownStat = true;

        Vector2 toPos = GridSystem.GridToWorld(to);
        Position = toPos;
        Modulate = new Color(1.0f,1.0f,1.0f,0.0f);
        if(_moveTween != null)
            _moveTween.Kill();
        _moveTween = GetTree().CreateTween();
        _moveTween.TweenProperty(this, "modulate", new Color(1.0f,1.0f,1.0f,1.0f),
            ACGlobal.ANIMATION_TIME_1/2).SetDelay(ACGlobal.ANIMATION_TIME_1/2);
        _gridPosition = to;
    }
    public void Destroy()
    {
        ClearHighlights();
        Tween tween = GetTree().CreateTween();
        tween.TweenProperty(this,"modulate", new Color(1.0f,1.0f,1.0f,0.0f),ACGlobal.ANIMATION_TIME_1/1.5f);
        tween.TweenCallback(Callable.From(QueueFree));
    }

    public PieceInstance(PieceType type)
    {
        _type = type;
        highlights = new List<PieceHighlight>();
    }

    public void SetStableModulate(Color modulate)
    {
        foreach(Node child in GetChildren())
        {
            if(child is CanvasItem){
                ((CanvasItem)child).Modulate = modulate;
            }
        }
    }
}