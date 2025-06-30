using System;
using Godot;

public partial class PiecePseudoHighlight : PieceHighlight
{
    public override void Destroy()
    {
        _disabled = true;
        Tween tween = GetTree().CreateTween();
        tween.TweenProperty(this,"modulate",new Color(1.0f,.2f,.2f,1.0f),0.08);
        tween.TweenProperty(this,"modulate",new Color(1.0f,1.0f,1.0f,.0f),0.05).SetDelay(0.085);
        tween.TweenCallback(Callable.From(QueueFree)).SetDelay(0.135);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
    }

    protected override void SubmitSignal()
    {
        EmitSignal(SignalName.SubmitMove,_initPosition);
    }

}