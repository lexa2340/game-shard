using Godot;

public partial class LevelConfig : Node
{
	[Export] public float ArrowSpeed = 300f;
	[Export] public float HitDistance = 70f;
	[Export] public float MissDistance = 50f;

	[Export] public string[] ArrowSequence = {
		"up", "down", "left", "right",
		"up", "left", "down", "right",
		"up", "up", "down", "down"
	};

	[Export] public float[] ArrowTimings = {
		1.0f, 1.0f, 1.0f, 1.0f,
		0.8f, 0.8f, 0.8f, 0.8f,
		0.6f, 0.5f, 0.6f, 0.5f
	};

	private GlobalManager _gm;
	private ArrowSpawner _spawner;
	private bool _finished = false;
	private int _currentIndex = 0;
	private float _timer = 0f;
	private bool _started = false;
	private int _resolved = 0;

	public override void _Ready()
	{
		_gm = GetNode<GlobalManager>("/root/GlobalManager");
		_gm.ResetStats();
		_spawner = GetNode<ArrowSpawner>("ArrowSpawner");

		_spawner.ArrowSpeed = ArrowSpeed;
		_spawner.HitDistance = HitDistance;
		_spawner.MissDistance = MissDistance;

		_spawner.ArrowHit += (dir) => ArrowResolved();
		_spawner.ArrowMissed += (dir) => ArrowResolved();

		GetTree().CreateTimer(2.0).Timeout += () => _started = true;
	}

	private void ArrowResolved()
	{
		_resolved++;
		if (_currentIndex >= ArrowSequence.Length && _resolved >= ArrowSequence.Length)
			FinishLevel();
	}

	public override void _Process(double delta)
	{
		if (_finished || !_started) return;

		_timer += (float)delta;

		if (_currentIndex < ArrowSequence.Length)
		{
			float delay = _currentIndex < ArrowTimings.Length ? ArrowTimings[_currentIndex] : 1.0f;

			if (_timer >= delay)
			{
				_timer = 0f;
				_spawner.SpawnArrow(ArrowSequence[_currentIndex]);
				_currentIndex++;
			}
		}
	}

	private void FinishLevel()
	{
		if (_finished) return;
		_finished = true;

		GD.Print("УРОВЕНЬ ПРОЙДЕН!");
		GetTree().CreateTimer(1.5).Timeout += () =>
		{
			GetTree().Paused = false;
			GetTree().ChangeSceneToFile("res://scense/menu.tscn");
		};
	}
}
