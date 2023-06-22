extends Control

@onready var playBtn = $AspectRatioContainer/VBoxContainer/PlayBtn

func _ready():
	playBtn.grab_focus()

func _on_play_btn_pressed():
	get_tree().change_scene_to_file("res://player_selection.tscn")

func _on_quit_btn_pressed():
	get_tree().quit()


func _on_controls_btn_pressed():
	get_tree().change_scene_to_file("res://controls.tscn")


func _on_credits_btn_pressed():
	get_tree().change_scene_to_file("res://credits.tscn")
