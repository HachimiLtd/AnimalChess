using System;
using System.Collections.Generic;
using Godot;

public record struct ChessOperation(Vector2I From, Vector2I To);

public enum PieceType
{
    EMPTY,
    UNKNOWN,
    RAT,
    CAT,
    DOG,
    WOLF,
    LEOPARD,
    TIGER,
    LION,
    ELEPHANT,
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