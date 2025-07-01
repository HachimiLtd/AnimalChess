using System;
using Godot;

public partial class PieceRatTeleHighlight : PieceHighlight
{
    public delegate void PRTHC(); //PieceRatTeleHighlightClicked
    private PRTHC _whenClicked = () => { };
    bool _pseudo = false;
    public void PRTHInitialize(PRTHC whenClicked,bool pseudo = false)
    {
        _pseudo = pseudo;
        _whenClicked = whenClicked;
    }
    protected override void SubmitSignal()
    {
        _whenClicked();
        if (_pseudo)
        {
            EmitSignal(SignalName.SubmitMove, _initPosition);
            return;
        }
        EmitSignal(SignalName.SubmitMove, _gridPosition);
    }
}