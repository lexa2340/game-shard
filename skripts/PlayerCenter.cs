using Godot;

public partial class PlayerCenter : Sprite2D
{
	public override void _Ready()
	{
		Texture = GD.Load<Texture2D>("res://texture/tek_1.png");
		Position = GetViewport().GetVisibleRect().Size / 2;
	}
}
