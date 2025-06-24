using System;
using Godot;

public partial class PieceHighlight : Button
{
    [Signal]
    public delegate void SubmitMoveEventHandler( Vector2I target );

    private Vector2I _gridPosition;
    private Vector2 _dest;
    private double _timepassed = .0;
    private bool _ready = false;
    private bool _disabled = false;
    private double _totalDist;

    private Sprite2D _spr;
    
    public PieceHighlight()
    {
        Pressed += HandlePressed;
        ButtonDown += HandleButtonDown;
        FocusExited += HandleFocusExited;
        FocusEntered += HandleFocusEnter;
    }
    public void Initialize(Vector2I pos,Vector2I dest)
    {
        Position = GridSystem.GridToWorld(pos);
        _gridPosition = dest;
        _dest = GridSystem.GridToWorld(dest);
        _totalDist = (Position-_dest).Length();
    }

    public void Destroy()
    {
        _disabled = true;
    }

    public override void _Ready()
    {
        base._Ready();
        _spr = (Sprite2D)GetNode("Spr");
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if(_ready)
            return;
        
        _timepassed += delta*60.0;
        if(_timepassed > 5.998 || _totalDist < 2.0)
        {
            Position = _dest;
            _ready = true;
            return;
        }
        
        double ratio = Math.Pow(0.1,delta/6.0);
        Position = Position.MoveToward(_dest,(float)(_totalDist*(1-ratio)));
        _totalDist *= ratio;
    }

    public void HandlePressed()
    {   
        _spr.Frame = 2;
        if(_ready && !_disabled)
            EmitSignal(SignalName.SubmitMove,_gridPosition);
    }
    
    public void HandleButtonDown()
    {
        _spr.Frame = 2;
    }
    public void HandleFocusEnter()
    {  
        _spr.Frame = 1;
    }
    public void HandleFocusExited()
    {
        _spr.Frame = 0;
    }

}