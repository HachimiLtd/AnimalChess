using Godot;

public partial class Lobby : Control
{
  private MultiplayerManager _multiplayer;
  private Button _joinBtn;
  private Button _hostBtn;
  private LineEdit _ipInput;

  public override void _Ready()
  {
    if (!HasNode("/root/MultiplayerManager"))
    {
      _multiplayer = new MultiplayerManager();
      CallDeferred(nameof(AddMultiplayerManager));
      _multiplayer.Name = "MultiplayerManager";
    }
    else
    {
      _multiplayer = GetNode<MultiplayerManager>("/root/MultiplayerManager");
    }
    _joinBtn = GetNode<Button>("Panel/Control/JoinButton");
    _hostBtn = GetNode<Button>("Panel/Control/HostButton");
    _ipInput = GetNode<LineEdit>("Panel/IPInput");

    // Connect multiplayer signals
    _multiplayer.ConnectionSuccessful += OnConnectionSuccessful;
    _multiplayer.ConnectionFailed += OnConnectionFailed;
    _multiplayer.PlayerConnected += OnPlayerConnected;
    _multiplayer.PlayerDisconnected += OnPlayerDisconnected;

    _joinBtn.ButtonDown += OnJoinButtonPressed;
    _hostBtn.ButtonDown += OnHostButtonPressed;
  }

  private void OnJoinButtonPressed()
  {
    GD.Print("Join button pressed");

    // Join game on localhost with default port
    string address = _ipInput.Text;
    int port = 7000;

    if (_multiplayer.JoinGame(address, port))
    {
      GD.Print($"Attempting to join game at {address}:{port}");
    }
    else
    {
      GD.PrintErr("Failed to initiate connection");
    }
  }

  private void OnHostButtonPressed()
  {
    GD.Print("Host button pressed");

    // Host game on default port
    int port = 7000;

    if (_multiplayer.HostGame(port))
    {
      GD.Print($"Successfully hosting game on port {port}");
      // Optionally switch to game scene or show waiting screen
    }
    else
    {
      GD.PrintErr("Failed to host game");
    }
  }

  // Multiplayer event handlers
  private void OnConnectionSuccessful()
  {
    GD.Print("Successfully connected to host!");
    // Switch to game scene
    GetTree().ChangeSceneToFile("res://scenes/world.tscn");
  }

  private void OnConnectionFailed()
  {
    GD.PrintErr("Failed to connect to host");
    // Show error message to user
  }

  private void OnPlayerConnected(long id)
  {
    GD.Print($"Player {id} joined the game");

    // If we're the host and we now have 2 players, start the game
    if (_multiplayer.IsHost && _multiplayer.GetPlayerCount() >= 2)
    {
      GD.Print("Game is ready to start!");
      GetTree().ChangeSceneToFile("res://scenes/world.tscn");
    }
  }

  private void OnPlayerDisconnected(long id)
  {
    GD.Print($"Player {id} left the game");
  }

  private void AddMultiplayerManager()
  {
    GetTree().Root.AddChild(_multiplayer);
  }

  public override void _ExitTree()
  {
    // Disconnect signals to prevent memory leaks
    if (_multiplayer != null)
    {
      _multiplayer.ConnectionSuccessful -= OnConnectionSuccessful;
      _multiplayer.ConnectionFailed -= OnConnectionFailed;
      _multiplayer.PlayerConnected -= OnPlayerConnected;
      _multiplayer.PlayerDisconnected -= OnPlayerDisconnected;
    }
  }
}