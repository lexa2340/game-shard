using Godot;
using System.IO;
using System.Text.Json;

public partial class ResultScreen : Control
{
	private GlobalManager _gm;
	private Label _titleLabel;
	private Label _statsLabel;
	private Label _recordLabel;

	public override void _Ready()
	{
		_gm = GetNode<GlobalManager>("/root/GlobalManager");
		_titleLabel = GetNode<Label>("VBoxContainer/TitleLabel");
		_statsLabel = GetNode<Label>("VBoxContainer/StatsLabel");
		_recordLabel = GetNode<Label>("VBoxContainer/RecordLabel");

		var menuBtn = GetNode<Button>("VBoxContainer/MenuBtn");
		var restartBtn = GetNode<Button>("VBoxContainer/RestartBtn");

		menuBtn.Pressed += () =>
		{
   		var menuMusic = GetNodeOrNull<MenuMusic>("/root/MenuMusic");
   	 	menuMusic?.Play();
		GetTree().ChangeSceneToFile("res://scense/menu.tscn");
		};
		restartBtn.Pressed += () => _gm.LoadLevel(_gm.CurrentLevelName);

		ShowResults();
	}

	private void ShowResults()
	{
		float accuracy = _gm.Accuracy;
		bool isNewRecord = CheckRecord();

		_statsLabel.Text = $"Очки: {_gm.Score}\n" +
						   $"Попаданий: {_gm.Hits}\n" +
						   $"Промахов: {_gm.Misses}\n" +
						   $"Точность: {accuracy:F1}%";

		if (isNewRecord)
		{
			_recordLabel.Text = "НОВЫЙ РЕКОРД!";
			_recordLabel.Modulate = new Color(1, 0.8f, 0);
		}
		else
		{
			int record = LoadRecord();
			_recordLabel.Text = $"Рекорд: {record} очков";
		}
	}

	private bool CheckRecord()
	{
		int oldRecord = LoadRecord();
		return _gm.Score > oldRecord;
	}

	private int LoadRecord()
	{
		string filePath = GetRecordPath();
		if (File.Exists(filePath))
		{
			string json = File.ReadAllText(filePath);
			var data = JsonSerializer.Deserialize<RecordData>(json);
			return data?.Score ?? 0;
		}
		return 0;
	}

	private void SaveRecord()
	{
		string filePath = GetRecordPath();
		var data = new RecordData { Score = _gm.Score };
		string json = JsonSerializer.Serialize(data);
		File.WriteAllText(filePath, json);
	}

	private string GetRecordPath()
	{
		string dir = ProjectSettings.GlobalizePath("user://");
		return Path.Combine(dir, "record_" + _gm.CurrentLevelName + ".json");
	}
}
