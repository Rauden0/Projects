using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class BackroomsGenerator : Node3D
{
	[Export] public double BlockDepth = 4;
	[Export] public double BlockWidth = 3;
	[Export] private PackedScene Ceiling;

	private readonly Dictionary<(int, int), Node3D> CeilPositions = new();

	[Export] public int CurrentEnemies;
	private bool FinishedBaking = true;
	[Export] private float Height = 1.5f;
	[Export] private int ChanceOfSpawingNote = 40;

	[ExportGroup("Parameters")] 
	[Export] private int ChanceOfSpawningEnemy = 100;

	private float LastBake = 5f;
	[Export] private int MaxEnemies = 10;
	[Export] private int MaxNotes = 10;
	private NavigationRegion3D NavRegion;
	[Export] private PackedScene note;

	private readonly Dictionary<(int, int), Node3D> NotesPosition = new();

	private CharacterBody3D Player;

	private readonly Dictionary<(int, int), Node3D> Positions = new();

	[Export] public int RenderDistanceInBlocks = 5;

	[ExportGroup("References")] 
	[Export] private PackedScene Room1;
	[Export] private PackedScene Room2;
	[Export] private PackedScene Room3;
	[Export] private PackedScene Room4;
	[Export] private PackedScene Room5;

	[Export] private PackedScene wretch;
	[Export] private NoteManager _noteManager;
	

	private PackedScene[] RoomArray;
	private double TimeFromLastDeletion;
	public override void _Ready()
	{
		Room1 = (PackedScene)ResourceLoader.Load("res://Generation/room_1.tscn");
		Room2 = (PackedScene)ResourceLoader.Load("res://Generation/room_2.tscn");
		Room3 = (PackedScene)ResourceLoader.Load("res://Generation/room_3.tscn");
		Room4 = (PackedScene)ResourceLoader.Load("res://Generation/room_4.tscn");
		Room5 = (PackedScene)ResourceLoader.Load("res://Generation/room_5.tscn");
		Ceiling = (PackedScene)ResourceLoader.Load("res://Generation/Ceilingn.tscn");
		wretch = (PackedScene)ResourceLoader.Load("res://NPCs/Enemies/Wretch/wretch.tscn");
		note = (PackedScene)ResourceLoader.Load("res://Scenes/Note.tscn");
		RoomArray = new[] { Room1,Room5, Room2, Room3, Room4 };
		Player = GetParent().GetNode<CharacterBody3D>("Player");
		NavRegion = GetParent().GetNode<NavigationRegion3D>("NavigationRegion3D");
	}

	public bool Present(int x, int z)
	{
		return Positions.ContainsKey((x, z));
	}

	public void DeleteUnnecesary(int PlayerX, int PlayerZ)
	{
		//Console.WriteLine("PlayerX{0},PlayerZ{1}",PlayerX,PlayerZ);
		foreach (var (DirX, DirZ) in
				 Positions.Keys.ToList())
			if (new Vector2(DirX, DirZ).DistanceTo(new Vector2(PlayerX, PlayerZ)) > RenderDistanceInBlocks * BlockDepth)
			{
				//Console.WriteLine("DirX{0},DirZ{1}",DirX,DirZ);
				if (NotesPosition.ContainsKey((DirX, DirZ)))
				{
					NotesPosition[(DirX, DirZ)].QueueFree();
					Console.WriteLine("Destroyed his ass{0}", NotesPosition.Count);
					NotesPosition.Remove((DirX, DirZ));
				}

				Positions[(DirX, DirZ)].QueueFree();
				CeilPositions[(DirX, DirZ)].QueueFree();
				Positions.Remove((DirX, DirZ));
				CeilPositions.Remove((DirX, DirZ));
			}
	}

	public override void _PhysicsProcess(double delta)
	{
		LastBake += (float)delta;
		var tmpX = Player.GlobalPosition.X / BlockWidth;
		var tmpZ = Player.GlobalPosition.Z / BlockDepth;
		var playerX = (int)((int)tmpX * BlockWidth);
		var playerZ = (int)((int)tmpZ * BlockDepth);
		for (var i = -RenderDistanceInBlocks + 1; i < RenderDistanceInBlocks + 2; i++)
		for (var j = -RenderDistanceInBlocks + 1; j < RenderDistanceInBlocks + 2; j++)
			GenerateRoomIfNotPresent(playerX + i * (int)BlockWidth, playerZ + j * (int)BlockDepth);

		TimeFromLastDeletion += delta;
		if (TimeFromLastDeletion > 10)
		{
			TimeFromLastDeletion = 0;
			DeleteUnnecesary(playerX, playerZ);
		}
	}

	private void GenerateRoomIfNotPresent(int x, int z)
	{
		if (!Present(x, z))
		{
			SpawnRoom(x, z);
			if (Player.GlobalPosition.DistanceTo(new Vector3(x,1,z)) < 10)
			{
				return;
			}
			var rand = new Random();
			if (rand.Next(ChanceOfSpawningEnemy) == 1 && CurrentEnemies < MaxEnemies)
			{
				CurrentEnemies++;
				SpawnWrench(x, z);
			}

			if (rand.Next(ChanceOfSpawingNote) == 1 && NotesPosition.Count < MaxNotes) SpawnNote(x, z);
		}
	}

	private void SpawnRoom(int x, int z)
	{
		var rand = new Random();
		var indexOfRandomRoom = rand.Next(RoomArray.Length - 1);
		if (Player.GlobalPosition.DistanceTo(new Vector3(x,1,z)) < 20)
		{
			indexOfRandomRoom = rand.Next(2);
		}
		var RandomRoom = RoomArray[indexOfRandomRoom];
		var ceiling = (Node3D)Ceiling.Instantiate();
		var room = (Node3D)RandomRoom.Instantiate();
		room.Position = new Vector3(x, 0, z);
		var randomRotation = rand.Next(0, 2) * 180;
		room.RotateY(Mathf.DegToRad(randomRotation));
		ceiling.Position = new Vector3(x + 2.5f, 0, z + 1.6f);
		Positions.Add((x, z), room);
		CeilPositions.Add((x, z), ceiling);
		NavRegion.AddChild(room);
		GetParent().AddChild(ceiling);
		if (LastBake > 5)
		{
			LastBake = 0;
			NavRegion.BakeNavigationMesh();
			Console.WriteLine("Baked");
		}
	}

	private void SpawnWrench(int x, int z)
	{
		if (CurrentEnemies > MaxEnemies) return;
		var wretch = (CharacterBody3D)this.wretch.Instantiate();
		wretch.Position = new Vector3(x + 1, Height, z + 1.5f);
		GetParent().AddChild(wretch);
	}

	private void SpawnNote(int x, int z)
	{
		Note instance = (Note) note.Instantiate();
		instance.Position = new Vector3(x + 1, 0.6f, z + 1);
		instance.RotationDegrees = new Vector3(90, 0, 0);
		instance.Content.Text = _noteManager.GetCurrentContent();
		_noteManager.AddNote(instance);
		AddChild(instance);
	}
}
