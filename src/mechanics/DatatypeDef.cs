using System;
using System.Collections.Generic;
using Godot;

public record struct ChessOperation(Vector2I From, Vector2I To);

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
    NEST,
    BOUNDARY,
    //just hate writing ifs everywhere for damn boundary conditions

}

public enum RoleType
{
    P1,
    P2,
}

public static class ACGlobal{
    public const float ANIMATION_TIME_1 = 0.15f;
}