extends Control

func _on_button_2d_pressed() -> void:
	# Go to 2D scene (not existent yet)
	pass # Replace with function body.

func _on_button_2d_2_pressed() -> void:
	# Go to 2D scene with 3D models
	load_scene("res://Scenes/TestScene2D.tscn")

func _on_button_3d_pressed() -> void:
	# Go to 3D scene
	load_scene("res://Scenes/TestScene3D.tscn")

func _on_button_bm_1_pressed() -> void:
	load_scene("res://Scenes/Benchmark1.tscn")
	# Go to Benchmark scene 1

func _on_button_bm_2_pressed() -> void:
	load_scene("res://Scenes/Benchmark2.tscn")
	# Go to Benchmark scene 2

func _on_button_bm_3_pressed() -> void:
	# Go to Benchmark scene 3
	load_scene("res://Scenes/Benchmark3.tscn")

func _on_button_bm_4_pressed() -> void:
	# Go to Benchmark scene 4 (not existent yet)
	load_scene("res://Scenes/Benchmark4.tscn")

func load_scene(path : String) -> void:
	get_tree().change_scene_to_file(path)
