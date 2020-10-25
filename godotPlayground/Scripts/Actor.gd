# Actor is a base script for all characters
class_name Actor
extends KinematicBody2D

#export var speed = Vector2(150.0, 350.0)
export var speed = Vector2(5.0, 350.0)
onready var gravity = ProjectSettings.get("physics/2d/default_gravity")


# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
#func _process(delta):
#	pass
