using Godot;

public partial class Menu : Node2D
{
	private Button _playBtn;
	private GlobalManager _gm;

	public override void _Ready()
	{
		_gm = GetNode<GlobalManager>("/root/GlobalManager");

		_playBtn = GetNode<Button>("PlayButton");
		var levelSelectBtn = GetNode<Button>("LevelSelectButton");
		var settingsBtn = GetNode<Button>("SettingsButton");
		var exitBtn = GetNode<Button>("ExitButton");

		UpdatePlayButton();

		levelSelectBtn.Pressed += () => GetTree().ChangeSceneToFile("res://scense/level_select.tscn");
		settingsBtn.Pressed += () => GetTree().ChangeSceneToFile("res://scense/settings.tscn");
		exitBtn.Pressed += () => GetTree().Quit();

		_playBtn.Pressed += () =>
		{
			if (_gm.SelectedLevel != "")
				_gm.LoadLevel(_gm.SelectedLevel);
		};
	}

	private void UpdatePlayButton()
	{
		bool hasLevel = _gm.SelectedLevel != "";
		_playBtn.Disabled = !hasLevel;
		_playBtn.Text = hasLevel ? $"Играть: {_gm.SelectedLevel}" : "Выберите уровень";
	}
}
