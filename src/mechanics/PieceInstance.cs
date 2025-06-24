using System;
using System.Collections.Generic;
using Godot;

public abstract partial class PieceInstance : Button
{
    protected static PackedScene _resPieceHighlight = (PackedScene)GD.Load("res://scenes/piece_highlight.tscn");

    protected ChessSystem _system;

    protected readonly PieceType _type; //THIS MUST GET A VALUE IN THE CONSTRUCTOR
    protected RoleType _player;
    protected Vector2I _gridPosition;

    public RoleType Player{ get{return _player;} set{_player=value;} }
    public PieceType Type{ get{return _type;} }
    public Vector2I GridPosition
    {
        get{ return _gridPosition; }
        set
        {
            if(IsNodeReady())
                Move(value);
            else
                Position = GridSystem.GridToWorld(value);
            _gridPosition = value;
        }
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

    public abstract void CreateHighLights();
    public abstract void UpdateDisplay();

    protected void CreateHighlight( Vector2I at )
    {
        _system.HightlightsExist = true;
        PieceHighlight highlight = (PieceHighlight)_resPieceHighlight.Instantiate();

        _system.MountHightlights.AddChild(highlight);
        highlight.Initialize(_gridPosition,at);
        highlight.SubmitMove += HandleSubmitMove;
    }

    public void ClearHighlights()
    {
        _system.HightlightsExist = false;
        foreach(Node child in GetChildren())
        {
            if(child is PieceHighlight)
                ((PieceHighlight)child).Destroy();
        }
    }


    //Interact with Godot System
    public override void _Ready()
    {
        base._Ready();
        _system = (ChessSystem)GetNode("../..");
        Pressed += HandlePressed;
    }

    public void HandlePressed()
    {
        if(!_system.CurrentlyPlaying && !Choosable)
            return;
        _system.HandlePieceSelection(_gridPosition);
    }


    //Interact with ChessSystem
    public void HandleSubmitMove(Vector2I dest)
    {
        ClearHighlights();
        _system.HandleOperation(new ChessOperation(_gridPosition,dest));
    }

    
    //Animation
    public void Move(Vector2I to)
    {
        Vector2 toPos = GridSystem.GridToWorld(to);
        Tween tween = GetTree().CreateTween();
        tween.TweenProperty(this, "position", toPos, 0.2f);
        tween.TweenCallback(Callable.From(UpdateDisplay));
        _gridPosition = to;
    }
    public void Destroy()
    {
        ClearHighlights();
        Tween tween = GetTree().CreateTween();
        tween.TweenProperty(this,"modulate", new Color(1.0f,1.0f,1.0f,0.0f),0.1f);
        tween.TweenCallback(Callable.From(QueueFree));
    }
}