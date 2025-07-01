using Godot;
using Godot.Collections;

public partial class Arrange : Node
{
  [Export]
  private GridContainer _gridContainer;
  [Export]
  private Button _doneButton;
  [Export]
  private Panel _fogPanel;
  [Export]
  private Label _waitingLabel;
  private PackedScene _draggablePieceScene = (PackedScene)GD.Load("res://scenes/draggable_piece.tscn");
  private PackedScene _worldScene = (PackedScene)GD.Load("res://scenes/world.tscn");
  private World _world;
  private ChessSystem _system;
  public ChessSystem System { get { return _system; } set { _system = value; } }
  private ChessProcessControl _control;
  public ChessProcessControl Control { get { return _control; } set { _control = value; } }

  private DraggablePiece[][] _draggablePieces;
  public DraggablePiece[][] DraggablePieces { get { return _draggablePieces; } set { _draggablePieces = value; } }

  private bool _isSelfReady = false;
  private bool _isOpponentReady = false;

  private static readonly Array<Vector2> _p1AllowedDropPositions = [
    new Vector2(0, 0), new Vector2(2, 0), new Vector2(1, 1),
    new Vector2(2, 2), new Vector2(2, 4), new Vector2(2, 7),
    new Vector2(2, 9), new Vector2(1, 10), new Vector2(0, 11),
    new Vector2(2, 11)
  ];
  private static readonly Array<Vector2> _p2AllowedDropPositions = [
    new Vector2(11, 11), new Vector2(9, 11), new Vector2(10, 10),
    new Vector2(9, 9), new Vector2(9, 7), new Vector2(9, 4),
    new Vector2(9, 2), new Vector2(10, 1), new Vector2(11, 0),
    new Vector2(9, 0)
  ];

  public Dictionary<RoleType, Array<Vector2>> AllowedDropPositions =
    new Dictionary<RoleType, Array<Vector2>>()
    {
      { RoleType.P1, _p1AllowedDropPositions },
      { RoleType.P2, _p2AllowedDropPositions }
    };

  private RoleType _player;
  public RoleType Player { get { return _player; } set { _player = value; } }

  private int[] p1Arrangements;
  private int[] p2Arrangements;

  private void AddWorldScene()
  {
    GetTree().Root.AddChild(_world);
    _system = _world.GetNode<ChessSystem>("CenterContainer/HBoxContainer/Control/ChessSystem");
    _control = _system.Control;
    _control.GameInit();

    var opponentId = Multiplayer.GetPeers()[0];
    var selfId = Multiplayer.GetUniqueId();
    var commonInt = selfId * opponentId + (IsMultiplayerAuthority() ? 1 : 0);
    _player = commonInt % 2 == 0 ? RoleType.P1 : RoleType.P2;

    var material = (ShaderMaterial)_fogPanel.Material;
    int[] lightStatus = material.GetShaderParameter("light_status").As<int[]>();

    GD.Print("Player role: ", _player);

    for (int i = 0; i < lightStatus.Length; i++)
    {
      if ((i % 12 >= 6) == (_player == RoleType.P1))
      {
        lightStatus[i] = 0;
      }
    }

    material.SetShaderParameter("light_status", lightStatus);
  }

  public override void _Ready()
  {
    base._Ready();

    Array<DraggablePiece> pieces = GeneratePieces();
    foreach (DraggablePiece piece in pieces)
    {
      _gridContainer.AddChild(piece);
    }

    _world = _worldScene.Instantiate<World>();
    _world.Visible = false;
    CallDeferred(nameof(AddWorldScene));

    _doneButton.Pressed += OnDoneButtonPressed;
  }

  private Array<DraggablePiece> GeneratePieces()
  {
    Array<DraggablePiece> pieces = new Array<DraggablePiece>();
    PieceType[] pieceTypes = [
      PieceType.ELEPHANT, PieceType.LION, PieceType.TIGER, PieceType.LEOPARD,
      PieceType.WOLF, PieceType.WOLF, PieceType.DOG,
      PieceType.DOG, PieceType.CAT, PieceType.RAT,
    ];
    foreach (PieceType pieceType in pieceTypes)
    {
      DraggablePiece piece = _draggablePieceScene.Instantiate<DraggablePiece>();
      piece.Type = pieceType;
      pieces.Add(piece);
    }
    return pieces;
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
    HandleAlwaysCentered();
  }

  public void OnChessPlaced()
  {
    GD.Print("Chess piece placed signal delivered.");
    bool allPiecesPlaced = true;
    foreach (var position in AllowedDropPositions[_player])
    {
      if (DraggablePieces[(int)position.X][(int)position.Y] == null)
      {
        allPiecesPlaced = false;
      }
    }

    if (allPiecesPlaced)
    {
      GD.Print("All chess pieces placed. Could proceed to next stage.");
      _doneButton.Visible = true;
    }
  }

  public void OnDoneButtonPressed()
  {
    GD.Print("Done button pressed. Proceeding to next stage.");
    _doneButton.Visible = false;
    _waitingLabel.Visible = true;
    _isSelfReady = true;

    var selfArrangements = new int[10];
    for (var i = 0; i < 10; i++)
    {
      var position = AllowedDropPositions[_player][i];
      if (DraggablePieces[(int)position.X][(int)position.Y] != null)
      {
        selfArrangements[i] = (int)DraggablePieces[(int)position.X][(int)position.Y].Type;
      }
    }
    if (_player == RoleType.P1)
    {
      p1Arrangements = selfArrangements;
    }
    else
    {
      p2Arrangements = selfArrangements;
    }

    Rpc(nameof(ProcessDoneButtonPressed), selfArrangements);
    CheckBothPlayersReady();
  }

  // using rpc, notify peer on ready, when both ready, proceed to next stage
  [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
  public void ProcessDoneButtonPressed(int[] opponentArrangements)
  {
    GD.Print("Peer's arrangements: ", opponentArrangements.Join(","));
    if (_player == RoleType.P1)
    {
      p2Arrangements = opponentArrangements;
    }
    else
    {
      p1Arrangements = opponentArrangements;
    }
    _isOpponentReady = true;
    GD.Print("Peer is ready.");
    CheckBothPlayersReady();
  }

  private void CheckBothPlayersReady()
  {
    if (_isSelfReady && _isOpponentReady)
    {
      GD.Print("Both players are ready. Proceeding to next stage.");
      _world.Visible = true;

      PieceType[][] typeMap = new PieceType[12][];
      for (int i = 0; i < 12; i++)
      {
        typeMap[i] = new PieceType[12];
      }

      for (int i = 0; i < 10; i++)
      {
        typeMap[(int)AllowedDropPositions[RoleType.P1][i].X][(int)AllowedDropPositions[RoleType.P1][i].Y] = (PieceType)p1Arrangements[i];
        typeMap[(int)AllowedDropPositions[RoleType.P2][i].X][(int)AllowedDropPositions[RoleType.P2][i].Y] = (PieceType)p2Arrangements[i];
      }

      for (int i = 0; i < 12; i++)
      {
        GD.Print("Row ", i, ": ", string.Join(", ", typeMap[i]));
      }
      ChessPieceInitialArrangement arrangement = _system.CreateInitialArrangement(typeMap);
      _system.GameInit(arrangement, _player);
      _control.SwitchStageInit();
      QueueFree();
    }
  }
}