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
    }
    protected static Dictionary<HighlightType,PackedScene> _resHightlights = new Dictionary<HighlightType,PackedScene>{
        {HighlightType.NORMAL,(PackedScene)GD.Load("res://scenes/piece_highlight.tscn")},
        {HighlightType.PSEUDO,(PackedScene)GD.Load("res://scenes/piece_pseudo_highlight.tscn")}
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

    public abstract void CreateHighlights();
    public abstract void CreateParamHighlights();
        //Use CreateHighlight to create special PieceHighlight that emits SubmitParam signal
        //Or, use SkipSubmitParam
    
    public abstract void UpdateDisplay();

    //public abstract void CreateSpecialHighlights();

    protected void CreateHighlight(Vector2I at,HighlightType type = HighlightType.NORMAL)
    {
        _system.HighlightOwner = this;
        PieceHighlight highlight = (PieceHighlight)_resHightlights[type].Instantiate();

        highlights.Add(highlight);
        _system.MountHightlights.AddChild(highlight);
        highlight.Initialize(_gridPosition,at);
        highlight.SubmitMove += HandleSubmitMove;
        highlight.SubmitParam += HandleSubmitParam;
    }

    public void ClearHighlights()
    {
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
    public void Move(Vector2I to)
    {
        Vector2 toPos = GridSystem.GridToWorld(to);
        Tween tween = GetTree().CreateTween();
        tween.TweenProperty(this, "position", toPos, ACGlobal.ANIMATION_TIME_1);
        tween.TweenCallback(Callable.From(UpdateDisplay)).SetDelay(ACGlobal.ANIMATION_TIME_1/2);
        _gridPosition = to;
    }
    public void MoveOutSecretly(Vector2I to)
    {
        /*if(_unknownStat)
        {
            _animalSpr.Visible = false;
        }*/
        Vector2 toPos = GridSystem.GridToWorld(to);
        Tween tween = GetTree().CreateTween();
        tween.TweenProperty(this, "modulate", new Color(1.0f,1.0f,1.0f,0.0f), ACGlobal.ANIMATION_TIME_1/2);
        tween.TweenCallback(Callable.From(()=>
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
        Tween tween = GetTree().CreateTween();
        tween.TweenProperty(this, "modulate", new Color(1.0f,1.0f,1.0f,1.0f),
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
}