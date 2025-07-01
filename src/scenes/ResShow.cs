using Godot;
using System;

public partial class ResShow : Window
{
    public override void _Ready()
    {
        base._Ready();
        GetNode<Label>("Lwin").Visible = false;
        GetNode<Label>("Lerr").Visible = false;
        GetNode<Label>("Llose").Visible = false;
        CloseRequested += Exit;
        WindowInput += (InputEvent @event) =>
        {
            if (@event is InputEventMouseButton buttonEvent && buttonEvent.IsPressed() && buttonEvent.ButtonIndex == MouseButton.Left)
            {
                Exit();
            }
        };
    }

    public void ShowRes( int res )
    {
        switch(res)
        {
            case 1:
                GetNode<Label>("Lwin").Visible = true;
                break;
            case 0:
                GetNode<Label>("Lerr").Visible = true;
                break;
            case -1:
                GetNode<Label>("Llose").Visible = true;
                break;
        }
        PopupCentered();
    }

    public void Exit()
    {
        GetTree().Quit();
    }
}
