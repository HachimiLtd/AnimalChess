using System;
using Godot;

public static class GridSystem
{
    public static Vector2 GridSize = new(48,48);
    public static Vector2 GridToWorld( Vector2I pos )
    {
        return new Vector2( (pos.X-1)*GridSize.X, (pos.Y-1)*GridSize.Y );
    }

    public static Vector2I WorldToGrid( Vector2 pos )
    {
        return new Vector2I(
            Mathf.FloorToInt(pos.X / GridSize.X) + 1,
            Mathf.FloorToInt(pos.Y / GridSize.Y) + 1
        );
    }
}