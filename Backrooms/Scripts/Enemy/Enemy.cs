using System;
using System.Collections.Generic;
using Backrooms.Scripts.Components;
using Godot;

namespace Backrooms.Scripts.Enemy;

public partial class Enemy : CharacterBody3D
{
	[ExportGroup("Movement")] [Export] public float Speed = 3.0f;
	private float _gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();
	[ExportGroup("References")] [Export] public CharacterBody3D Player;

	[ExportGroup("Components")] [Export] public Vision Vision;

	[ExportGroup("Sounds")] [Export] public AudioStreamPlayer3D Steps;
	[Export] public AudioStreamPlayer3D Scream;
	private double StepTime = 0;

	[ExportGroup("Navigation")] [Export] public NavigationAgent3D PlayerNavigationAgent3D;
	[Export] public NavigationAgent3D PathNavigationAgent3D;

	[ExportGroup("Animations")] [Export] public AnimationTree AnimationTree;

	private Vector3 _newGlobalPosition;
	private double _timeElapsed;
	private bool _isHunting;
	private double _timeFromLastScream = 0;
	private BackroomsGenerator Generator;
	private Vector3 CurrentLocation = new(0, 0, 0);

	public override void _Ready()
	{
		_timeElapsed = 0;
		Player = GetParent().GetNode<CharacterBody3D>("Player");
		Generator = GetParent().GetNode<BackroomsGenerator>("Generator");
	}

	public override void _PhysicsProcess(double delta)
	{
		var velocity = Velocity;
		if (Position.Y > 0.5) Position = new Vector3(Position.X, 0.5f, Position.Z);
		_UpdateAnimations(delta);

		_isHunting = Vision.Sees(Player);

		if (_isHunting)
		{
			_timeElapsed = 3;
			_UpdateTargetLocation(Player.GlobalPosition);
		}

		_timeFromLastScream -= 0.1;
		_timeElapsed -= delta;

		_UpdateDirection();

		_Rotate();

		_PlaySounds();

		if (!IsInsideTree())
		{
			TooFar();
			return;
		}

		MoveAndSlide();

		TooFar();
		if (Player.GlobalPosition.DistanceTo(GlobalPosition) < 1.5f && _timeElapsed > 0)
		{
			//Console.WriteLine("FUCKING HELL");
			_isHunting = true;
			_on_to_player_navigation_agent_target_reached();
		}
	}

	private void TooFar()
	{
		if (GlobalPosition.DistanceTo(Player.GlobalPosition) > 50)
		{
			GetParent().GetNode<BackroomsGenerator>("Generator").CurrentEnemies--;
			QueueFree();
		}
	}

	private void _UpdateAnimations(double delta)
	{
		AnimationTree.Set("parameters/IdleRun/blend_amount", Velocity.Length() != 0 ? Velocity.Length() / 3 : 0);
		AnimationTree.Advance(delta);
	}

	private void _PlaySounds()
	{
		
		_TryPlayScream();
		//GD.Print(Velocity.Length());
		StepTime -= 0.1f;
		if ((Steps.Playing == false && Velocity.Length() > 1f) || StepTime > 0)
		{
			// GD.Print(StepTime);
			if (!Steps.Playing)
			{
				StepTime = 5;
				Steps.Play();
			}
		}

		else 
			Steps.Stop();
	}

	private void _TryPlayScream()
	{
		if (_timeElapsed > 2.90f && _timeFromLastScream < 0)
		{
			_timeFromLastScream = 30;
			if (Scream.Playing == false) Scream.Play();
		}
	}

	private void _UpdateDirection()
	{
		//Checking how much time since last was player seen by enemy 
		PathNavigationAgent3D.TargetPosition = Player.GlobalPosition;
		var nextLocation = PlayerNavigationAgent3D.GetNextPathPosition();

		if (!_isHunting && _timeElapsed <= 0)
		{
			if (CurrentLocation == new Vector3(0, 0, 0) || CurrentLocation.DistanceTo(GlobalPosition) < 20 || Velocity.Length() < 0.3f)
			{
				var rand = new Random();
				var x = rand.Next((int)(Player.GlobalPosition.X - Generator.RenderDistanceInBlocks * Generator.BlockWidth),
					(int)(Player.GlobalPosition.X + Generator.RenderDistanceInBlocks * Generator.BlockWidth));
				var z = rand.Next((int)(Player.GlobalPosition.Z - Generator.RenderDistanceInBlocks * Generator.BlockDepth),
					(int)(Player.GlobalPosition.Z + Generator.RenderDistanceInBlocks * Generator.BlockDepth));
				PathNavigationAgent3D.TargetPosition = new Vector3(x, 1, z);
				_newGlobalPosition = PathNavigationAgent3D.GetNextPathPosition();;
				nextLocation = PathNavigationAgent3D.GetNextPathPosition();
				CurrentLocation = new Vector3(x, 1, z);
			}else
			{
				PathNavigationAgent3D.TargetPosition = CurrentLocation;
				_newGlobalPosition = PathNavigationAgent3D.GetNextPathPosition();;
				nextLocation = PathNavigationAgent3D.GetNextPathPosition();
			}

		}

		if (!IsInsideTree()) return;
		var currentLocation = GlobalTransform.Origin;
		// new velocity based on navigation agent that follows player and looks out for obstacles
		var newVelocity = (nextLocation - currentLocation).Normalized() * Speed;
		//GD.Print(newVelocity.Length());
		Velocity = Velocity.MoveToward(newVelocity, 0.25f);
		_newGlobalPosition = PathNavigationAgent3D.GetNextPathPosition();;
	}

	private void _Rotate()
	{
		var lookAtTransform = Transform.LookingAt(_newGlobalPosition, Vector3.Up);
		GlobalTransform = new Transform3D(Basis.Identity, GlobalTransform.Origin);
		GlobalTransform = lookAtTransform;
		// keep the enemy from spinning around its own	axis (Y-axis)
		this.GlobalRotation = new Vector3(0, this.GlobalRotation.Y, 0);
	}

	private void _UpdateTargetLocation(Vector3 targetLocation)
	{
		PlayerNavigationAgent3D.TargetPosition = targetLocation;
	}

	private void _on_to_player_navigation_agent_target_reached()
	{
		if (_isHunting == false && _timeElapsed < 0) return;
		GetTree().ChangeSceneToFile("res://Scenes/Menu.tscn");
	}
}
