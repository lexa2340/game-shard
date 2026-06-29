using Godot;

public partial class Settings : Control
{
	private HSlider _musicSlider;
	private HSlider _sfxSlider;
	private Button _backBtn;

	private int _musicBus;
	private int _sfxBus;

	public override void _Ready()
	{
		// Получаем шины в _Ready, а не в полях
		_musicBus = AudioServer.GetBusIndex("Music");
		_sfxBus = AudioServer.GetBusIndex("SFX");

		_musicSlider = GetNode<HSlider>("VBoxContainer/MusicSlider");
		_sfxSlider = GetNode<HSlider>("VBoxContainer/SFXSlider");
		_backBtn = GetNode<Button>("VBoxContainer/BackButton");

		_musicSlider.Value = Mathf.DbToLinear(AudioServer.GetBusVolumeDb(_musicBus)) * 100;
		_sfxSlider.Value = Mathf.DbToLinear(AudioServer.GetBusVolumeDb(_sfxBus)) * 100;

		_musicSlider.ValueChanged += (v) =>
		{
			AudioServer.SetBusVolumeDb(_musicBus, Mathf.LinearToDb((float)v / 100f));
		};

		_sfxSlider.ValueChanged += (v) =>
		{
			AudioServer.SetBusVolumeDb(_sfxBus, Mathf.LinearToDb((float)v / 100f));
		};

		_backBtn.Pressed += () => GetTree().ChangeSceneToFile("res://scense/menu.tscn");
	}
}
