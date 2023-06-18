extends Control

@onready var results_lbl: Label = $CenterContainer/VBoxContainer/ResultsLbl

signal level_restart()

func update_results_time(time):
	results_lbl.text = "Your time was: " + Time.get_time_string_from_unix_time(time)


func _on_play_again_btn_pressed():
	level_restart.emit()

func _on_quit_btn_pressed():
	get_tree().quit()

func _on_main_menu_btn_pressed():
	get_tree().change_scene_to_file("res://main_menu.tscn")