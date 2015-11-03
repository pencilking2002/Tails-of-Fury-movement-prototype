using UnityEngine;
using System.Collections;

public class RomanCharController : MonoBehaviour {
	
	
	//---------------------------------------------------------------------------------------------------------------------------
	// Public Variables
	//---------------------------------------------------------------------------------------------------------------------------
		
	public float idleRotateSpeed = 10.0f;				// How fast the Squirrel will turn in idle mode
	public float speedDampTime = 0.05f;
	public float walkToRunDampTime = 1f;

	public float maxRunningRotation = 20f;
	
	public float runRotateSpeed = 10f;
	public float runSpeed = 10.0f;

	[Header("JUMPING")]
	[Range(0,100)]
	public float maxJumpForce = 0.8f;					// Maximum Y force to Apply to Rigidbody when jumping
	[Range(0,1)]
	public float jumpForceDeclineSpeed = 0.02f;			// How fast the jump force declines when jumping
	[Range(0,50)]
	public float jumpTurnSpeed = 20f;
	// Speed modifier of the character's Z movement wheile jumping
	[Range(0,400)]
	public float idleJumpForwardSpeed = 10f;
	[Range(0,400)]
	public float runningJumpForwardSpeed = 10f;
	
	//---------------------------------------------------------------------------------------------------------------------------
	//	Private Variables
	//---------------------------------------------------------------------------------------------------------------------------	

	private RomanCharState charState;
	private Animator animator;
	private Rigidbody rb;
	private Transform cam;
		
	private float yRot;				// The value to feed into the character's rotation in idle mode
	private float angle;			// used to check which way the character is rotating
	private float dir;				// The  result of the cross product of the player's rotation and the camera's rotation
	
	
	private Vector3 moveDirection;
	private Vector3 moveDirectionRaw;
	private Quaternion targetRot;		// the target rotation to achieve while in idle or running
	
	// Character rotation -------------
	private Vector3 camForward;
	private Quaternion camRot;
	
	// jumping ----------------------
	private float jumpForce;
	
//	private int facingAwayFromCam = 1; 
	private float forwardSpeed; 			// Temp var for forward speed
	private bool holdShift = false;
	
	void Start () 
	{
		charState = GetComponent<RomanCharState>();
		animator = GetComponent<Animator>();
		rb = GetComponent<Rigidbody>();
		cam = Camera.main.transform;
		
		
		jumpForce = maxJumpForce;
	}

	
	private void FixedUpdate ()
	{
		if (charState.IsClimbing())
			return;

		moveDirection = new Vector3(InputController.h, 0, InputController.v);
		moveDirectionRaw = new Vector3(InputController.rawH, 0, InputController.rawV);
		
		float speed = Mathf.Clamp01(moveDirectionRaw.sqrMagnitude);
		
		// if holding sprint modifier key and pressing LeftStick go straight into sprint mode without damping
		if (holdShift && speed > 0)
			animator.SetFloat ("Speed", speed + 2);
		
		else // Else go into run
			animator.SetFloat ("Speed", Mathf.Clamp01(speed), walkToRunDampTime, Time.deltaTime);
			
	
		
		//print (animator.GetFloat("Speed"));
		// Keep track of the character's direction compared to the camera
		//facingAwayFromCam = Vector3.Dot (transform.forward, cam.forward) < 0.0f ? -1 : 1;

		TurnCharToCamera();
		
		if (charState.IsIdle())
		{
			if (moveDirectionRaw != Vector3.zero)
				rb.MoveRotation(Quaternion.Slerp (transform.rotation, Quaternion.LookRotation(moveDirectionRaw), idleRotateSpeed * Time.deltaTime));
			
			//rb.velocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;

		}
		
		// Stop moving on the X and Z plane when landing
		if (charState.IsLanding())
			rb.velocity = Vector3.zero;

		else if (charState.IsJumping ())
		{
			if (moveDirectionRaw != Vector3.zero)
			{
				rb.MoveRotation(Quaternion.Slerp (transform.rotation, Quaternion.LookRotation(moveDirectionRaw), idleRotateSpeed * Time.deltaTime));
				
				// Move the character forward based on Vertical input and weather they are idle jumping or runnign jumping
				forwardSpeed = charState.IsIdleJumping() ? idleJumpForwardSpeed : runningJumpForwardSpeed;

				Vector3 vel = transform.forward * forwardSpeed * Mathf.Clamp01(moveDirectionRaw.sqrMagnitude) * Time.deltaTime;
				vel.y = rb.velocity.y;
				rb.velocity = vel;
				
			}
			else
			{
				rb.velocity = Vector3.Lerp(rb.velocity, new Vector3(0, rb.velocity.y, 0), 2 * Time.deltaTime);
			}
		
			 
			// Aadd a force downwards if the player releases the jump button
			// when the character is jumping up
			if (InputController.jumpReleased)
			{
				InputController.jumpReleased = false;

				if (rb.velocity.y > 0)
				{
					rb.AddForce (new Vector3 (0,  -5, 0), ForceMode.Impulse);
				}
			}
		}
		
	}
	
	private void OnAnimatorMove ()
	{
//		if (charState.IsClimbing())
//			return;
//			
		if (charState.IsRunning() && moveDirectionRaw != Vector3.zero)
		{		
			transform.rotation = Quaternion.Slerp (transform.rotation, Quaternion.LookRotation(moveDirectionRaw), runRotateSpeed * Time.fixedDeltaTime);
			animator.ApplyBuiltinRootMotion();
		}
	}


	/// <summary>
	/// - Get a vector of the camera's forward direction
	/// - Create a rotation based on the forward direction of the camera
	/// - Move moveDirectionRaw be in reference to the camera rotation so that if we go straight for example
	/// the character will also go straight
	/// </summary>
	private void TurnCharToCamera()
	{
		camForward = new Vector3(cam.forward.x, 0, cam.forward.z);
		camRot = Quaternion.LookRotation(camForward);
		moveDirectionRaw = camRot * moveDirectionRaw;
		//moveDirection = camRot * moveDirection;
	}
	
	private void OnTriggerEnter (Collider col)
	{
		if (col.CompareTag("JumpCollider") && rb.velocity.y < 0)
		{
			// Test to see if the ground is below the Squirrel. If it is, don't attach the follow
			if (!Physics.Raycast(transform.position, Vector3.down, 2))
				EventManager.OnCharEvent(GameEvent.AttachFollow);
		}
	}
	
	private void OnCollisionStay (Collision coll)
	{
		if (charState.IsFalling() && Vector3.Dot(coll.contacts[0].normal, Vector3.up) > 0.5f )
		{
			animator.SetTrigger("Land");
			EventManager.OnCharEvent(GameEvent.AttachFollow);
			
		}
	}


	// Events --------------------------------------------------------------------------------------------------------------------------------
	
	// Hook on to Input event
	private void OnEnable () 
	{ 
		EventManager.onInputEvent += Jump;
		EventManager.onInputEvent += Sprint;
		EventManager.onCharEvent += Disable;
	}
	private void OnDisable () 
	{ 
		EventManager.onInputEvent -= Jump;
		EventManager.onInputEvent -= Sprint;
		EventManager.onCharEvent += Disable;
	}
	
	private void Disable (GameEvent gameEvent)
	{
		if (gameEvent == GameEvent.StartEdgeClimbing)
		{
			this.enabled = false;
		}
		
		if (gameEvent == GameEvent.StopEdgeClimbing)
		{
			this.enabled = true;
		}
	}
	
	private void Sprint(GameEvent gameEvent)
	{
		if (gameEvent == GameEvent.SprintModifierDown)
		{
			holdShift = true;
		}
		else if (gameEvent == GameEvent.SprintModifierUp)
		{
			holdShift = false;
		}

	}

	// Trigger the jump animation and disable root motion
	public void Jump (GameEvent gameEvent)
	{
		if (gameEvent == GameEvent.Jump && (charState.IsIdle() || charState.IsRunning())) 
		{	
			EventManager.OnCharEvent(GameEvent.DetachFollow);
			
			JumpUpAnim ();
			rb.AddForce (new Vector3 (0,  maxJumpForce, 0), ForceMode.Impulse);
			jumpForce = maxJumpForce;

		}
	}
	
	// Trigger the jump up animation
	private void JumpUpAnim()
	{

		if (charState.IsIdle())
		{
			charState.SetState(RomanCharState.State.IdleJumping);
			animator.SetTrigger ("IdleJump");
		}
		else if (charState.IsRunning())
		{
			charState.SetState(RomanCharState.State.RunningJumping);
			animator.SetTrigger ("RunningJump");
		}
	}
	
	//---------------------------------------------------------------------------------------------------------------------------
	// Public Methods
	//---------------------------------------------------------------------------------------------------------------------------	
	
	// Enable Root motion
	public void ApplyRootMotion (bool apply)
	{
		animator.applyRootMotion = apply;
	}

	private void ResetJumpForce ()
	{
		jumpForce = maxJumpForce;
	}
	
	
}
