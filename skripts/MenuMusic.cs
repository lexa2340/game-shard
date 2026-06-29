using Godot;

public partial class MenuMusic : Node
{
	private AudioStreamPlayer _player;

	public override void _Ready()
	{
		_player = new AudioStreamPlayer();
		_player.Stream = GD.Load<AudioStream>("res://music/MenuMusic.mp3");
		_player.VolumeDb = -10f;
		_player.Bus = "Master";
		AddChild(_player);
		Play();
	}

	public void Play()
	{
		if (!_player.Playing)
			_player.Play();
	}

	public void Stop()
	{
		_player.Stop();
	}
}
