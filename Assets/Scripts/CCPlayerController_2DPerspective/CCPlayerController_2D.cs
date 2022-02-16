using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;
using UnityEngine.Animations;
using Mirror;
using Cinemachine;

public class CCPlayerController_2D : NetworkBehaviour
{
    private PlayerInput playerInput;

    private string playerDevice;

    public enum MovementStatus
    {
        Idle,
        Moving,
        Falling,
        Stopping,
        Stunned
    };

    //[HideInInspector]
    public MovementStatus currentMoveStatus;

    [Header("Movement Values")]
    [Tooltip("The maximum speed of the player")]
    public float maxMoveSpeed;
    [Tooltip("The force of the player's jump")]
    public float jumpForce;
    [HideInInspector]
    public float currentJumpVector;
    private bool isJumping = false;
    [Tooltip("The inertia level on the player's movement. Values closer to 0.1 mean they'll come to a stop faster.")]
    [Range(0.01f, 0.1f)]
    public float inertiaSpeed = 0.01f;
    [Tooltip("The speed threshold the player needs to be at in order for the sliding-stop animation to play.")]
    [Range(0.01f, 1)]
    public float stoppingSpeedThreshold = 0.01f;
    private bool isStopping;

    [Header("Camera Control Variables")]
    public float cameraRotateSpeed;

    [Header("Player Turn Speed")]
    public float turnSpeed;

    [Header("Player's Animator")]
    [Tooltip("Found on the Player's mesh object")]
    public Animator animator;

    //Variable for the Character Controller component on the Player
    private CharacterController controller;

    //Variable for defining the Current InputVector, and using it to create a Movement Direction
    private Vector3 moveDirection = Vector3.zero;
    private float rawInputVector = 0;
    private float inputVector = 0;

    //CAMERA SETUP
    public GameObject cameraToSpawn;
    private GameObject myCamera;

    //GRAVITY VARIABLES
    [Header("Gravity Variables")]
    [Tooltip("The default strength of gravity, be mindful that the value of Gravity Ramp Up is added to this each frame that the player is falling.")]
    public float defaultGravityMultiplier;
    [Tooltip("The maximum strength of gravity. For example: A value of 2 would mean that that the strength can go up to double the Unity default.")]
    public float maxGravityMultiplier = 2;
    [Tooltip("The value that gravity increases by each frame that a player is falling. Values above 0.02 can end up looking really goofy, beware.")]
    public float gravityRampUp;
    [HideInInspector]
    public float gravityMultiplier = 0;
    private Vector3 currentGravity;

    //ATTACKING VARIABLES
    [Header("Attacking Variables")]
    //[Tooltip("The amount of damage the Player's attack does")]
    //public int attackDamage; CURRENTLY NOT IN USE
    [Tooltip("Force of knockback applied to enemies when hit with an attack")]
    public float knockbackStrength;
    [Tooltip("How long the attacked enemy is stunned for")]
    public float stunLength = 2f;

    //ATTACK TARGETS
    [HideInInspector]
    public List<GameObject> playerTargets;
    [HideInInspector]
    public List<GameObject> npcTargets;


    //This function is only ran if the player has authority over the prefab.
    public override void OnStartAuthority()
    {
        myCamera = Instantiate(cameraToSpawn);
        myCamera.GetComponent<CameraFollow_2D>().whoSpawnedMe = gameObject;

        //These components should all be disabled in the prefab. These are all components we want a player to control only on their prefab (controls, camera etc.)
        GetComponent<CCPlayerController_2D>().enabled = true;
        myCamera.GetComponentInChildren<Camera>().enabled = true;
        //GetComponentInChildren<CinemachineBrain>().enabled = true;
        //GetComponentInChildren<CinemachineVirtualCamera>().enabled = true;
        GetComponent<PlayerInput>().enabled = true;
    }

    public void Awake()
    {
        /* Moving this into the OnStartAuthority() method b/c I'm not sure if it'll run properly in Awake()
        myCamera = Instantiate(cameraToSpawn);
        myCamera.GetComponent<CameraFollow_2D>().whoSpawnedMe = gameObject;
        */

        transform.position = new Vector3(transform.position.x, 2, 2);

        //Finds the PlayerInput component on the Player, without this everything explodes.
        playerInput = GetComponent<PlayerInput>();

        //Gets what device the player is using.
        playerDevice = playerInput.currentControlScheme;

        //Find the CharacterController on the Player
        controller = GetComponent<CharacterController>();
    }

    public void SetInputVector(CallbackContext context)
    {
        rawInputVector = context.ReadValue<float>();
    }

    public void OnJumpAction(CallbackContext context)
    {
        if (context.started && controller.isGrounded && currentMoveStatus != MovementStatus.Stunned)
        {
            gravityMultiplier = defaultGravityMultiplier;
            isJumping = true;
        }

        if (context.canceled && !controller.isGrounded)
        {
            EndJumpEarly(6);
        }

        /* Possibility of having player hold down jump button for higher jump
        if (context.performed && isJumping)
        {
            gravityMultiplier = defaultGravityMultiplier;
        }
        */
    }

    public void OnAttackAction(CallbackContext context)
    {
        if (context.started && currentMoveStatus != MovementStatus.Stunned)
        {
            animator.SetTrigger("Attacking");

            //This loop handles "attacking" each of the Players in range
            for (int i = 0; i < playerTargets.Count; i++)
            {
                playerTargets[i].GetComponent<CCPlayerController_2D>().OnStun(stunLength);
            }

            /* UN-COMMENT THIS LOOP LATER WHEN YOU DO NPC STUFF
            //This loop handles "attacking" each of the NPCs in range
            for (int i = 0; i < npcTargets.Count; i++)
            {
                npcTargets[i].GetComponent<NPCController>().OnAttacked();
            }
            */
        }
    }

    public void OnStun(float stunDuration)
    {
        if (currentMoveStatus != MovementStatus.Stunned)
        {
            currentMoveStatus = MovementStatus.Stunned;
            animator.SetBool("isStunned", true);
            Invoke("StunEnds", stunDuration);
        }
    }

    public void StunEnds()
    {
        currentMoveStatus = MovementStatus.Idle;
        animator.SetBool("isStunned", false);
    }

    public void Update()
    {
        inputVector = Mathf.Lerp(inputVector, rawInputVector, inertiaSpeed);

        float inputX = inputVector;
        if (inputX < 0)
            inputX *= -1;

        //And Thirdly, uses the higher of the two input values to determine the blend tree's threshold
        if (currentMoveStatus != MovementStatus.Stopping && currentMoveStatus != MovementStatus.Falling && currentMoveStatus != MovementStatus.Stunned)
        {
            currentMoveStatus = MovementStatus.Moving;
            animator.SetFloat("InputVector", (float)inputX);
        }

        //if (/* Put something here later for walljumping and the like*/)
        //{
        //    currentGravity = (Physics.gravity * gravityMultiplier) * Time.deltaTime;
        //}

        if (currentMoveStatus == MovementStatus.Falling || currentMoveStatus == MovementStatus.Stunned)
        {
            //A statement to make sure that gravity can't go beyond a maximum value set in the inspector. This is to prevent gravity from reaching an infinite value.
            if (gravityMultiplier < maxGravityMultiplier)
            {
                gravityMultiplier += gravityRampUp;
            }
        }
        else
        {
            gravityMultiplier = defaultGravityMultiplier;
        }

        // *** STATE CHECKS *** //

        if (controller.isGrounded && inputX > 0 && currentMoveStatus != MovementStatus.Stunned)
        {
            currentMoveStatus = MovementStatus.Moving;
        }

        else if (!controller.isGrounded && /*moveDirection.y < 0 && */ currentMoveStatus != MovementStatus.Stunned)
        {
            currentMoveStatus = MovementStatus.Falling;
        }

        else if (currentMoveStatus != MovementStatus.Stunned)
        {
            currentMoveStatus = MovementStatus.Idle;
        }

        // *** END STATE CHECKS *** //

        //Has the script calculate the direction the player needs to move in
        CalculateMovement();

        //Slows down animation speeds if highestInput is below the threshhold
        if ((inputX <= 0.5f && inputX >= 0.05f) && currentMoveStatus != MovementStatus.Idle && currentMoveStatus != MovementStatus.Stunned)
            animator.speed = inputX * 2;
        else if (inputX < 0.05)
            animator.speed = 1;
    }


    public void TurnPlayer(Quaternion rotation)
    {
        if (currentMoveStatus != MovementStatus.Stopping)
            //Rotates the Player to face the value of the rotation variable.
            gameObject.transform.rotation = Quaternion.RotateTowards(gameObject.transform.rotation, rotation, (turnSpeed * Time.deltaTime));
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && other.gameObject != gameObject)
        {
            Debug.Log("Player Detected");
            playerTargets.Add(other.gameObject);
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            playerTargets.Remove(other.gameObject);
        }
    }

    public void CalculateMovement()
    {
        if (((rawInputVector <= 0 && inputVector > stoppingSpeedThreshold) || (-rawInputVector <= 0 && -inputVector > stoppingSpeedThreshold)) && controller.isGrounded)
        {
            isStopping = true;
            animator.SetBool("isStopping", true);
        }
        else
        {
            isStopping = false;
            animator.SetBool("isStopping", false);
        }
            

        //Sets the player's horizontal movement direction using the input vector from the Player's Input
        moveDirection.x = (inputVector * maxMoveSpeed);

        //Uses a modified variable of moveDirection to also calculate what direction the player should be facing.
        var rotationVector = new Vector3(moveDirection.x, 0, 0);

        //Gets the final value for use with the TurnPlayer() function.
        rotationVector *= maxMoveSpeed;

        //Uses one final value, labelled 'rotation' which is reset each time Update() is called to prevent the player from spinning out like a Beyblade.
        Quaternion rotation = Quaternion.Euler(0, 0, 0);
        if (rotationVector != Vector3.zero)
            rotation = Quaternion.LookRotation(rotationVector);

        //TurnPlayer is only called when there is still new input coming from the player, thus keeping the player from turning back to 0 bearing.
        if (rawInputVector != 0)
            TurnPlayer(rotation);

        if (controller.isGrounded)
        {
            //tell the animator that the player is grounded
            animator.SetBool("isGrounded", true);

            if (isJumping)
            {
                moveDirection.y = jumpForce;
                isJumping = false;
            }
        }
        else
        {
            //Tell the animator that the player is not grounded
            animator.SetBool("isGrounded", false);
            currentGravity = moveDirection - (Physics.gravity * gravityMultiplier);
            moveDirection.y -= currentGravity.y * Time.deltaTime;
        }

        //If the Player is stunned, prevents them from moving
        if (currentMoveStatus == MovementStatus.Stunned)
        {
            moveDirection = new Vector3(0, moveDirection.y, 0);
        }

        //Debug.Log(moveDirection);
        controller.Move(moveDirection * Time.deltaTime);

        //Give the animator the current value of the player's vertical velocity as a percentage
        animator.SetFloat("verticalVelocity", (moveDirection.y / (jumpForce/4)));
    }

    public void EndJumpEarly(float jumpEndForce)
    {
        if (moveDirection.y < jumpEndForce)
            return;
        else
            moveDirection.y /= 2;
    }

    public void OnControllerColliderHit(ControllerColliderHit hit)
    {
        //Debug.Log("Direction of Character Controller collision: " + hit.moveDirection);
        if (hit.moveDirection.y > 0.3)
        {
            EndJumpEarly(0);
        }
    }
}
