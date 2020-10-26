extends Actor


# Declare member variables here. Examples:
var jumpvelocity = -150
var gravityscale = 1
var walkspeed = 500
var FLOOR_DETECT_DISTANCE = 20
# 0 is left, 1 is right
var direction = Vector2.ONE
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

	var snap_vector = Vector2.DOWN * FLOOR_DETECT_DISTANCE if direction.y == 0.0 else Vector2.ZERO
	var velocity = calculate_move_velocity()
	#velocity.y += gravity * delta
	var is_on_platform = $PlatformDetector.is_colliding()
	print(is_on_platform)
	velocity = move_and_slide_with_snap(
		velocity, snap_vector, FLOOR_NORMAL, not is_on_platform, 4, 0.9, false
	)
	
	if direction.x < 0:
		$Skeleton.flip_h = true
	elif direction.x > 0:
		$Skeleton.flip_h = false
	var collision = move_and_slide(velocity)
	#print(collision)
	
