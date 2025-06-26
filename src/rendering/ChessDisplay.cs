using System;
using Godot;

public static class GridSystem
{
    public static Vector2 GridSize = new(48,48);
    public static Vector2 GridToWorld( Vector2I pos )
    {
        return new Vector2( (pos.X-1)*GridSize.X, (pos.Y-1)*GridSize.Y );
    }
}