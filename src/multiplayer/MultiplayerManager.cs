using Godot;

public partial class MultiplayerManager : Node
{
  [Signal]
  public delegate void ConnectionSuccessfulEventHandler();

  [Signal]
  public delegate void ConnectionFailedEventHandler();

  [Signal]
  public delegate void PlayerConnectedEventHandler(long id);

  [Signal]
  public delegate void PlayerDisconnectedEventHandler(long id);

  [Signal]
  public delegate void ServerDisconnectedEventHandler();
  [Signal]
  public delegate void GameDataReceivedEventHandler(Vector2I from, Vector2I to, long fromId);

  private const int DEFAULT_PORT = 7000;
  private const int MAX_CLIENTS = 10; // For chess, typically 2 players

  private ENetMultiplayerPeer _peer;
  private bool _isHost = false;

  public new bool IsConnected => Multiplayer.HasMultiplayerPeer();
  public bool IsHost => _isHost;
  public long GetUniqueId() => Multiplayer.GetUniqueId();
  public override void _Ready()
  {
    // Connect multiplayer signals
    Multiplayer.PeerConnected += OnPeerConnected;
    Multiplayer.PeerDisconnected += OnPeerDisconnected;
    Multiplayer.ConnectedToServer += OnConnectedToServer;
    Multiplayer.ConnectionFailed += OnConnectionFailed;
    Multiplayer.ServerDisconnected += OnServerDisconnected;
  }

  /// <summary>
  /// Host a game server
  /// </summary>
  /// <param name="port">Port to host on (default: 7000)</param>
  /// <returns>True if hosting was successful</returns>
  public bool HostGame(int port = DEFAULT_PORT)
  {
    _peer = new ENetMultiplayerPeer();
    var error = _peer.CreateServer(port, MAX_CLIENTS);

    if (error != Error.Ok)
    {
      GD.PrintErr($"Failed to create server: {error}");
      _peer.Close();
      _peer = null;
      Multiplayer.MultiplayerPeer = null;
      return false;
    }

    Multiplayer.MultiplayerPeer = _peer;
    _isHost = true;

    GD.Print($"Server started on port {port}, waiting for players...");
    return true;
  }

  /// <summary>
  /// Join a game server
  /// </summary>
  /// <param name="address">Server address to connect to</param>
  /// <param name="port">Server port (default: 7000)</param>
  /// <returns>True if connection attempt was initiated successfully</returns>
  public bool JoinGame(string address, int port = DEFAULT_PORT)
  {
    if (Multiplayer == null)
    {
      GD.PrintErr("Multiplayer system is not initialized");
      return false;
    }

    _peer = new ENetMultiplayerPeer();
    var error = _peer.CreateClient(address, port);

    if (error != Error.Ok)
    {
      GD.PrintErr($"Failed to create client: {error}");
      _peer.Close();
      _peer = null;
      Multiplayer.MultiplayerPeer = null;
      return false;
    }

    Multiplayer.MultiplayerPeer = _peer;
    _isHost = false;

    GD.Print($"Attempting to connect to {address}:{port}...");
    return true;
  }

  /// <summary>
  /// Disconnect from the current game
  /// </summary>
  public void DisconnectFromGame()
  {
    if (_peer != null)
    {
      _peer.Close();
      _peer = null;
    }

    Multiplayer.MultiplayerPeer = null;
    _isHost = false;

    GD.Print("Disconnected from game");
  }

  /// <summary>
  /// Send a chess move to other players
  /// </summary>
  /// <param name="operation">The chess operation to send</param>
  /// <param name="targetId">Target player ID (0 for all players)</param>
  public void SendChessMove(ChessOperation operation, long targetId = 0)
  {
    if (!IsConnected)
    {
      GD.PrintErr("Cannot send move: Not connected to multiplayer session");
      return;
    }

    if (targetId == 0)
    {
      // Send to all peers
      Rpc(nameof(ReceiveChessMove), operation.From, operation.To);
    }
    else
    {
      // Send to specific peer
      RpcId(targetId, nameof(ReceiveChessMove), operation.From, operation.To);
    }
  }

  /// <summary>
  /// Receive a chess move from another player
  /// </summary>
  [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false)]
  private void ReceiveChessMove(Vector2I from, Vector2I to)
  {
    long senderId = GetMultiplayer().GetRemoteSenderId();
    EmitSignal(SignalName.GameDataReceived, from, to, senderId);
    GD.Print($"Received chess move from player {senderId}: {from} -> {to}");
  }    /// <summary>
       /// Send game state synchronization data
       /// </summary>
  public void SyncGameState(Godot.Collections.Dictionary gameState)
  {
    if (!_isHost)
    {
      GD.PrintErr("Only the host can sync game state");
      return;
    }

    Rpc(nameof(ReceiveGameState), gameState);
  }

  /// <summary>
  /// Receive game state synchronization data
  /// </summary>
  [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false)]
  private void ReceiveGameState(Godot.Collections.Dictionary gameState)
  {
    // Handle game state synchronization
    GD.Print("Received game state synchronization");
    // Implementation depends on how you want to handle game state
  }

  /// <summary>
  /// Get list of connected player IDs
  /// </summary>
  public int[] GetConnectedPlayers()
  {
    if (!IsConnected)
      return new int[0];

    return Multiplayer.GetPeers();
  }

  /// <summary>
  /// Get the number of connected players (including self)
  /// </summary>
  public int GetPlayerCount()
  {
    if (!IsConnected)
      return 0;

    return GetConnectedPlayers().Length + 1; // +1 for self
  }

  // Signal handlers
  private void OnPeerConnected(long id)
  {
    GD.Print($"Player {id} connected");
    EmitSignal(SignalName.PlayerConnected, id);

    if (_isHost)
    {
      // Host can send initial game state to new player
      // SyncGameState(GetCurrentGameState());
    }
  }

  private void OnPeerDisconnected(long id)
  {
    GD.Print($"Player {id} disconnected");
    EmitSignal(SignalName.PlayerDisconnected, id);
  }

  private void OnConnectedToServer()
  {
    GD.Print("Successfully connected to server");
    EmitSignal(SignalName.ConnectionSuccessful);
  }

  private void OnConnectionFailed()
  {
    GD.PrintErr("Failed to connect to server");
    EmitSignal(SignalName.ConnectionFailed);
  }

  private void OnServerDisconnected()
  {
    GD.Print("Server disconnected");
    EmitSignal(SignalName.ServerDisconnected);
    DisconnectFromGame();
  }
}