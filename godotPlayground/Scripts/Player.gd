extends Actor

onready var platform_detector = $PlatformDetector

# Declare member variables here. Examples:
const JUMP_FORCE = -600
const MAX_SPEED = 500
const GRAVITY = Vector2(0, 15);
const MAX_VELOCITY = Vector2(1200, 1200)


# physics
var velocity = Vector2.ZERO
var acceleration = 3
var deceleration = 1.5

var isAttacking = false;


var form = "Skeleton"


# Called when the node enters the scene tree for the first time.\
func _ready():
	pass # Replace with function body.

func handle_input(delta):

	if Input.is_action_pressed("my_left"): # move left
		velocity.x = velocity.x - MAX_SPEED*delta*acceleration
		$Skeleton.flip_h = true
		if !isAttacking: $Skeleton.play("Walk")
	elif Input.is_action_pressed("my_right"): # move right
		velocity.x = velocity.x + MAX_SPEED*delta*acceleration
		$Skeleton.flip_h = false
		if !isAttacking: $Skeleton.play("Walk")
		
	else:
		if(velocity.x > 0):
			velocity.x = velocity.x - MAX_SPEED*delta*deceleration
			if(velocity.x < 0): velocity.x = 0.0
		elif (velocity.x < 0):
			velocity.x = velocity.x + MAX_SPEED*delta*deceleration
			if(velocity.x > 0): velocity.x = 0.0
		else:
			velocity.x = 0;

	if Input.is_action_pressed("my_jump"): # jump
#		if is_on_floor():
#			apply_force(Vector2(0, JUMP_FORCE))
		if platform_detector.is_colliding():
			velocity.y = JUMP_FORCE
	
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
	# handles input
	handle_input(delta)
	
	#limits velocity
	if(velocity.x > MAX_SPEED):
		velocity.x = MAX_SPEED
	elif(velocity.x < -MAX_SPEED):
		velocity.x = -MAX_SPEED

	# applies gravity
	velocity += GRAVITY
	
	# applies movement
	move_and_slide(velocity, Vector2.UP)
	
	# double checks animation
	if abs(velocity.x) < 10 && !isAttacking:
		$Skeleton.play("Idle")
	elif abs(velocity.x) > 10 && !isAttacking:
		$Skeleton.play("Walk")


func _on_Skeleton_animation_finished():
		if isAttacking:
			isAttacking = false
			$Skeleton.animation = "Idle"
