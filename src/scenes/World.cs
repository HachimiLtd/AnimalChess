using Godot;

public partial class World : Node
{
  public override void _Ready()
  {
    // Initialize the world scene
    GD.Print("World scene is ready");

    // You can add more initialization logic here
  }

  public void HandleAlwaysCentered()
  {
    CenterContainer centerContainer = GetNode<CenterContainer>("CenterContainer");
    Rect2 visibleRect = GetWindow().GetVisibleRect();
    Vector2 worldSize = visibleRect.Size;
    centerContainer.Size = worldSize;
  }

  public override void _Process(double delta)
  {
    // Update logic for the world scene
    // This method is called every frame

    HandleAlwaysCentered();
  }
}