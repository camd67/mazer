extends Control

signal pause_menu_closed

@onready var return_btn = %returnBtn

func _on_quit_button_pressed():
	get_tree().quit()

func _on_main_menu_btn_pressed():
	get_tree().change_scene_to_file("res://main_menu.tscn")

func _on_return_btn_pressed():
	pause_menu_closed.emit()


func _on_visibility_changed():
	if (visible):
		return_btn.grab_focus()
