using Godot;

public partial class notes_debug_scene : Node3D
{
	[Export]
	private NoteManager _noteManager;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GD.Print("This works!");
		var noteScene = GD.Load<PackedScene>("res://Scenes/Note.tscn");
		for (int i = 0; i < 10; i++)
		{
			var x = i * 2;

			for (int j = 0; j < 10; j++)
			{
				var instance = (Note)noteScene.Instantiate();
				var z = j * 2;
				instance.Position = new Vector3(x, 0.1f, z);
				instance.RotationDegrees = new Vector3(90, 0, 0);
				instance.Content.Text = _noteManager.GetCurrentContent();
				_noteManager.AddNote(instance);
				AddChild(instance);
				GD.Print("Instatiating note at", instance.Position);
			}
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
