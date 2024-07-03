using System;
using Godot;
using System.Collections.Generic;
using Backrooms.Scripts.Player;

public partial class NoteManager : Node3D
{
	[ExportGroup("References")] 
	[Export] private Player _player;

	[ExportGroup("Settings")] 
	[Export] private int _neededPages = 4;
	
	private Godot.Collections.Dictionary<string, Note> _notes = new ();
	private readonly List<string> _contents = new ();
	private int _totalCollectedNotes = 0;
	


	public override void _Ready()
	{
		// Connect(Note.SignalName.NoteRead, new Callable(this, MethodName.OnNoteRead));
		LoadDirContents("res://Assets/Resources/NotesText/");
		ShuffleList(_contents);
	}
	
	// Fisher-Yates shuffle algorithm
	private void ShuffleList<T>(List<T> list)
	{
		int n = list.Count;
		Random random = new Random();

		for (int i = n - 1; i > 0; i--)
		{
			// Generate a random index between 0 and i (inclusive)
			int randomIndex = random.Next(0, i + 1);

			// Swap elements at randomIndex and i
			(list[i], list[randomIndex]) = (list[randomIndex], list[i]);
		}
	}
	
	private void LoadDirContents(string path)
	{
		using var dir = DirAccess.Open(path);
		if (dir != null)
		{
			dir.ListDirBegin();
			var fileName = dir.GetNext();
			while (fileName != "")
			{
				if (dir.CurrentIsDir())
				{
					GD.Print($"Found directory: {fileName}");
					LoadDirContents(path + fileName);
				}
				else
				{
					GD.Print($"Found file: {fileName}");
					using var file = FileAccess.Open(path + fileName, FileAccess.ModeFlags.Read);
					_contents.Add(file.GetAsText());
				}
				fileName = dir.GetNext();
			}
		}
		else
		{
			GD.Print("An error occurred when trying to access the path.");
		}
	}

	public void AddNote(Note note)
	{
		note.NoteRead += OnNoteRead;
		note.Destroy += RemoveNote;
		_notes.Add(note.Id.ToString(), note);
	}

	private void RemoveNote(string noteId)
	{
		_notes.Remove(noteId);
	}

	private void OnNoteRead(string noteId)
	{
		_totalCollectedNotes++;
		var note = _notes[noteId];
		RemoveNote(noteId);
		note.QueueFree();
		_UpdateNotes();
		GD.Print("Pages remaining: ", _neededPages - _totalCollectedNotes);
		_CheckWinCondition();
	}

	private void _CheckWinCondition()
	{
		if (_neededPages - _totalCollectedNotes <= 0)
		{
			GetTree().ChangeSceneToFile("res://Scenes/WinScreen/WinScreen.tscn");
		}
	}

	public string GetCurrentContent()
	{
		return _contents[_totalCollectedNotes];
	}

	private void _UpdateNotes()
	{
		foreach (var note in _notes.Values)
		{
			note.Content.Text = GetCurrentContent();
		}
	}

	private float FindClosestNote()
	{
		if (_notes.Count == 0)
		{
			return 0;
		}

		float min = float.MaxValue;

		foreach (var note in _notes.Values)
		{
			float distance = (_player.Position - note.Position).Length();
			min = Mathf.Min(distance, min);
		}

		return min;
	}

	public override void _PhysicsProcess(double delta)
	{
		//GD.Print("Closest Note: ", FindClosestNote());
	}
}
