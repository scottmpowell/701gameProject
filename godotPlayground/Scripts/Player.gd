extends KinematicBody2D


# Declare member variables here. Examples:
var jumpvelocity = 150
var gravityscale = 1
var walkspeed = 200
var velocity = Vector2()

# 0 is left, 1 is right
var direction = 1
var form = "Skeleton"

# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.

func get_input():
	if Input.is_action_pressed("ui_left"):
		velocity.x = -walkspeed
		direction = 0
		$Skeleton.play("Walk")
		
	elif Input.is_action_pressed("ui_right"):
		velocity.x = walkspeed
		direction = 1
		$Skeleton.play("Walk")
		
	elif Input.is_action_pressed("ui_accept"):
			$Skeleton.play("Attack")
	else:
		$Skeleton.play("Idle")
		velocity.x = 0
		


# Called every frame. 'delta' is the elapsed time since the previous frame.
#func _process(delta):
#	pass
func _physics_process(delta):
	get_input()
	velocity.y = 20
	if direction:
		$Skeleton.flip_h = false
	else:
		$Skeleton.flip_h = true
		
	var motion = velocity
	move_and_collide(motion)
	
