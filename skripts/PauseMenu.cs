using Godot;

public partial class PauseMenu : CanvasLayer
{
	public override void _Ready()
	{
		var resumeBtn = GetNode<Button>("ResumeBtn");
		var menuBtn = GetNode<Button>("MenuBtn");

		resumeBtn.Pressed += () =>
		{
			GetTree().Paused = false;
			Hide();
		};

		menuBtn.Pressed += () =>
		{
			GetTree().Paused = false;
			GetTree().ChangeSceneToFile("res://scense/menu.tscn");
		};
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey key && key.Pressed && key.Keycode == Key.Escape)
		{
			if (Visible)
			{
				GetTree().Paused = false;
				Hide();
			}
			else
			{
				GetTree().Paused = true;
				Show();
			}
		}
	}
}
