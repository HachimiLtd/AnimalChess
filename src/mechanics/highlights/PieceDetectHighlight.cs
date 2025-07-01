using System;
using Godot;

public partial class PieceDetectHighlight : PieceHighlight
{
    public delegate void PDHC(); //PieceDetectHighlightClicked
    private PDHC _whenClicked = () => { };
    private ChessSystem _system;

    public void PDHInitialize(PDHC whenClicked,ChessSystem system)
    {
        _whenClicked = whenClicked;
        _system = system;

        Vector2I dir = _gridPosition - _initPosition;
        Sprite2D sprite = (Sprite2D)GetNode("Spr");
        if (dir.X>0){
            sprite.Rotation = Mathf.Pi / 2;
            sprite.Position = new Vector2(GridSystem.GridSize.X,0);
        }
        else if (dir.X<0){
            sprite.Rotation = -Mathf.Pi / 2;
            sprite.Position = new Vector2(0,GridSystem.GridSize.Y);
        }
        else if (dir.Y<0)
            sprite.Rotation = 0;
        else if (dir.Y>0){
            sprite.Rotation = Mathf.Pi;
            sprite.Position = new Vector2(GridSystem.GridSize.X,GridSystem.GridSize.Y);
        }
    }

    private Vector2I FindFirstPiece()
    {
        Vector2I dir = _gridPosition - _initPosition;
        Vector2I current = _initPosition + dir;
        while (_system.GroundLayer[current.X][current.Y]!=GroundType.BOUNDARY)
        {
            if (_system.PieceLayer[current.X][current.Y] != null)
                return current;
            current += dir;
        }
        return Vector2I.Zero;
    }

    protected override void SubmitSignal()
    {
        _whenClicked();
        Vector2I firstPiece = FindFirstPiece();
        EmitSignal(SignalName.SubmitParam, new Vector4I(firstPiece.X, firstPiece.Y,0,0));
    }
}