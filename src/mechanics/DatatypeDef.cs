using System;
using System.Collections.Generic;
using Godot;

public record struct ChessOperation(Vector2I From, Vector2I To, Vector4I param);
public record struct ChessMove(Vector2I From, Vector2I To);

public struct ChessPieceInitialArrangement
{
    public PieceType[][] typeMap;
    public RoleType[][] roleMap;
}

public enum PieceType
{
    EMPTY = 0,
    UNKNOWN,
    RAT = 32,
    CAT = 33,
    DOG = 34,
    WOLF = 35,
    LEOPARD = 36,
    TIGER = 37,
    LION = 38,
    ELEPHANT = 39,
}

public enum GroundType
{
    NORMAL,
    FLOODED,
    TRAP,
    NEST, //Nest precursor
    NEST_REAL,
    NEST_FAKE,
    BOUNDARY,
    //just hate writing ifs everywhere for damn boundary conditions

}

public enum RoleType
{
    P1,
    P2,
}

public enum EndingType
{
    ERROR,
    P1W,
    P2W,
    DRAW,
}

public enum TurnStage
{
    INITWAIT,
    ARRANGEWAIT,
    INIT,
    LOOPHEAD,
    MOVE_DECISION,
    PARAM_DECISION,
    WAITING,
    LOOPEND,
    ENDED = 128,
    ERROR = -1,
}

public static class ACGlobal{
    public const float ANIMATION_TIME_1 = 0.15f;

    public static Color GetPlayerColor(RoleType player)
    {
        if(player==RoleType.P2)
            return new Color(0.8f,0.6f,1.2f);
        return new Color(1.2f,.5f,.6f);
    }
}