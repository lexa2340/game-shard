using Godot;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;

public partial class LevelBase : Node2D
{
	private bool _paused = false;
	private bool _finished = false;
	private ColorRect _overlay;
	private Button _resumeBtn;
	private Button _menuBtn;

	private ArrowSpawner _spawner;
	private string[] _sequence = { "up", "down", "left", "right" };
	private float[] _timings = { 1.0f, 1.0f, 1.0f, 1.0f };
	private int _currentIndex = 0;
	private float _timer = 0f;
	private bool _started = false;
	private int _resolved = 0;

	public override void _Ready()
	{
		var gm = GetNode<GlobalManager>("/root/GlobalManager");
		gm.ResetStats();

		LoadLevelData(gm.CurrentLevelPath);

		_spawner = GetNode<ArrowSpawner>("ArrowSpawner");
		_spawner.ArrowHit += (dir) => ArrowResolved();
		_spawner.ArrowMissed += (dir) => ArrowResolved();

		ApplyLevelSettings(gm.CurrentLevelPath);

		var menuMusic = GetNodeOrNull<MenuMusic>("/root/MenuMusic");
		menuMusic?.Stop();

		string vidPath = Path.Combine(gm.CurrentLevelPath, "vid.ogv");
		if (File.Exists(vidPath))
		{
			var video = GetNode<VideoStreamPlayer>("BGVideo");
			video.Stream = ResourceLoader.Load<VideoStream>(vidPath);
			video.Loop = true;
			video.Size = GetViewport().GetVisibleRect().Size;
			video.Play();
		}

		string iconPath = Path.Combine(gm.CurrentLevelPath, "ico.png");
		if (File.Exists(iconPath))
		{
			var image = new Image();
			if (image.Load(iconPath) == Error.Ok)
			{
				TextureRect icon = new TextureRect();
				icon.Texture = ImageTexture.CreateFromImage(image);
				icon.Modulate = new Color(1, 1, 1, 0.3f);
				icon.Position = new Vector2(10, 10);
				icon.Size = new Vector2(64, 64);
				icon.Visible = false;
				icon.MouseFilter = Control.MouseFilterEnum.Ignore;
				AddChild(icon);
			}
		}

		CreatePauseMenu();

		GetTree().CreateTimer(2.0).Timeout += () =>
		{
			_started = true;
			_spawner.StartChecking();
		};
	}

	private void LoadLevelData(string levelPath)
	{
		string configPath = Path.Combine(levelPath, "level.cs");
		if (!File.Exists(configPath))
		{
			GD.Print("level.cs не найден");
			return;
		}

		string code = File.ReadAllText(configPath);

		var seqMatch = Regex.Match(code, @"Sequence\s*=\s*new\s*string\s*\[\s*\]\s*\{([^}]+)\}");
		if (seqMatch.Success)
		{
			var items = seqMatch.Groups[1].Value.Split(',');
			var list = new System.Collections.Generic.List<string>();
			foreach (var item in items)
			{
				var cleaned = item.Trim().Trim('"');
				if (!string.IsNullOrEmpty(cleaned))
					list.Add(cleaned);
			}
			_sequence = list.ToArray();
		}

		var timMatch = Regex.Match(code, @"Timings\s*=\s*new\s*float\s*\[\s*\]\s*\{(.+?)\};", RegexOptions.Singleline);
		if (timMatch.Success)
		{
			var content = timMatch.Groups[1].Value;
			var items = content.Split(',');
			var list = new System.Collections.Generic.List<float>();
			foreach (var item in items)
			{
				var cleaned = item.Trim().Replace("f", "").Replace("\r", "").Replace("\n", "").Replace(" ", "");
				if (float.TryParse(cleaned, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float val))
					list.Add(val);
			}
			_timings = list.ToArray();
		}

		GD.Print($"Загружено: {_sequence.Length} стрелок, {_timings.Length} таймингов");
	}

	private void ApplyLevelSettings(string levelPath)
	{
		string configPath = Path.Combine(levelPath, "level.cs");
		if (!File.Exists(configPath)) return;

		string code = File.ReadAllText(configPath);

		var speedMatch = Regex.Match(code, @"ArrowSpeed\s*=\s*([\d.]+)f");
		if (speedMatch.Success && float.TryParse(speedMatch.Groups[1].Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float speed))
		{
			_spawner.ArrowSpeed = speed;
			GD.Print("Скорость стрелок: " + speed);
		}

		var hitMatch = Regex.Match(code, @"HitDistance\s*=\s*([\d.]+)f");
		if (hitMatch.Success && float.TryParse(hitMatch.Groups[1].Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float hit))
		{
			_spawner.HitDistance = hit;
		}

		var missMatch = Regex.Match(code, @"MissDistance\s*=\s*([\d.]+)f");
		if (missMatch.Success && float.TryParse(missMatch.Groups[1].Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float miss))
		{
			_spawner.MissDistance = miss;
		}
	}

	private void ArrowResolved()
	{
		_resolved++;
		if (_currentIndex >= _sequence.Length && _resolved >= _sequence.Length)
			FinishLevel();
	}

	public override void _Process(double delta)
	{
		if (_paused || !_started || _finished) return;

		_timer += (float)delta;

		while (_currentIndex < _sequence.Length)
		{
			float delay = _currentIndex < _timings.Length ? _timings[_currentIndex] : 1.0f;

			if (_timer >= delay)
			{
				_timer -= delay;
				_spawner.SpawnArrow(_sequence[_currentIndex]);
				_currentIndex++;
			}
			else
			{
				break;
			}
		}
	}

	private void FinishLevel()
	{
		if (_finished) return;
		_finished = true;

		var player = GetNodeOrNull<CanvasItem>("Player");
		if (player != null) player.Visible = false;

		var arrows = GetTree().GetNodesInGroup("arrows");
		foreach (var arrow in arrows)
		{
			if (IsInstanceValid(arrow))
				arrow.QueueFree();
		}

		AudioServer.SetBusVolumeDb(0, Mathf.LinearToDb(0.3f));

		var video = GetNodeOrNull<VideoStreamPlayer>("BGVideo");
		if (video != null)
			video.Modulate = new Color(0.4f, 0.4f, 0.4f, 1f);

		SaveRecord();

		GetTree().CreateTimer(1.0).Timeout += () =>
		{
			var canvas = new CanvasLayer();
			canvas.Name = "ResultCanvas";
			canvas.Layer = 100;
			AddChild(canvas);

			var resultScreen = GD.Load<PackedScene>("res://scense/ResultScreen.tscn").Instantiate();
			resultScreen.Name = "ResultScreen";
			canvas.AddChild(resultScreen);
		};
	}

	private void SaveRecord()
	{
		string filePath = Path.Combine(
			ProjectSettings.GlobalizePath("user://"),
			"record_" + GetNode<GlobalManager>("/root/GlobalManager").CurrentLevelName + ".json"
		);

		int oldRecord = 0;
		if (File.Exists(filePath))
		{
			string json = File.ReadAllText(filePath);
			oldRecord = JsonSerializer.Deserialize<RecordData>(json)?.Score ?? 0;
		}

		var gm = GetNode<GlobalManager>("/root/GlobalManager");
		if (gm.Score > oldRecord)
		{
			var data = new RecordData { Score = gm.Score };
			File.WriteAllText(filePath, JsonSerializer.Serialize(data));
		}
	}

	private void CreatePauseMenu()
	{
		_overlay = new ColorRect();
		_overlay.Color = new Color(0, 0, 0, 0.6f);
		_overlay.Size = GetViewport().GetVisibleRect().Size;
		_overlay.Visible = false;
		_overlay.MouseFilter = Control.MouseFilterEnum.Ignore;
		AddChild(_overlay);

		_resumeBtn = new Button();
		_resumeBtn.Text = "Продолжить";
		_resumeBtn.Size = new Vector2(200, 50);
		_resumeBtn.Position = new Vector2(476, 300);
		_resumeBtn.Visible = false;
		_resumeBtn.Pressed += () => SetPaused(false);
		AddChild(_resumeBtn);

		_menuBtn = new Button();
		_menuBtn.Text = "В меню";
		_menuBtn.Size = new Vector2(200, 50);
		_menuBtn.Position = new Vector2(476, 360);
		_menuBtn.Visible = false;
		_menuBtn.Pressed += () =>
		{
			SetPaused(false);
			var mm = GetNodeOrNull<MenuMusic>("/root/MenuMusic");
			mm?.Play();
			GetTree().ChangeSceneToFile("res://scense/menu.tscn");
		};
		AddChild(_menuBtn);
	}

	private void SetPaused(bool paused)
	{
		_paused = paused;
		GetTree().Paused = paused;
		AudioServer.SetBusMute(0, paused);
		_overlay.Visible = paused;
		_resumeBtn.Visible = paused;
		_menuBtn.Visible = paused;

		var video = GetNodeOrNull<VideoStreamPlayer>("BGVideo");
		if (video != null)
			video.Paused = paused;
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey key && key.Pressed && key.Keycode == Key.Escape)
		{
			if (_finished) return;
			SetPaused(!_paused);
		}
	}
}

public class RecordData
{
	public int Score { get; set; }
}
