extends Actor


# Declare member variables here. Examples:
var jumpvelocity = -150
var gravityscale = 1
var walkspeed = 500

# 0 is left, 1 is right
var direction = Vector2.ZERO
var form = "Skeleton"

# Called when the node enters the scene tree for the first time.\
func _ready():
	pass # Replace with function body.

func get_input():
	if Input.is_action_pressed("ui_left"):
		direction.x = -1
		$Skeleton.play("Walk")
		
	elif Input.is_action_pressed("ui_right"):
		direction.x = 1
		$Skeleton.play("Walk")
		
	elif Input.is_action_pressed("ui_accept"):
		pass
		
	else:
		$Skeleton.play("Idle")
		direction.x = 0
		
		


# Called every frame. 'delta' is the elapsed time since the previous frame.
#func _process(delta):
#	pass
func calculate_move_velocity():
	return speed * direction
	
func _physics_process(delta):
	get_input()

		
	#velocity.y += gravity * delta
	
	if direction.x < 0:
		$Skeleton.flip_h = true
	elif direction.x > 0:
		$Skeleton.flip_h = false
	var velocity = calculate_move_velocity()
	#velocity.y = 2
	move_and_collide(velocity)
	
