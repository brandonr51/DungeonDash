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

    public Transform cameraPivot;

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
    private Vector2 inputVector = Vector2.zero;
    private Vector2 cameraInputVector = Vector2.zero;

    //Variable that finds the Camera
    private Camera levelCamera;

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
    [HideInInspector]
    public bool useGravity = true;

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
        //These components should all be disabled in the prefab. These are all components we want a player to control only on their prefab (controls, camera etc.)
        GetComponent<CCPlayerController_2D>().enabled = true;
        GetComponentInChildren<Camera>().enabled = true;
        GetComponentInChildren<CinemachineBrain>().enabled = true;
        GetComponentInChildren<CinemachineVirtualCamera>().enabled = true;
        GetComponent<PlayerInput>().enabled = true;
    }

    public void Awake()
    {
        gameObject.transform.position = GameObject.FindGameObjectWithTag("SpawnPoint").transform.position;


        //Finds the PlayerInput component on the Player, without this everything explodes.
        playerInput = GetComponent<PlayerInput>();

        //Gets what device the player is using.
        playerDevice = playerInput.currentControlScheme;

        //Finds the main camera for the level (used for movement context)
        //levelCamera = Camera.main;
        //cameraPivot = levelCamera.transform.parent;

        //Find the CharacterController on the Player
        controller = GetComponent<CharacterController>();
    }

    public void SetInputVector(CallbackContext context)
    {
        inputVector = context.ReadValue<Vector2>();
        //Debug.Log("Current Movement InputVector" + inputVector);
    }

    public void SetCameraInputVector(CallbackContext context)
    {
        cameraInputVector = context.ReadValue<Vector2>();
        Debug.Log("Current Camera InputVector" + cameraInputVector);
    }

    public void OnJumpAction(CallbackContext context)
    {
        if (context.started && controller.isGrounded && currentMoveStatus != MovementStatus.Stunned)
        {
            currentJumpVector = jumpForce;
            gravityMultiplier = defaultGravityMultiplier;
        }
    }

    public void OnAttackAction(CallbackContext context)
    {
        if (context.started && currentMoveStatus != MovementStatus.Stunned)
        {
            Debug.Log("he attac");
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
            Debug.Log("Stone cold stunner");
            currentMoveStatus = MovementStatus.Stunned;
            animator.SetBool("isStunned", true);
            Invoke("StunEnds", stunDuration);
        }
    }

    public void StunEnds()
    {
        Debug.Log("Wake up jeff");
        currentMoveStatus = MovementStatus.Idle;
        animator.SetBool("isStunned", false);
    }

    public void Update()
    {
        // *** PLAYER MOVEMENT AND ROTATION *** //

        //Sets the player's horizontal movement direction using the input vector from the Player's Input
        moveDirection = new Vector3(inputVector.x, 0, 0);

        //Uses a modified variable of moveDirection to also calculate what direction the player should be facing.
        var rotationVector = moveDirection;
        //Adjusts the moveDirectoin based on the angle of the Main Camera.
        //moveDirection = Quaternion.Euler(0, levelCamera.gameObject.transform.eulerAngles.y, 0) * moveDirection;

        //Adjusts the rotationVector based on the angle of the level camera.
        rotationVector = Quaternion.Euler(0, 0, 0) * rotationVector;
        
        //Gets the final value for use with the TurnPlayer() function.
        //rotationVector *= maxMoveSpeed;

        //Uses one final value, labelled 'rotation' which is reset each time Update() is called to prevent the player from spinning out like a Beyblade.
        Quaternion rotation = Quaternion.Euler(0, 0, 0);
        if (rotationVector != Vector3.zero)
            rotation = Quaternion.LookRotation(rotationVector);
        
        //TurnPlayer is only called when there is still new input coming from the player, thus keeping the player from turning back to 0 bearing.
        if (inputVector != new Vector2(0, 0))
            TurnPlayer(rotation);


        //Updates the state of the Player based on the strength of the InputVector
        //Firstly, determines if either inputVector.x or .y are negative, and turns them into positives.
        var inputX = inputVector.x;
        if (inputX < 0)
            inputX *= -1;

        var inputY = inputVector.y;
        if (inputY < 0)
            inputY *= -1;

        //Secondly, determines whether input X or Y is higher, if they are tied, defaults to X
        var highestInput = 0f;
        if (inputX >= inputY)
            highestInput = inputX;
        else
            highestInput = inputY;

        //And Thirdly, uses the higher of the two input values to determine the blend tree's threshold
        if (currentMoveStatus != MovementStatus.Stopping && currentMoveStatus != MovementStatus.Falling && currentMoveStatus != MovementStatus.Stunned)
        {
            currentMoveStatus = MovementStatus.Moving;
            animator.SetInteger("MovementState", (int)currentMoveStatus);
            animator.SetFloat("InputVector", (float)highestInput);
        }

        //Adds the max move speed value on the player prefab
        moveDirection *= maxMoveSpeed;

        if (useGravity)
        {
            moveDirection += (Physics.gravity * gravityMultiplier) * Time.deltaTime;
        }

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
        if (controller.isGrounded && highestInput > 0 && currentMoveStatus != MovementStatus.Stunned)
        {
            currentMoveStatus = MovementStatus.Moving;
        }

        else if (!controller.isGrounded && moveDirection.y < 0 && currentMoveStatus != MovementStatus.Stunned)
        {
            currentMoveStatus = MovementStatus.Falling;
        }

        else if (currentMoveStatus != MovementStatus.Stunned)
        {
            currentMoveStatus = MovementStatus.Idle;
        }

        //If the Player is stunned, prevents them from moving
        if (currentMoveStatus == MovementStatus.Stunned)
        {
            moveDirection = new Vector3(0, moveDirection.y, 0);
        }

        // *** END STATE CHECKS *** //

        // *** JUMP VECTOR UPDATE *** //

        if (currentJumpVector > 0)
        {
            moveDirection.y += Mathf.Sqrt(currentJumpVector * -3.0f * Physics.gravity.y);
        }

        if (moveDirection.y < 0)
            currentJumpVector = 0;

        // *** END JUMP VECTOR *** //

        //Calls the Move() function on the Character Controller
        controller.Move(moveDirection * Time.deltaTime);

        // *** CAMERA ROTATION *** //

        //cameraPivot.Rotate(new Vector3(0, cameraInputVector.x * cameraRotateSpeed, 0));

        // *** CAMERA ROTATION ENDS *** //

        // *** ANIMATOR STATE SETTINGS *** //

        animator.SetInteger("MovementState", (int)currentMoveStatus);

        //Slows down animation speeds if highestInput is below the threshhold
        if (highestInput <= 0.5f && currentMoveStatus != MovementStatus.Idle && currentMoveStatus != MovementStatus.Stunned)
            animator.speed = highestInput * 2;
        else
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
}
