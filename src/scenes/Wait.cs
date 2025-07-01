using Godot;
using Godot.Collections;

public partial class Wait : Control
{
  private MultiplayerManager _multiplayer;
  private LineEdit _ipPlaceholder;
  private VBoxContainer _ipContainer;
  private Array<LineEdit> _ipLabels = new Array<LineEdit>();

  public override void _Ready()
  {
    base._Ready();

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

    _ipPlaceholder = GetNode<LineEdit>("Panel/Control/VC/HC/IPLabelPlaceholder");
    _ipContainer = GetNode<VBoxContainer>("Panel/Control/SC/MC/IPContainer");

    _multiplayer.PlayerConnected += OnPlayerConnected;
    _multiplayer.PlayerDisconnected += OnPlayerDisconnected;

    GD.Print("IPs: ");
    string[] ips = NetUtils.GetValidIPv4Addresses();

    if (ips.Length == 0)
    {
      GD.Print("No valid IP addresses found.");
      _ipPlaceholder.Text = "Not Detected.";
      return;
    }
    else
    {
      _ipPlaceholder.Text = "";
    }

    foreach (string ip in ips)
    {
      GD.Print(ip);
      var label = new LineEdit
      {
        Text = ip,
        Editable = false,
        Alignment = HorizontalAlignment.Right,
        TooltipText = "Click to Copy"
      };
      label.FocusEntered += () =>
      {
        label.SelectAll();

        foreach (var ipLabel in _ipLabels)
        {
          if (ipLabel != label)
          {
            ipLabel.SelectingEnabled = false;
            ipLabel.SelectingEnabled = true;
          }
        }
        DisplayServer.ClipboardSet(label.Text);

        Tween tween = GetTree().CreateTween();
        tween.TweenCallback(Callable.From(() =>
        {
          label.SelectingEnabled = false;
          label.SelectingEnabled = true;
          label.ReleaseFocus();
        })).SetDelay(0.1f);
      };
      _ipLabels.Add(label);
      _ipContainer.AddChild(label);
    }
  }

  private void AddMultiplayerManager()
  {
    GetTree().Root.AddChild(_multiplayer);
  }

  private void OnPlayerDisconnected(long id)
  {
    GD.Print($"Player {id} left the game");
  }

  private void OnPlayerConnected(long id)
  {
    GD.Print($"Player {id} joined the game");

    // If we're the host and we now have 2 players, start the game
    if (_multiplayer.IsHost && _multiplayer.GetPlayerCount() >= 2)
    {
      GD.Print("Game is ready to start!");
      GetTree().ChangeSceneToFile("res://scenes/arrange.tscn");
    }
  }

  public override void _ExitTree()
  {
    // Disconnect signals to prevent memory leaks
    if (_multiplayer != null)
    {
      _multiplayer.PlayerConnected -= OnPlayerConnected;
      _multiplayer.PlayerDisconnected -= OnPlayerDisconnected;
    }
    base._ExitTree();
  }
}