using Godot;

namespace Backrooms.Scripts.Player;

public partial class Player : CharacterBody3D
{
	[ExportGroup("Movement")] [Export] public float Speed = 0.5f;
	[Export] public float JumpVelocity = 4.5f;
	[Export] public float Stamina = 100f;

	[ExportGroup("CameraSettings")] [Export]
	public float Sensitivity = 0.003f;

	[Export] public float MinClamp = -40f;
	[Export] public float MaxClamp = 60f;


	// Get the _gravity from the project settings to be synced with RigidBody nodes.
	private float _gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

	// Node references
	private Node3D _head;
	private Camera3D _camera3D;
	private AudioStreamPlayer3D _audio;
	private float _timePlay;
	private ProgressBar _staminaMeter;
	private float _staminaUsed;

	public override void _Ready()
	{
		_audio = GetNode<AudioStreamPlayer3D>("PlayerSound");
		_head = GetNode<Node3D>("Head");
		_camera3D = GetNode<Camera3D>("Head/Camera3D");
		_staminaMeter = GetNode<ProgressBar>("Head/Camera3D/CanvasLayer/ProgressBar");
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	private void HandleCamera(InputEventMouseMotion mouseMotion)
	{
		var cameraRotationX = -mouseMotion.Relative.Y * Sensitivity;
		var cameraRotationY = -mouseMotion.Relative.X * Sensitivity;
		// Clamping the values to min and max angle. Otherwise the player will be able to make barrel rolls.
		if (_camera3D.Rotation.X + cameraRotationX < Mathf.DegToRad(MaxClamp)
			&& _camera3D.Rotation.X + cameraRotationX > Mathf.DegToRad(MinClamp))
			_camera3D.RotateX(Mathf.Clamp(cameraRotationX, Mathf.DegToRad(MinClamp),
				Mathf.DegToRad(MaxClamp)));
		_head.RotateY(cameraRotationY);
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventMouseMotion mouseMotion) HandleCamera(mouseMotion);
	}

	public override void _PhysicsProcess(double delta)
	{
		var velocity = Velocity;
		// Add the _gravity.
		if (!IsOnFloor()) velocity.Y -= _gravity * (float)delta;

		// Handle Jump.
		if (Input.IsActionJustPressed("jump") && IsOnFloor()) velocity.Y = JumpVelocity;

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		var inputDir = Input.GetVector("left", "right", "forward", "backward");
		var direction = (_head.Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

		var currentSpeed = Speed;
		if (Input.IsActionJustPressed("jump") && IsOnFloor()) velocity.Y = JumpVelocity;
		if (Input.IsActionJustPressed("FlashLightToggle"))
		{
			GetNode<SpotLight3D>("Head/Camera3D/SpotLight3D").Visible =  !GetNode<SpotLight3D>("Head/Camera3D/SpotLight3D").Visible;
		}

		if (Input.IsActionPressed("sprint") && Stamina > 0 && _staminaUsed < 0)
		{
			if (Stamina < 1) _staminaUsed = 200;
			currentSpeed = Speed * 1.5f;
			Stamina -= 1f;
			_staminaMeter.Value -= 1f;
		}
		else
		{
			_staminaUsed -= 1;
			_staminaMeter.Value += 0.2f;
			Stamina += 0.2f;
		}

		if (direction != Vector3.Zero)
		{
			velocity.X = direction.X * currentSpeed;
			velocity.Z = direction.Z * currentSpeed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, currentSpeed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, currentSpeed);
		}

		//Basic logic to play walking sound
		if (inputDir != new Vector2(0, 0))
		{
			_timePlay = 25;
			_audio.VolumeDb = _timePlay + 80f;
			if (_audio.Playing == false) _audio.Play();
		}
		else
		{
			_timePlay -= 1;
		}

		if (_timePlay < 0)
		{
			_timePlay = 0;
			_audio.Stop();
		}

		Velocity = velocity;
		MoveAndSlide();
	}
}
