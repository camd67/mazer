extends Control

@onready var character_list: ItemList = $CharacterList
@onready var audio_manager = $"/root/UiAudioManager"

func _ready():
	character_list.grab_focus()
	character_list.select(0)

func _on_character_list_item_activated(index):
	audio_manager.PlayPressed()
	var itemTexture = character_list.get_item_icon(index)
	var atlasLocation = itemTexture.region.position
	get_node("/root/PlayerState").playerAtlas = atlasLocation
	get_tree().change_scene_to_file("res://main.tscn")


func _on_character_list_item_selected(_index):
	audio_manager.PlayFocusEntered()
