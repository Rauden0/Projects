using System;
using Backrooms.Scripts.Interfaces;
using Godot;

public partial class Note : Area3D, IInteractable
{
	[ExportGroup("Animation Parameters")] [Export]
	public float Duration;

	[Export] public float DistanceFromCamera;

	[ExportGroup("Components")] [Export] public RichTextLabel Text;
	[Export] public Label3D Content;

	public readonly Guid Id = Guid.NewGuid(); 
	private Vector3 _interactorLocation;
	private Vector3 _adjustedPosition;
	private bool _isFrozen;
	private bool _isTweenFinished;

	[Signal]
	public delegate void NoteReadEventHandler(string noteId);
	
	[Signal]
	public delegate void DestroyEventHandler(string noteId);

	public void Interact(Node3D interacter, params Variant[] args)
	{
		// There should be some mutex mechanism to prevent multiple starts.

		_interactorLocation = interacter.GlobalTransform.Origin;
		_adjustedPosition = _interactorLocation - interacter.GetNode<Camera3D>("Camera3D")
			.GlobalTransform.Basis.Z.Normalized() * DistanceFromCamera;

		// Set up the Tween animation
		_isTweenFinished = false;
		var tween = GetTree().CreateTween().SetPauseMode(Tween.TweenPauseMode.Process);
		tween.TweenProperty(this, "position", _adjustedPosition, Duration).SetTrans(Tween.TransitionType.Linear);
		tween.TweenCallback(new Callable(this, MethodName._TweenFinished));
		LookAt(_interactorLocation);
		_Freeze();
	}
	private void _TweenFinished()
	{
		_isTweenFinished = true;
	}

	private void _Freeze()
	{
		_isFrozen = true;
		GetTree().Paused = _isFrozen;
	}

	private void _UnFreeze()
	{
		_isFrozen = false;
		GetTree().Paused = _isFrozen;
		EmitSignal(SignalName.NoteRead, Id.ToString());
	}

	public override void _Process(double delta)
	{

		if (Input.IsActionJustPressed("interact") && _isFrozen && _isTweenFinished)
		{
			_UnFreeze();
		}
	}
  
	public override void _ExitTree()
	{
		EmitSignal(SignalName.Destroy, Id.ToString());
		base._ExitTree();
	}
}
