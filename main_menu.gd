extends Control

@onready var playBtn = $AspectRatioContainer/VBoxContainer/PlayBtn
@onready var controls_btn = $AspectRatioContainer/VBoxContainer/ControlsBtn
@onready var credits_btn = $AspectRatioContainer/VBoxContainer/CreditsBtn
@onready var quit_btn = $AspectRatioContainer/VBoxContainer/QuitBtn

func _ready():
	playBtn.grab_focus()
	var audio_manager = get_tree().root.get_node("/root/UiAudioManager")
	audio_manager.RegisterForward([playBtn, controls_btn, credits_btn, quit_btn])

func _on_play_btn_pressed():
	get_tree().change_scene_to_file("res://player_selection.tscn")

func _on_quit_btn_pressed():
	get_tree().quit()


func _on_controls_btn_pressed():
	get_tree().change_scene_to_file("res://controls.tscn")


func _on_credits_btn_pressed():
	get_tree().change_scene_to_file("res://credits.tscn")
