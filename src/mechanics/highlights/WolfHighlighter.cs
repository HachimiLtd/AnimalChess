using Godot;
using System;

public partial class WolfHighlighter : Control
{
    [Signal]
    public delegate void SubmitParamEventHandler(Vector4I position);

    private Sprite2D _indicator;
    private Vector2I _wolfGridPosition;

    private ChessSystem _system;
    
    public void Initialize(ChessSystem system, Vector2I wolfGridPosition)
    {
        _system = system;
        _wolfGridPosition = wolfGridPosition;
    }
    public override void _Ready()
    {
        // Get references to child nodes
        _indicator = GetNode<Sprite2D>("Indicator");
        
        FocusMode = FocusModeEnum.Click;
        GrabFocus();
        GuiInput += HandleInput;
        Position = GridSystem.GridToWorld(new Vector2I(1,1));
        Size = new Vector2(
            GridSystem.GridSize.X*_system.GroundSize.X,
            GridSystem.GridSize.Y*_system.GroundSize.Y);
        MouseExited += NoIndicator;
    }
    public void Destroy()
    {
        QueueFree();
    }

    public void HandleInput(InputEvent @event)
    {
        if (!(@event is InputEventMouse))
            return;
        Vector2 mousePosition = GetGlobalMousePosition();
        mousePosition = _system.MountHightlights.ToLocal(mousePosition);
        if (@event is InputEventMouseMotion motionEvent)
        {
            HandleHover(mousePosition);
        }
        else if (@event is InputEventMouseButton buttonEvent)
        {
            if (buttonEvent.IsPressed() && buttonEvent.ButtonIndex == MouseButton.Left)
            {
                HandlePress(mousePosition);
            }
            else if (buttonEvent.IsReleased())
            {
                HandleClick(mousePosition);
            }
        }
    }
    private Vector2I _currentGrid = new Vector2I(0,0);
    private bool _consistentPress = false;
    
    public void NoIndicator()
    {
        _indicator.Frame = 0;
        _consistentPress = false;
    }
    private void HandleHover(Vector2 mousePosition)
    {
        Vector2I gridPosition = GridSystem.WorldToGrid(mousePosition);
        if( gridPosition != _currentGrid)
        {
            _currentGrid = gridPosition;
            _consistentPress = false; // Reset consistent press when grid position changes
        }

        if ( !IsSkillAllowed(gridPosition) )
        {
            _indicator.Frame = 0; // Reset frame when hovering over the cancel button
        }
        else
        {
            if(Input.IsActionPressed("ui_left"))
                _indicator.Frame = 2;
            else
                _indicator.Frame = 1; // Hover frame
            _indicator.Position = GridSystem.GridToWorld(gridPosition);
        }
    }

    private void HandlePress(Vector2 mousePosition)
    {
        Vector2I gridPosition = GridSystem.WorldToGrid(mousePosition);
        _currentGrid = gridPosition;

        if ( IsSkillAllowed(gridPosition) )
        {
            _indicator.Frame = 2;
            _consistentPress = true;
            _indicator.Position = GridSystem.GridToWorld(gridPosition);
            AcceptEvent();
        }
    }

    private void HandleClick(Vector2 mousePosition)
    {
        Vector2I gridPosition = GridSystem.WorldToGrid(mousePosition);
        
        if ( !IsSkillAllowed(gridPosition) )
        {
            _currentGrid = gridPosition;
            _consistentPress = false;
            return;
        }
        AcceptEvent();

        if(!_consistentPress)
        {
            _currentGrid = gridPosition;
            _consistentPress = false;
            return;
        }
        Vector4I encodedPosition = new Vector4I(gridPosition.X, gridPosition.Y, 0, 0);
        EmitSignal(nameof(SubmitParam), encodedPosition);
    }

    private bool IsSkillAllowed(Vector2I gridPosition)
    {
        return (!_system.IsGridVisible(gridPosition)) && (gridPosition != _wolfGridPosition);
    }
}
