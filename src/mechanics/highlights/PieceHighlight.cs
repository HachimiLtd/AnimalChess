using System;
using Godot;

public partial class PieceHighlight : Button
{
    [Signal]
    public delegate void SubmitMoveEventHandler( Vector2I target );
    [Signal]
    public delegate void SubmitParamEventHandler( Vector4I param );

    protected Vector2I _initPosition;
    protected Vector2I _gridPosition;
    protected Vector2 _dest;
    private double _timepassed = .0;
    protected bool _ready = false;
    protected bool _disabled = false;
    private double _totalDist;

    private Sprite2D _spr;
    
    public PieceHighlight()
    {
        Pressed += HandlePressed;
        ButtonDown += HandleButtonDown;
        MouseExited += HandleMouseExited;
        MouseEntered += HandleMouseEnter;
    }
    public void Initialize(Vector2I pos,Vector2I dest)
    {
        Position = GridSystem.GridToWorld(pos);
        _initPosition = pos;
        _gridPosition = dest;
        _dest = GridSystem.GridToWorld(dest);
        _totalDist = (Position-_dest).Length();
        Disabled = false;

        Tween tween = GetTree().CreateTween();
        tween.TweenProperty(this,"position",_dest,0.05);
        tween.TweenCallback(Callable.From(TweenSetReady)).SetDelay(0.05);
    }
    public void TweenSetReady()
    {
        _ready = true;
    }

    public virtual void Destroy()
    {
        _disabled = true;
        Tween tween = GetTree().CreateTween();
        tween.TweenProperty(this,"modulate",new Color(1.0f,1.0f,1.0f,0.0f),0.1);
        tween.TweenCallback(Callable.From(QueueFree)).SetDelay(0.1);
    }

    public override void _Ready()
    {
        base._Ready();
        _spr = (Sprite2D)GetNode("Spr");
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
    }

    protected virtual void SubmitSignal()
    {
        EmitSignal(SignalName.SubmitMove,_gridPosition);
    }

    public void HandlePressed()
    {
        if(!_ready)
            return;
        _spr.Frame = 2;
        if(!_disabled)
            SubmitSignal();
    }
    
    public void HandleButtonDown()
    {
        if(!_ready)
            return;
        _spr.Frame = 2;
    }
    public void HandleMouseEnter()
    {
        if(!_ready)
            return;
        _spr.Frame = 1;
    }
    public void HandleMouseExited()
    {
        if(!_ready)
            return;
        _spr.Frame = 0;
    }

}