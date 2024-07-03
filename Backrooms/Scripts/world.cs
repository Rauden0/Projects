using Godot;

public partial class world : Node3D
{
	private Node3D _player;

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Ready()
	{
		_player = GetNode<Node3D>("Player");
	}

	public override void _PhysicsProcess(double delta)
	{
		//calls update_target_location within enemy.cs with player location
		GetTree().CallGroup("enemies", "update_target_location", _player.GlobalTransform.Origin);
	}
}
