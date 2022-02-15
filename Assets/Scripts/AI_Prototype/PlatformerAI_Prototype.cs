using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class PlatformerAI_Prototype : MonoBehaviour
{
    [Header("Components")]
    public NavMeshAgent agent = null;

    [Header("Movement")]
    public float moveSpeed;
    public float currentSpeed;
    public float movementSmoothStep;
    public Vector3 moveDirection;

    [Header("Rotation")]
    public float turnSpeed;

    [Header("Jumping")]
    public float jumpHeight;
    public bool isGrounded = true;
    public bool hasJumped = false;

    [Header("Gravity")]
    public float gravity;

    [Header("Navigating")]
    public LayerMask obstacleLayers;
    public float groundDistance;
    public Transform ledgeDetection;
    public float roamRange;
    public float roamFrequency;
    public float roamTimer;

    [Header("PlayerTargeting")]
    public PlayerManager playerManager = null;
    public List<Transform> playersInRange = new List<Transform>();
    public float chaseRange;
    public Transform lowestDistancePlayer;


    public void Start()
    {
        playerManager = GameObject.Find("PlayerManager").GetComponent<PlayerManager>();
        agent = GetComponent<NavMeshAgent>();
    }

    public void Update()
    {
        if (Keyboard.current[Key.Space].wasPressedThisFrame) { DoJump(); }
        // *** AI MOVEMENT *** //

        if (lowestDistancePlayer == null)
        {
            //Find a random point to roam to and search for players
            FindPlayer();
            roamTimer += Time.deltaTime;
            if (roamTimer > roamFrequency)
            {
                Roam();
                roamTimer = 0;
            }
        } else
        {
            //Check how far current target is and if it's out of range go back to roaming
            if (Vector3.Distance(transform.position, lowestDistancePlayer.transform.position) > chaseRange)
            {
                lowestDistancePlayer = null;
            } else 
            {
                DoMovement();
            }
        }

        DoMovement();

        if (isGrounded)
        {
            //
        } else if (!isGrounded)
        {
            //
        }



    }

    public void DoMovement()
    {
        //Check for ground below
        RaycastHit groundDetection;
        if (Physics.Raycast(ledgeDetection.transform.position, transform.TransformDirection(Vector3.down), out groundDetection, groundDistance, obstacleLayers))
        {
            Debug.Log("Grounded");
            isGrounded = true;
            hasJumped = false;
        } else
        {
            isGrounded = false;
        }

        agent.destination = lowestDistancePlayer.position;

    }

    public void ScanInfront()
    {
        //Check if there is ground ahead of the AI
        RaycastHit hit;
        if (Physics.Raycast(ledgeDetection.transform.position, transform.TransformDirection(Vector3.down), out hit, groundDistance, obstacleLayers))
        {
            Debug.Log("Empty space detected.");
        } 
    }
    public void DoJump()
    {
        if (isGrounded && !hasJumped)
        {
            hasJumped = true;
        }

    }

    public void Roam()
    {
        //Find a random horizontal point to go to
        Vector3 roamPoint = new Vector3(Random.Range(transform.position.x - roamRange, transform.position.x + roamRange), transform.position.y, transform.position.z);

        if (transform.position.x - roamPoint.x > 0)
        {
            moveSpeed = -moveSpeed;
        }

    }

    public void ScaleWall()
    {
        RaycastHit wallDetect;
        Debug.DrawLine(transform.position, transform.position + transform.forward);
        if (Physics.Raycast(transform.position, transform.position + transform.forward, out wallDetect, 1f, obstacleLayers))
        {
            Debug.Log("Wall detected");
            moveSpeed = -moveSpeed;
        }
        else
        {
            DoJump();
        }
    }

    public void FindPlayer()
    {
        if (lowestDistancePlayer == null && playerManager.players.Count > 0)
        foreach (Transform player in playerManager.players)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance < chaseRange)
            {
                lowestDistancePlayer = player;
            }

        }
    }

    public void ChasePlayer()
    {
    }

}