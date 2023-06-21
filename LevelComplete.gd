extends Control

@onready var results_lbl: Label = %ResultsLbl
@onready var play_again_btn = %PlayAgainBtn

signal level_restart()

func update_results_time(timeText):
	results_lbl.text = "Your time was: " + timeText

func _on_play_again_btn_pressed():
	level_restart.emit()

func _on_quit_btn_pressed():
	get_tree().quit()

func _on_main_menu_btn_pressed():
	get_tree().change_scene_to_file("res://main_menu.tscn")


func _on_visibility_changed():
	if (visible):
		play_again_btn.grab_focus()
