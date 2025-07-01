using System;
using Godot;

public partial class PieceSecondHighlight : PieceHighlight
{
    private bool _pseudo = false;
    public void SetPseudo()
    {
        _pseudo = true;
    }

    public override void Destroy()
    {
        _disabled = true;
        if(_pseudo)
        {
            Tween tween = GetTree().CreateTween();
            tween.TweenProperty(this,"modulate",new Color(1.0f,.2f,.2f,1.0f),0.08);
            tween.TweenProperty(this,"modulate",new Color(1.0f,1.0f,1.0f,.0f),0.05).SetDelay(0.085);
            tween.TweenCallback(Callable.From(QueueFree)).SetDelay(0.135);
            return;
        }
        base.Destroy();
    }

    protected override void SubmitSignal()
    {
        if(_pseudo)
            EmitSignal(SignalName.SubmitParam, Vector4I.Zero);
        else
            EmitSignal(SignalName.SubmitParam, new Vector4I(_gridPosition.X, _gridPosition.Y, 0,0));
    }
}