using System;
using Godot;

public partial class PieceCancelHighlight : PieceHighlight
{
    protected override void SubmitSignal()
    {
        EmitSignal(SignalName.SubmitParam, Vector4I.Zero);
    }
}