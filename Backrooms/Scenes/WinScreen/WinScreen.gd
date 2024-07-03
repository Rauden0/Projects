extends Control

func _ready():
	Input.mouse_mode = Input.MOUSE_MODE_VISIBLE;

func _on_button_button_down():
	get_tree().change_scene_to_file("res://debug.tscn");
