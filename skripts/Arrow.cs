using Godot;

public partial class Arrow : Area2D
{
	[Export] public float Speed = 300f;
	[Export] public string Direction { get; set; } = "up";

	private Vector2 _velocity;
	private bool _isHit = false;

	public override void _Ready()
	{
		AddToGroup("arrows");

		var sprite = new Sprite2D();
		AddChild(sprite);

		switch (Direction)
		{
			case "up":    sprite.Texture = GD.Load<Texture2D>("res://texture/tek_2.png"); break;
			case "down":  sprite.Texture = GD.Load<Texture2D>("res://texture/tek_5.png"); break;
			case "left":  sprite.Texture = GD.Load<Texture2D>("res://texture/tek_4.png"); break;
			case "right": sprite.Texture = GD.Load<Texture2D>("res://texture/tek_3.png"); break;
		}

		var collision = new CollisionShape2D();
		var shape = new RectangleShape2D();
		shape.Size = new Vector2(70, 70);
		collision.Shape = shape;
		AddChild(collision);

		switch (Direction)
		{
			case "up":    _velocity = new Vector2(0, -Speed); break;
			case "down":  _velocity = new Vector2(0,  Speed); break;
			case "left":  _velocity = new Vector2(-Speed, 0); break;
			case "right": _velocity = new Vector2( Speed, 0); break;
		}
	}

	private float _lifeTime = 0f;
private float _maxLifeTime = 10f; 

public override void _Process(double delta)
{
	if (GetTree().Paused) return;
	
	_lifeTime += (float)delta;
	if (_lifeTime > _maxLifeTime)
	{
		Miss(); 
		return;
	}
	
	Position += _velocity * (float)delta;

	var screenSize = GetViewport().GetVisibleRect().Size;
	if (Position.X < -100 || Position.X > screenSize.X + 100 ||
		Position.Y < -100 || Position.Y > screenSize.Y + 100)
		QueueFree();
}

	public void Hit()
	{
		if (_isHit) return;
		_isHit = true;
		QueueFree();
	}

	public void Miss()
	{
		if (_isHit) return;
		_isHit = true;
		QueueFree();
	}

	public bool IsHit() => _isHit;
	public string GetDirection() => Direction;
}
