using UnityEngine;
using System.Collections;
using Holoville.HOTween;
using Holoville.HOTween.Plugins;

// This controller class is inspired by Langman Controller.
[System.Serializable]
public class Movement {
	public float runSpeed = 10.0f;
	
	public float slideFactor = 0.05f;
	[@System.NonSerialized]
	public float slideX = 0.0f;
	
	public float gravity = 60.0f;
	public float maxFallSpeed = 20.0f;
	
	public float speedSmoothing = 20.0f;
	
	[@System.NonSerialized]
	public Vector3 direction = Vector3.zero;
	[@System.NonSerialized]
	public float verticalSpeed = 0.0f;
	[@System.NonSerialized]
	public float speed = 0.0f;
	[@System.NonSerialized]
	public bool isMoving = false;
	[@System.NonSerialized]
	public CollisionFlags collisionFlags;
	[@System.NonSerialized]
	public Vector3 velocity;
	[@System.NonSerialized]
	public float hangTime = 0.0f;
}
[System.Serializable]
public class Jump {
	public bool enabled = true;
	
	public float height = 0.5f;
	public float extraHeight = 1.6f;
	public float speedSmoothing = 3.0f;
	public float jumpSpeed = 9.5f;
	[@System.NonSerialized]
	public float repeatTime = 0.05f;
	
	[@System.NonSerialized]
	public float timeout = 0.15f;
	
	[@System.NonSerialized]
	public bool jumping = false;
	
	[@System.NonSerialized]
	public bool reachedApex = false;
	
	[@System.NonSerialized]
	public float lastButtonTime = -10.0f;
	
	[@System.NonSerialized]
	public float lastGroundedTime = -10.0f;
	
	[@System.NonSerialized]
	public float groundingTimeout = 0.1f;
	
	[@System.NonSerialized]
	public float lastTime = 1.0f;
	
	[@System.NonSerialized]
	public float lastStartHeight = 0.0f;
	
	[@System.NonSerialized]
	public bool touchedCeiling = false;
	
	[@System.NonSerialized]
	public bool buttonReleased = true;
}

public class PlayerScript : MonoBehaviour {
	public bool canControl = true;
	public Movement movement;
	public Jump jump;
	
	private CharacterController controller;
	public bool isFrozen = false;
	public bool isKilled = false;
	
	public void Spawn (Vector3 pos) {
		movement.verticalSpeed = 0.0f;
		movement.speed = 0.0f;
		transform.position = pos;
		canControl = true;
		isFrozen = false;
	}
	
	public void Unspawn() {
		canControl = false;
	}
	
	public void Freeze() {
		if(isFrozen) {
			isFrozen = false;
			canControl = true;
		} else {
			isFrozen = true;
			canControl = false;
		}
	}
	
	public void Reset() {
		isFrozen = true;
		canControl = false;
	}
	
	void Awake () {
		movement.direction = transform.TransformDirection(Vector3.forward);
		controller = GetComponent<CharacterController>();
		isFrozen = true;
		canControl = false;
	}
	
	void Move () {
		float h = Input.GetAxisRaw("Horizontal");
		
		if(!canControl)
			h = 0.0f;
		
		movement.isMoving = Mathf.Abs (h) > 0.1f;
		
		if(movement.isMoving) 
			movement.direction = new Vector3(h, 0, 0);
		
		float curSmooth = 0.0f;
		float targetSpeed = Mathf.Min (Mathf.Abs (h), 1.0f);
		
		if(controller.isGrounded){
			curSmooth = movement.speedSmoothing * Time.smoothDeltaTime;
			targetSpeed *= movement.runSpeed;
			movement.hangTime = 0.0f;
		} else {
			curSmooth = jump.speedSmoothing * Time.smoothDeltaTime;
			targetSpeed *= jump.jumpSpeed;
			movement.hangTime += Time.smoothDeltaTime;
		}
		
		movement.speed = Mathf.Lerp (movement.speed, targetSpeed, curSmooth);
		
	}
	
	public IEnumerator DeathAnimation() {
		canControl = false;
		
		Sequence seq = new Sequence();
		seq.Append(HOTween.To (transform, 0.3f, new TweenParms().Prop("position", new Vector3(0.0f, 5.0f, 0.0f), true).Ease(EaseType.EaseOutCubic)));
		seq.Append(HOTween.To (transform, 0.5f, new TweenParms().Prop("position", new Vector3(0.0f, -20.0f, 0.0f), true).Ease(EaseType.EaseInCubic)));
		seq.Play();
		
		while(!seq.isComplete){
			yield return null;
		}
		
	}
	
	void Animate() {
		
	}
	
	bool JustUngrounded() {
		return (Time.time < (jump.lastGroundedTime + jump.groundingTimeout) && jump.lastGroundedTime > jump.lastTime);
	}
	
	void Jump() {
		if(Input.GetButtonDown("Jump") && canControl) {
			jump.lastButtonTime = Time.time;
		}
		
		if(jump.lastTime + jump.repeatTime > Time.time) {
			return;
		}
		
		bool isGrounded = controller.isGrounded;
		
		if(isGrounded || JustUngrounded()) {
			if(isGrounded)
				jump.lastGroundedTime = Time.time;
			
			if(jump.enabled && Time.time < jump.lastButtonTime + jump.timeout) {
				movement.verticalSpeed = CalculateJumpSpeed(jump.height);
				SendMessage("DidJump", SendMessageOptions.DontRequireReceiver);
			}
		}
	}
	
	void ApplyGravity() {
		bool jumpButton = Input.GetButton("Jump");
		if(!canControl)
			jumpButton = false;
		
		if(jump.jumping && !jump.reachedApex && movement.verticalSpeed <= 0.0f) {
			jump.reachedApex = true;
		}
		
		if(!jump.touchedCeiling && IsTouchingCeiling()){
			jump.touchedCeiling = true;
		}
		
		if(!jumpButton)
			jump.buttonReleased = true;
		
		bool extraJump = jump.jumping && movement.verticalSpeed > 0.0f && jumpButton && !jump.buttonReleased && transform.position.y < jump.lastStartHeight + jump.extraHeight && !jump.touchedCeiling;
		if (extraJump)
			return;
		else if(controller.isGrounded)
			movement.verticalSpeed = -movement.gravity * Time.smoothDeltaTime;
		else
			movement.verticalSpeed -= movement.gravity * Time.smoothDeltaTime;
		
		movement.verticalSpeed = Mathf.Max (movement.verticalSpeed, -movement.maxFallSpeed);
		if(isKilled) movement.verticalSpeed = 0.0f;
	}
	
	float CalculateJumpSpeed(float height) {
		return Mathf.Sqrt(2 * height * movement.gravity);
	}
	
	void DidJump() {
		jump.jumping = true;
		jump.reachedApex = false;
		jump.lastTime = Time.time;
		jump.lastStartHeight = transform.position.y;
		jump.lastButtonTime = -10;
		jump.touchedCeiling = false;
		jump.buttonReleased = false;
	}
	
	bool IsTouchingCeiling() {
		return (movement.collisionFlags & CollisionFlags.CollidedAbove) != 0;
	}
	
	// Update is called once per frame
	void Update () {
		transform.position = new Vector3(transform.position.x, transform.position.y, 0.0f);
		
		Move();
		Animate();
		ApplyGravity();
		Jump();
		
		Vector3 lastPos = transform.position;
		Vector3 movOffset = (movement.direction * movement.speed) + new Vector3(0.0f, movement.verticalSpeed, 0.0f);
		movOffset *= Time.smoothDeltaTime;
		movOffset.x += movement.slideX * movement.slideFactor;
		movement.slideX = 0.0f;
		
		//Move!
		movement.collisionFlags = controller.Move (movOffset);
		movement.velocity = (transform.position - lastPos) / Time.smoothDeltaTime;
		
		if(controller.isGrounded) {
			if(jump.jumping) {
				jump.jumping = false;
				
				Vector3 jumpMoveDirection = movement.direction * movement.speed;
				if(jumpMoveDirection.sqrMagnitude > 0.01f)
					movement.direction = jumpMoveDirection.normalized;
			}
		}
	}
	
	void OnTriggerEnter(Collider other) {
		if(!GameEngine.Instance.isTransitioning && other.gameObject.name == "Trigger")
			StartCoroutine(GameEngine.Instance.NextZone());
		else if(!GameEngine.Instance.isKilled && !GameEngine.Instance.isTransitioning && other.gameObject.name == "explos"){
			StartCoroutine(GameEngine.Instance.PlayDeath(1));
		}
	}
	
	void OnControllerColliderHit (ControllerColliderHit hit) {
		if(GameEngine.Instance.isStarted && isFrozen) {
			Entity e = hit.gameObject.GetComponent<Entity>();
			if(!GameEngine.Instance.isKilled && e != null && (e.GetVelocity() > 1.0f && (movement.collisionFlags & CollisionFlags.CollidedAbove) != 0)){
				StartCoroutine(GameEngine.Instance.PlayDeath(0));
			}
		}
	}
}
