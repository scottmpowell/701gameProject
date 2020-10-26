extends Actor

onready var platform_detector = $PlatformDetector

# Declare member variables here. Examples:
const JUMP_FORCE = -120
const SPEED = 10
const GRAVITY = Vector2(0, 15);
const MAX_VELOCITY = Vector2(1200, 1200)

# physics
var acceleration = Vector2.ZERO
var velocity = Vector2.ZERO
var direction = Vector2.ZERO # 0 is left, 1 is right

var isAttacking = false;


var form = "Skeleton"


# Called when the node enters the scene tree for the first time.\
func _ready():
	pass # Replace with function body.

func get_input():

	if Input.is_action_pressed("my_left"): # move left
		direction.x = -1
		apply_force(Vector2(-SPEED, 0))
		$Skeleton.flip_h = true
		if !isAttacking: $Skeleton.play("Walk")
		
	elif Input.is_action_pressed("my_right"): # move right
		direction.x = 1
		apply_force(Vector2(SPEED, 0))
		$Skeleton.flip_h = false
		if !isAttacking: $Skeleton.play("Walk")

	if Input.is_action_pressed("my_jump"): # jump
		if platform_detector.is_colliding():
			apply_force(Vector2(0, JUMP_FORCE))
	
	if Input.is_action_pressed("my_attack"): # attack
		if !isAttacking:
			isAttacking = true
			$Skeleton.play("Attack")


# Called every frame. 'delta' is the elapsed time since the previous frame.
#func _process(delta):
#	pass


#func calculate_move_velocity(speed):
#	return WALKSPEED * velocity


func _physics_process(delta):
	apply_force(GRAVITY)
	# handles input
	get_input()

	# applies gravity and updates velocity
	
	velocity += acceleration


	# applies movement
	move_and_slide(velocity)

	# resets acceleration and applies a sort of friction
	
	acceleration *= Vector2.ZERO
	velocity.x = lerp(0, velocity.x, .98)
	
	# double checks animation
	if abs(velocity.x) < 10 && !isAttacking:
		$Skeleton.play("Idle")
		direction.x = 0
	elif abs(velocity.x) > 10 && !isAttacking:
		$Skeleton.play("Walk")


# applies a force to the player
func apply_force(force):
	acceleration += force;


func _on_Skeleton_animation_finished():
		if isAttacking:
			isAttacking = false
