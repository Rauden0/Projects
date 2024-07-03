using Godot;
using System;

public partial class Menu : Control
{
	public override void _Ready()
	{
		Input.MouseMode = Input.MouseModeEnum.Visible;
	}

	private void _on_play_pressed()
	{
		GetTree().ChangeSceneToFile("res://debug.tscn");
	}


	private void _on_options_pressed()
	{
		// Replace with function body.
	}


	private void _on_quit_pressed()
	{
		GetTree().Quit();
	}
}
