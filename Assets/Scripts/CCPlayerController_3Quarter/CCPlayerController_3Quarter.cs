using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;
using UnityEngine.Animations;
using Cinemachine;
using Mirror;

public class CCPlayerController_3Quarter : NetworkBehaviour
{
    private PlayerInput playerInput;

    private string playerDevice;

    public Transform cameraPivot;

    public enum MovementStatus
    {
        Idle,
        Moving,
        Falling,
        Stopping
    };

    //[HideInInspector]
    public MovementStatus currentMoveStatus;

    [Header("Max Movement Speed Values")]
    [Tooltip("The maximum speed of the player")]
    public float maxMoveSpeed;

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

    public void Awake()
    {
        //Finds the PlayerInput component on the Player, without this everything explodes.
        playerInput = GetComponent<PlayerInput>();

        //Gets what device the player is using.
        playerDevice = playerInput.currentControlScheme;

        //Find the CharacterController on the Player
        controller = GetComponent<CharacterController>();
        GetComponent<CCPlayerController_3Quarter>().enabled = true;
    }

    public override void OnStartAuthority()
    {
        GetComponent<CCPlayerController_3Quarter>().enabled = true;
        GetComponentInChildren<Camera>().enabled = true;
        GetComponentInChildren<CinemachineBrain>().enabled = true;
        GetComponentInChildren<CinemachineVirtualCamera>().enabled = true;
        GetComponent<PlayerInput>().enabled = true;
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

    public void Update()
    {
        // *** PLAYER MOVEMENT AND ROTATION *** //

        //Sets the player's horizontal movement direction using the input vector from the Player's Input
        moveDirection = new Vector3(inputVector.x, 0, 0);

        //Uses a modified variable of moveDirection to also calculate what direction the player should be facing.
        var rotationVector = moveDirection;

        //Gets the final value for use with the TurnPlayer() function.
        rotationVector *= maxMoveSpeed;

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
        if (currentMoveStatus != MovementStatus.Stopping && currentMoveStatus != MovementStatus.Falling)
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

        //Yes officer this line right here
        if (currentMoveStatus == MovementStatus.Falling)
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

        //State checks
        if (controller.isGrounded && highestInput > 0)
        {
            currentMoveStatus = MovementStatus.Moving;
        }

        else if (!controller.isGrounded)
        {
            currentMoveStatus = MovementStatus.Falling;
        }

        else
        {
            currentMoveStatus = MovementStatus.Idle;
        }

        //Calls the Move() function on the Character Controller
        controller.Move(moveDirection * Time.deltaTime);

        //Debug.Log(moveDirection);
        //Debug.Log(currentMoveStatus);

        // *** ANIMATOR STATE SETTINGS *** //

        animator.SetInteger("MovementState", (int)currentMoveStatus);

        //Slows down animation speeds if highestInput is below the threshhold
        if (highestInput <= 0.5f)
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

    public void Jump()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            moveDirection.y = 10f;
        }
    }
}
