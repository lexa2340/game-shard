using Godot;
using System.IO;

public partial class LevelSelect : Control
{
	[Export] public string MenuScenePath = "res://scense/menu.tscn";

	private GlobalManager _gm;
	private VBoxContainer _list;

	public override void _Ready()
	{
		Button backBtn = GetNode<Button>("VBoxContainer/BackButton");
		backBtn.Pressed += () => GetTree().ChangeSceneToFile(MenuScenePath);
		_gm = GetNode<GlobalManager>("/root/GlobalManager");
		_list = GetNode<VBoxContainer>("VBoxContainer");
		RefreshList();
	}

	private void RefreshList()
	{
	   
		for (int i = _list.GetChildCount() - 1; i >= 0; i--)
		{
			var child = _list.GetChild(i);
			if (child.Name != "BackButton" && child.Name != "TitleLabel")
				child.QueueFree();
		}

		_gm.ScanLevelsFolder();
		int count = _gm.GetLevelCount();

		if (count == 0)
		{
			Label noLabel = new Label();
			noLabel.Text = "Нет уровней";
			noLabel.HorizontalAlignment = HorizontalAlignment.Center;
			_list.AddChild(noLabel);
		}
		else
		{
			for (int i = 0; i < count; i++)
			{
				string name = _gm.GetLevelName(i);
				string iconPath = Path.Combine(_gm.LevelPaths[i], "ico.png");

				HBoxContainer row = new HBoxContainer();
				row.Alignment = BoxContainer.AlignmentMode.Center;

				
				if (File.Exists(iconPath))
				{
					TextureRect icon = new TextureRect();
					var image = new Image();
					if (image.Load(iconPath) == Error.Ok)
					{
					icon.Texture = ImageTexture.CreateFromImage(image);
					}
					icon.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
					icon.CustomMinimumSize = new Vector2(48, 48);
					icon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
					row.AddChild(icon);
				}

				
			  Button btn = new Button();
		btn.Text = name;
		btn.Size = new Vector2(300, 50);


		var font = ResourceLoader.Load<Font>("res://front/Regez-Regular.otf");
		btn.AddThemeFontOverride("font", font);
		btn.AddThemeFontSizeOverride("font_size", 32);

		string captured = name;
		btn.Pressed += () =>
		{
			_gm.SelectedLevel = captured;
			GetTree().ChangeSceneToFile(MenuScenePath);
		};
		row.AddChild(btn);

				_list.AddChild(row);
			}
		}
	}

	public override void _EnterTree()
	{
		if (_list != null)
			RefreshList();
	}
}
