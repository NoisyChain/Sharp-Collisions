extends Node

var fixed : Fix64GD
# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	fixed = Fix64GD.new()
	fixed = Fix64GD.pi()
	print(fixed.to_float())
	fixed.add(Fix64GD.one())
	print(fixed.to_float())
	fixed.from_int(270)
	fixed.deg2rad()
	print(fixed.to_float())
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass
