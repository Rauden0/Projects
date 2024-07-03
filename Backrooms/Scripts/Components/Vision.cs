using System;
using System.Linq;
using Godot;

namespace Backrooms.Scripts.Components;

public partial class Vision : Node3D
{
	[Export] public float AngleOfVision = Mathf.DegToRad(60.0f);
	[Export] public float MaxVision = 40.0f;
	[Export] public float AngleBetweenRays = Mathf.DegToRad(1.5f);

	private RayCast3D[] _rays;

	public override void _Ready()
	{
		CreateRays();
	}

	//creating pre set number of rays and setting them as children

	private void CreateRays()
	{
		// rayCasts == float?? Watafak ale nebudu to měnit, tohle tvůj salám
		var rayCasts = AngleOfVision / AngleBetweenRays;
		_rays = new RayCast3D[(int)Math.Floor(rayCasts)];

		for (var i = 0; i < rayCasts; i++)
		{
			var ray = new RayCast3D();
			var angle = AngleOfVision * ((i - rayCasts) / 2);
			var x = new Vector3(0, 1, 0);
			ray.TargetPosition = Vector3.Forward.Rotated(x.Normalized(), angle) * MaxVision;
			_rays[i] = ray;
			AddChild(ray);
		}
	}

	// Checking if ray is colliding with player
	public bool Sees(Node3D target)
	{
		return _rays.Any(ray => ray.GetCollider() == target);
	}
}
