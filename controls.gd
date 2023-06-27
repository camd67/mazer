extends Control

@onready var back_to_menu_btn = $BackToMenuBtn

func _ready():
	back_to_menu_btn.grab_focus()
	var audio_manager = get_tree().root.get_node("/root/UiAudioManager")
	audio_manager.RegisterBack(back_to_menu_btn)


func _on_back_to_menu_btn_pressed():
	get_tree().change_scene_to_file("res://main_menu.tscn")

func _unhandled_key_input(event):
	if event is InputEventKey and event.pressed:
		if event.keycode == KEY_ESCAPE:
			_on_back_to_menu_btn_pressed()
