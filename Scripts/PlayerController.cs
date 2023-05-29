using System.Collections;
using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

  public bool canMove {get; private set;} = true;
	//Lamda if can sprint is true and sprint key is pressed.
	private bool isSprinting => canSprint && currentInput != Vector2.zero && Input.GetKey(sprintKey);
	private bool shouldJump => Input.GetKeyDown(jumpKey) && characterController.isGrounded;
	private bool shouldCrouch => Input.GetKeyDown(crouchKey) && !duringCrouchingAnimation && characterController.isGrounded;

	[Header("Functional Options")]
	[SerializeField] private bool canSprint = true;
	[SerializeField] private bool canJump = true;
	[SerializeField] private bool canCrouch = true;
	[SerializeField] private bool canUseHeadBob = false;
	[SerializeField] private bool willSlideonslopes = true;
	[SerializeField] private bool canZoom = true;
	[SerializeField] private bool canInteract = true;
	[SerializeField] private bool useStamina = true;

	[Header("Health Options")]
	[SerializeField] private float maxHealth = 100.0f;
	[SerializeField] private float timeBeforeRegenStarts = 2.0f;
	[SerializeField] private float healthValueIncrement = 1.0f;
	[SerializeField] private float HeathTimeIncrement = 0.1f;
	private float currentHealth;
	private Coroutine regeneratingHealth;

	[Header("Stamina Options")]
	[SerializeField] private float maxStamina = 100.0f;
	[SerializeField] private float timeBeforeStaminaRegenStarts = 5.0f;
	[SerializeField] private float staminaValueIncrement = 5.0f;
	[SerializeField] private float staminaTimeIncrement = 0.1f;
	[SerializeField] private float staminaUseMultiplier = 0.1f;
	private float currentStamina;
	private Coroutine regeneratingStamina;
	public static Action<float> onStaminaChange;
	
	public static Action<float> onTakeDamage;
	public static Action<float> onDamage;
	public static Action<float> onHealDamage;

	[Header("Controlls")]
	[SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
	[SerializeField] private KeyCode jumpKey = KeyCode.Space;
	[SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;
	[SerializeField] private KeyCode zoomKey = KeyCode.Mouse1;
	[SerializeField] private KeyCode interactKey = KeyCode.E;

  [Header("Movement Parameter")]
  [SerializeField] private float walkSpeed = 3.0f;
	[SerializeField] private float sprintSpeed = 6.0f;
	[SerializeField] private float CrouchSpeed = 2.0f;
	[SerializeField] private float slopeSpeed = 8.0f;
  
  [Header("Look Parameter")]
  [SerializeField] private float lookSpeedX = 2.0f;
  [SerializeField] private float lookSpeedY = 2.0f;
  [SerializeField] private float lowerLookLimit = 90.0f;
  [SerializeField] private float upperLookLimit = 90.0f;

	[Header("Jump Perameter")]
	[SerializeField] private float JumpForce = 8.0f;
	[SerializeField] private float gravity = 20.0f;

	[Header("Crouch Perameter")]
	//Crouch height
	[SerializeField] private float crouchHeight = 0.5f;
	[SerializeField] private float standingHeight = 2.0f;
	[SerializeField] private float timeToCrouch = 0.5f;
	[SerializeField] private Vector3 crouchingcenter = new Vector3(0, 0.5f , 0);
	[SerializeField] private Vector3 standingCenter = new Vector3(0, 0 , 0);
	private bool isCrouching ;
	private bool duringCrouchingAnimation;

	[Header("HeadBob Perameters")]
	[SerializeField] private float walkbobspeed = 14.0f;
	[SerializeField] private float walkbobAmount = 0.05f;
	[SerializeField] private float sprintbobspeed = 28.0f;
	[SerializeField] private float sprintbobAmount = 0.11f;
	[SerializeField] private float Crouchbobspeed = 8f;
	[SerializeField] private float crouchbobAmount = 0.25f;
	private float defaultcameraY = 0f;
	private float timer;

	[Header("Zoom Perameters")]
	[SerializeField] private float timeToZoom = 0.25f;
	[SerializeField] private float zoomFOV = 50f;
	private float defaultFOV;
	private Coroutine zoomRoutine;

	private Vector3 hitPointNormal;
	private bool isSliding{
		get{	// Raycast is a single line and should be another type? that dosnt stop the slide once not interacting with the slope.
				if (characterController.isGrounded && Physics.Raycast(transform.position, Vector3.down, out RaycastHit slopeHit, 1.5f ))
				{
					hitPointNormal = slopeHit.normal;
					return Vector3.Angle(hitPointNormal, Vector3.up) > characterController.slopeLimit;
			}else
			{
				return false;
			}
		}
	}

	[Header("Interaction")]
	[SerializeField] private Vector3 interactionRayPoint = default;
	[SerializeField] private float interactionDistance = default;
	[SerializeField] private LayerMask interactionLayer = default;
	// Here interactable not interactableobject
	public Interactable currentInteractable;

	private Camera playerCamera;
	private CharacterController characterController;

	private Vector3 moveDirecion;
	private Vector2 currentInput;

	private float rotationX = 0;

	void OnEnable()
	{
		onTakeDamage += applyDammage;
	}

	void OnDisable()
	{
			onTakeDamage -= applyDammage;
	}

	// Change to Awake?
  void Start(){

    playerCamera = GetComponentInChildren<Camera>();
		characterController = GetComponent<CharacterController>();
		defaultcameraY = playerCamera.transform.localPosition.y;
		defaultFOV = playerCamera.fieldOfView;
		currentHealth = maxHealth;
		currentStamina = maxStamina;
		
		//Lock Cursor to game window
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

  }
  void Update(){

    if (canMove){
			HandleMovementInput();
			handleMouseLook();
			
			if(canJump)
				handleJump();
			
			if (canCrouch)
				handleCrouch();
			
			if (canUseHeadBob )
				handleHeadbob();

			if (canZoom)
				handleZoom();

			if (canInteract){
				handleInteractionCheck();
				handleInteractionInput();
			}
			if (useStamina)
				handleStamina();

			applyFinalMovement();
		}
	
  }
	private void HandleMovementInput(){
		//checks if sprinting if no walk
		currentInput = new Vector2((isCrouching? CrouchSpeed: isSprinting ? sprintSpeed : walkSpeed) * Input.GetAxis("Vertical"), (isCrouching ? CrouchSpeed: isSprinting ? sprintSpeed : walkSpeed) * Input.GetAxis("Horizontal"));

		float moveDirecionY = moveDirecion.y;
		moveDirecion = (transform.TransformDirection(Vector3.forward) * currentInput.x) + (transform.TransformDirection(Vector3.right) * currentInput.y);
		moveDirecion.y = moveDirecionY;
	}
	private void handleMouseLook(){
		//Look up and down
		rotationX -= Input.GetAxis("Mouse Y") * lookSpeedY;
		rotationX = Mathf.Clamp(rotationX, -upperLookLimit, lowerLookLimit);
		playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);                      
		//Rotate Character to look left and right
		transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X")*lookSpeedX, 0);

	}
	private void handleJump(){
		if(shouldJump)
			moveDirecion.y = JumpForce;
	}

	private void handleCrouch(){
		if (shouldCrouch)
			StartCoroutine(crouchStand());
	}

	private void handleHeadbob(){
		if (!characterController.isGrounded) return;
		
		if(Mathf.Abs(moveDirecion.x) > 0.1f || Mathf.Abs(moveDirecion.z) > 0.1f){
			timer += Time.deltaTime * (isCrouching ? Crouchbobspeed : isSprinting ? sprintbobspeed : walkbobspeed);
			playerCamera.transform.localPosition = new Vector3(
				playerCamera.transform.localPosition.x, 
				defaultcameraY + Mathf.Sin(timer) * (isCrouching ? crouchbobAmount : isSprinting ? sprintbobAmount: walkbobAmount), playerCamera.transform.localPosition.z);
		}
	}
	private void handleStamina(){
		if(isSprinting && currentInput != Vector2.zero){

			if(regeneratingStamina != null){
				StopCoroutine(regeneratingStamina);
				regeneratingStamina = null;
			}
			currentStamina -= staminaUseMultiplier * Time.deltaTime;

			if(currentStamina < 0)
				currentStamina = 0;

			onStaminaChange?.Invoke(currentStamina);

			if (currentStamina <= 0)
				canSprint = false;
		}
		if(!isSprinting && currentStamina < maxStamina && regeneratingStamina == null){
			regeneratingStamina = StartCoroutine(RegenerateStamina());
		}
	}

	private void handleZoom(){
		if(Input.GetKeyDown(zoomKey)){
			if (zoomRoutine != null){
				StopCoroutine(zoomRoutine);
				zoomRoutine = null;
			}
			zoomRoutine = StartCoroutine(toggleZoom(true));
		}
				if(Input.GetKeyUp(zoomKey)){
			if (zoomRoutine != null){
				StopCoroutine(zoomRoutine);
				zoomRoutine = null;
			}
			zoomRoutine = StartCoroutine(toggleZoom(false));
		}
	}
	private void handleInteractionCheck(){
		if(Physics.Raycast(playerCamera.ViewportPointToRay(interactionRayPoint), out RaycastHit hit, interactionDistance)){
			if(hit.collider.gameObject.layer == 9 && (currentInteractable == null || hit.collider.gameObject.GetInstanceID() != currentInteractable.GetInstanceID())){
				hit.collider.TryGetComponent(out currentInteractable);

			if (currentInteractable)
				currentInteractable.onFocus();
			}

			//
			Debug.DrawRay(playerCamera.ViewportPointToRay(interactionRayPoint).origin, playerCamera.ViewportPointToRay(interactionRayPoint).direction * interactionDistance, Color.red);

		}else if(currentInteractable){
			currentInteractable.onLoseFocus();
			currentInteractable = null;
		}

	}
	private void handleInteractionInput(){
		if (Input.GetKeyDown(interactKey) && currentInteractable != null && Physics.Raycast(playerCamera.ViewportPointToRay(interactionRayPoint), out RaycastHit hit, interactionDistance, interactionLayer)){
			currentInteractable.onInteract();
			Debug.Log("interaction key pressed");
		}
	}
	// here
	private void applyDammage(float dmg){
		currentHealth -= dmg;
		onDamage?.Invoke(currentHealth);
		if(currentHealth <= 0){
			killPlayer();
		}else if(regeneratingHealth != null){
			StopCoroutine(regeneratingHealth);
		}
		regeneratingHealth = StartCoroutine(regenHeath());
	}
	private void killPlayer(){
		currentHealth = 0;
		if (regeneratingHealth !=null)
		StopCoroutine(regeneratingHealth);
		//What Happens on death?
		print("dead");
	}
	private void applyFinalMovement(){
		if(!characterController.isGrounded)

				moveDirecion.y -= gravity * Time.deltaTime;

		if (willSlideonslopes && isSliding)
				moveDirecion += new Vector3(hitPointNormal.x , -hitPointNormal.y, hitPointNormal.z) * slopeSpeed;

		characterController.Move(moveDirecion * Time.deltaTime);
		
	}

	private IEnumerator crouchStand(){
		//if(isCrouching && Physics.Raycast(playerCamera.transform.position, Vector3.up, 1f))
			//yield break;
			
		duringCrouchingAnimation = true;

		float timeElapsed = 0;
		float targetHeight = isCrouching ? standingHeight : crouchHeight;
		float currentHeight = characterController.height;
		Vector3 targetCenter = isCrouching ? standingCenter : crouchingcenter;
		Vector3 currentCenter = characterController.center;

		while (timeElapsed < timeToCrouch)
		{
			characterController.height = Mathf.Lerp(currentHeight,targetHeight,timeElapsed/timeToCrouch);
			characterController.center = Vector3.Lerp(currentCenter, targetCenter, timeElapsed/timeToCrouch);
			timeElapsed += Time.deltaTime;
			yield return null;
		}

		characterController.height = targetHeight;
		characterController.center = targetCenter;

		isCrouching = !isCrouching;

		duringCrouchingAnimation = false;
		
	}

	private IEnumerator toggleZoom(bool isEnter){
		float targetFOV = isEnter ? zoomFOV : defaultFOV;
		float startingFOV = playerCamera.fieldOfView;
		float timeElapsed = 0;

		while (timeElapsed < timeToZoom){
			playerCamera.fieldOfView = Mathf.Lerp(startingFOV, targetFOV, timeElapsed / timeToZoom);
			timeElapsed += Time.deltaTime;
			yield return null; 

		}
		playerCamera.fieldOfView = targetFOV;
		zoomRoutine = null;
	}
	private IEnumerator regenHeath(){
		yield return new WaitForSeconds(timeBeforeRegenStarts);
		WaitForSeconds timeToWait = new WaitForSeconds(HeathTimeIncrement);

		while(currentHealth < maxHealth){

			currentHealth += healthValueIncrement;

			if (currentHealth > maxHealth)
					currentHealth = maxHealth;
			onHealDamage?.Invoke(currentHealth);
			yield return timeToWait;
		}

		regeneratingHealth = null;
	}

	private IEnumerator RegenerateStamina(){
		yield return new WaitForSeconds(timeBeforeStaminaRegenStarts);
		WaitForSeconds timeToWait = new WaitForSeconds(staminaTimeIncrement);
		while(currentStamina < maxStamina){
			
			if (currentStamina > 0)
					canSprint = true;
		
			currentStamina += staminaValueIncrement;

			if (currentStamina > maxStamina)
				currentStamina = maxStamina;

			onStaminaChange?.Invoke(currentStamina);

			yield return timeToWait;

			
		}
		regeneratingStamina = null;
	}
}
