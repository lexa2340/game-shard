using Godot;

public partial class HUD : CanvasLayer
{
	private Label _label;
	private GlobalManager _gm;

	public override void _Ready()
	{
		_label = GetNode<Label>("Label");
		_gm = GetNode<GlobalManager>("/root/GlobalManager");
	}

	public override void _Process(double delta)
	{
		_label.Text = $"Очки: {_gm.Score}\n" +
					  $"Попадания: {_gm.Hits}\n" +
					  $"Промахи: {_gm.Misses}\n" +
					  $"Точность: {_gm.Accuracy:F1}%";
	}
}
