using Godot;
using System.Collections.Generic;
using System.IO;

public partial class GlobalManager : Node
{
	public List<string> LevelNames = new List<string>();
	public List<string> LevelPaths = new List<string>();
	public List<string> LevelIcons = new List<string>();

	public int Score = 0;
	public int Hits = 0;
	public int Misses = 0;
	public float Accuracy => (Hits + Misses > 0) ? (float)Hits / (Hits + Misses) * 100f : 100f;

	public string SelectedLevel = "";
	public string CurrentLevelName = "";
	public string CurrentLevelPath = "";

	public override void _Ready()
	{
		ScanLevelsFolder();
	}

	public void ScanLevelsFolder()
	{
		LevelNames.Clear();
		LevelPaths.Clear();
		LevelIcons.Clear();

		string levelsDir = Path.Combine(ProjectSettings.GlobalizePath("user://"), "levels");

		if (!Directory.Exists(levelsDir))
		{
			Directory.CreateDirectory(levelsDir);
			return;
		}

		string[] subDirs = Directory.GetDirectories(levelsDir);
		foreach (string dir in subDirs)
		{
			string levelName = Path.GetFileName(dir);
			string configPath = Path.Combine(dir, "level.cs");

			if (File.Exists(configPath))
			{
				LevelNames.Add(levelName);
				LevelPaths.Add(dir);
				LevelIcons.Add("");
				GD.Print("Уровень добавлен: " + levelName);
			}
		}
	}

	public void LoadLevel(string levelName)
	{
		int idx = LevelNames.IndexOf(levelName);
		if (idx == -1) return;

		CurrentLevelName = levelName;
		CurrentLevelPath = LevelPaths[idx];

		var scene = ResourceLoader.Load<PackedScene>("res://scense/level_base.tscn");
		if (scene != null)
			GetTree().ChangeSceneToPacked(scene);
	}

	public int GetLevelCount() => LevelNames.Count;
	public string GetLevelName(int i) => (i >= 0 && i < LevelNames.Count) ? LevelNames[i] : "";

	public void ResetStats()
	{
		Score = 0;
		Hits = 0;
		Misses = 0;
	}
}
