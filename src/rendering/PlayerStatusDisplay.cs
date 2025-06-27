using System;
using Godot;

public partial class PlayerStatusDisplay : Sprite2D
{
    public void SetWait()
    {
        Frame = 2;
    }

    public void SetPlayerPlay()
    {
        Frame = 0;
    }

    public void SetOtherPlay()
    {
        Frame = 1;
    }
}