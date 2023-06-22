extends Control

@onready var back_to_menu_btn = $BackToMenuBtn
@onready var credits_container = $ScrollContainer/CreditsContainer
@onready var show_licenses_btn = $ShowLicensesBtn

@onready var credits_header = $ScrollContainer/CreditsContainer/CreditsHeader
@onready var credits_lbl = $ScrollContainer/CreditsContainer/CreditsLbl
@onready var license_lbl = $ScrollContainer/CreditsContainer/LicenseLbl

var remaining_licenses: Array
var license_info = Engine.get_license_info()

func _ready():
	back_to_menu_btn.grab_focus()
	credits_container.remove_child(credits_header)
	credits_container.remove_child(credits_lbl)
	credits_container.remove_child(license_lbl)
	generate_credits()

func _process(delta):
	# This is not optimized at all. It lags a lot but no one's probably
	# pressing this anyways...
	if remaining_licenses.size() > 0:
		var license_key = remaining_licenses.pop_back()
		add_license(license_key + "\n" + license_info[license_key])


func _on_back_to_menu_btn_pressed():
	get_tree().change_scene_to_file("res://main_menu.tscn")


func _on_show_licenses_btn_pressed():
	remove_child(show_licenses_btn)
	generate_licenses()
	
func add_header(text):
	var header = credits_header.duplicate()
	header.text = text
	credits_container.add_child(header)

func add_credits(text):
	var label = credits_lbl.duplicate()
	label.text = text
	credits_container.add_child(label)
	
func add_license(text):
	var label = license_lbl.duplicate()
	label.text = text
	credits_container.add_child(label)
	
func generate_credits():
	add_header("Mazer")
	add_credits("Created by: CamD67")
	add_header("Assets provided by")
	add_credits("Tilesheet: https://www.kenney.nl/assets/tiny-dungeon\n" \
	+ "Mushroom sprite: https://opengameart.org/content/tiny-mushrooms-16x-minecraft-style\n" \
	+ "Input icons: https://thoseawesomeguys.com/prompts/\n")	

func generate_licenses():
	add_header("Mazer License")
	add_license(FileAccess.open("res://LICENSE.txt", FileAccess.READ).get_as_text())
	add_header("Godot License")
	add_license(Engine.get_license_text())
	remaining_licenses = license_info.keys()
	
	
