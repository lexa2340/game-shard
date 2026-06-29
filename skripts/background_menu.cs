using Godot;
using System.Collections.Generic;

public partial class background_menu : Node2D
{
	private Texture2D _tileTexture;
	private float ScrollSpeed = 60f;
	private List<Sprite2D> _tiles = new();
	private float _tileWidth;
	private int _cols, _rows;
	private float _totalWidth;

	public override void _Ready()
	{
		GD.Print("СТАРТ ФОНА");

		_tileTexture = GD.Load<Texture2D>("res://texture/tile.png");

		if (_tileTexture == null)
		{
			GD.PrintErr("ОШИБКА: файл не найден!");
			return;
		}

		_tileWidth = _tileTexture.GetSize().X;
		var screenSize = GetViewport().GetVisibleRect().Size;

		_cols = Mathf.CeilToInt(screenSize.X / _tileWidth) + 2;
		_rows = Mathf.CeilToInt(screenSize.Y / _tileTexture.GetSize().Y) + 1;
		_totalWidth = _cols * _tileWidth;

		for (int x = 0; x < _cols; x++)
		{
			for (int y = 0; y < _rows; y++)
			{
				var tile = new Sprite2D();
				tile.Texture = _tileTexture;
				tile.Position = new Vector2(x * _tileWidth, y * _tileTexture.GetSize().Y);
				AddChild(tile);
				_tiles.Add(tile);
			}
		}

		GD.Print($"Создано плиток: {_tiles.Count}");
	}

	public override void _Process(double delta)
	{
		if (_tiles.Count == 0) return;

		foreach (var tile in _tiles)
		{
			tile.Position = new Vector2(
				tile.Position.X - ScrollSpeed * (float)delta,
				tile.Position.Y
			);

			if (tile.Position.X < -_tileWidth)
			{
				tile.Position = new Vector2(
					tile.Position.X + _totalWidth,
					tile.Position.Y
				);
			}
		}
	}
}
