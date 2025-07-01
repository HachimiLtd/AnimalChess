using System;
using Godot;

public partial class PieceAttackHighlight : PieceHighlight
{
    public delegate void PAHC(Vector4I param);

    private PAHC _whenClicked;
    private bool _pseudo;

    public void PAHInitialize(PAHC whenClicked, bool pseudo)
    {
        _whenClicked = whenClicked;
        _pseudo = pseudo;
    }

    protected override void SubmitSignal()
    {
        if (_pseudo)
        {
            _whenClicked(Vector4I.Zero);
        }
        else
        {
            _whenClicked(new Vector4I(_gridPosition.X, _gridPosition.Y, 0, 0));
        }
        EmitSignal(SignalName.SubmitMove, _initPosition);
    }
}