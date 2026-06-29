using Godot;
using System;
using System.Collections.Generic;

public partial class ArrowSpawner : Node2D
{
	[Export] public PackedScene ArrowScene;

	public float ArrowSpeed = 300f;
	public float HitDistance = 70f;
	public float MissDistance = 50f;

	public event Action<string> ArrowHit;
	public event Action<string> ArrowMissed;

	private Vector2 _center;
	private GlobalManager _gm;
	private bool _checking = false;

	// Запоминаем нажатия
	private HashSet<string> _pressedKeys = new HashSet<string>();

	public override void _Ready()
	{
		_center = GetViewport().GetVisibleRect().Size / 2;
		_gm = GetNode<GlobalManager>("/root/GlobalManager");
	}

	public void StartChecking()
{
	_checking = true;
	GD.Print("StartChecking вызван!");
}

	public void SpawnArrow(string direction)
	{
		 GD.Print("SpawnArrow: " + direction);
		if (ArrowScene == null) return;

		Arrow arrow = (Arrow)ArrowScene.Instantiate();
		arrow.Direction = direction;
		arrow.Speed = ArrowSpeed;

		var screenSize = GetViewport().GetVisibleRect().Size;

		switch (direction)
		{
			case "up":    arrow.Position = new Vector2(_center.X, screenSize.Y + 50); break;
			case "down":  arrow.Position = new Vector2(_center.X, -50); break;
			case "left":  arrow.Position = new Vector2(screenSize.X + 50, _center.Y); break;
			case "right": arrow.Position = new Vector2(-50, _center.Y); break;
		}

		AddChild(arrow);
	}

	public override void _Process(double delta)
{
	if (GetTree().Paused || !_checking) return;

	// Исправлено: W=вверх, S=вниз, A=влево, D=вправо
	if (Input.IsKeyPressed(Key.S)) _pressedKeys.Add("up");
	if (Input.IsKeyPressed(Key.W)) _pressedKeys.Add("down");
	if (Input.IsKeyPressed(Key.D)) _pressedKeys.Add("left");
	if (Input.IsKeyPressed(Key.A)) _pressedKeys.Add("right");

	CheckArrows();
}

	private void CheckArrows()
{
	var arrows = GetTree().GetNodesInGroup("arrows");

	foreach (Arrow arrow in arrows)
	{
		if (!IsInstanceValid(arrow) || arrow.IsHit()) continue;

		float dist = arrow.Position.DistanceTo(_center);
		string dir = arrow.GetDirection();

		
		if (dist < 50f && _pressedKeys.Contains(dir))
		{
			_gm.Hits++;
			_gm.Score += 10;
			arrow.Hit();
			ArrowHit?.Invoke(dir);
			GD.Print($"ПОПАДАНИЕ: {dir} на дист={dist}");
			continue;
		}

		
		if (dist < 10f)
		{
			_gm.Misses++;
			arrow.Miss();
			ArrowMissed?.Invoke(dir);
			GD.Print($"ПРОМАХ: {dir} на дист={dist}");
		}
	}

	_pressedKeys.Clear();
}
}
