using System;
using Godot;

namespace Backrooms.Scripts.Player;

public partial class Head : Node3D
{
	private FastNoiseLite _noise;
	private double _time;
	private Vector3 _origin;

	// Nodes
	private CharacterBody3D _player;

	[ExportGroup("BobbingSettings")] [Export]
	// Seed to offset periodic functions
	public float BobbingAmplitude = 0.04f;

	// Scales velocity so head bobbing feels just right
	private const float BobbingFrequency = 1.8f;

	[ExportGroup("SwaySettings")] [Export]
	// Seed to offset periodic functions
	public int Seed = 1024;

	[Export] public float Frequency = 0.01f;
	[Export] public float AmplitudeNoise = 0.1f;
	[Export] public float AmplitudeWave = 0.01f;

	[ExportGroup("Components")] [Export] public RayCast3D InteractionRay;

	private void SetupNoise()
	{
		_noise = new FastNoiseLite();
		// In case of performance problems switch to SimplexNoise
		_noise.NoiseType = FastNoiseLite.NoiseTypeEnum.SimplexSmooth;
		_noise.Frequency = Frequency;
	}


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		SetupNoise();
		_origin = Position;
		_player = GetNode<CharacterBody3D>("../");
	}

	public override void _Process(double delta)
	{
		// Check for interaction input (e.g., the "E" key)
		if (Input.IsActionJustPressed("interact"))
			if (InteractionRay.IsColliding())
			{
				var collidedObject = InteractionRay.GetCollider() as Node;
				// Implement your interaction logic here
				GD.Print("Interacted with: ", collidedObject?.Name);

				// Connect to the Interacted signal
				if (collidedObject is Note note) note.Interact(this);
			}
	}

	private void AddSway()
	{
		// Simplex + Sin or Cos, own hase it's own amplitude
		var yNoise = _noise.GetNoise1D((float)_time) * AmplitudeNoise
					 + (float)Math.Sin(_time) * AmplitudeWave;
		var xNoise = _noise.GetNoise1D((float)_time + Seed) * AmplitudeNoise +
					 (float)Math.Cos(_time + Seed) * AmplitudeWave;

		Position = new Vector3(_origin.X + xNoise, _origin.Y + yNoise, _origin.Z);
	}

	private void AddBobbing()
	{
		// Excludes the Y axis because we don't need head bobbing when falling
		var xzVelocity = new Vector2(_player.Velocity.X, _player.Velocity.Z).Length();

		// Two functions what when combined make an infinity shape. The 2 there is a constant for proper shape.
		var xBob = (float)Math.Cos(xzVelocity * BobbingFrequency * _time) * BobbingAmplitude;
		var yBob = (float)Math.Sin(xzVelocity * BobbingFrequency * _time * 2) * BobbingAmplitude;

		Position = new Vector3(Position.X + xBob, Position.Y + yBob, Position.Z);
	}

	public override void _PhysicsProcess(double delta)
	{
		_time += delta;
		AddSway();
		AddBobbing();
	}
}
