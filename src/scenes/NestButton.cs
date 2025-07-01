using System;
using Godot;

public partial class NestButton : Button
{
    [Export]
    private NestButton _partner;
    private Vector2I _gridPosition;
    private bool _selected = false;

    private Sprite2D _spr;
    public bool Selected
    {
        get => _selected;
        set
        {
            _selected = value;
            if (_selected)
            {
                _spr.Frame = 0 + (_selected ? 3 : 0);
            }
            else
            {
                _spr.Frame = 0 + (_selected ? 3 : 0);
            }
        }
    }
    
    public NestButton()
    {
        Pressed += HandlePressed;
        ButtonDown += HandleButtonDown;
        MouseExited += HandleMouseExited;
        MouseEntered += HandleMouseEnter;
    }
    public void Initialize(Vector2I pos)
    {
        _gridPosition = pos;
        Position = GridSystem.GridToWorld(pos);
    }

    public override void _Ready()
    {
        base._Ready();
        _spr = (Sprite2D)GetNode("Spr");
    }

    public void HandlePressed()
    {
        _spr.Frame = 2 + (_selected?3:0);
        Selected = true;
        _partner.Selected = false;
    }
    
    public void HandleButtonDown()
    {
        _spr.Frame = 2 + (_selected?3:0);
    }
    public void HandleMouseEnter()
    {
        _spr.Frame = 1 + (_selected?3:0);
    }
    public void HandleMouseExited()
    {
        _spr.Frame = 0 + (_selected?3:0);
    }

}