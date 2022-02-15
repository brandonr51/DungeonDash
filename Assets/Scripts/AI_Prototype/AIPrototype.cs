using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPrototype : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 0;
    [SerializeField] private float gravity;
    public float currentSpeed = 0f;
    public float velocity;
    [SerializeField] private Vector3 moveDirection;
    [SerializeField] LayerMask groundLayer;
    public Transform groundChecker;
    public float distToGround;
    public float maxMoveSpeed;
    public bool changedVelocity = false;
    public float turnSpeed = 1;
    public SphereCollider chaseRangeCollider;
    public float closestPlayer = Mathf.Infinity;
    Transform tMin = null;
    public float step;
    public CharacterController controller;

    private void Start()
    {
        Collider collider = GetComponent<Collider>();
        moveDirection = Vector3.right;
        distToGround = collider.bounds.extents.y;
        moveDirection.y = gravity;
    }

    private void Update()
    {
        DoMove();
        RoamPlatform();

        //Uses a modified variable of moveDirection to also calculate what direction the player should be facing.
        var rotationVector = new Vector3 (moveDirection.x,0,0);

        Quaternion rotation = Quaternion.Euler(0, 0, 0);
        if (rotationVector != Vector3.zero)
            rotation = Quaternion.LookRotation(rotationVector);

        TurnAround(rotation);

        Debug.DrawLine(transform.position, groundChecker.position);


        controller.Move(moveDirection * Time.deltaTime);
    }

    public void DoMove()
    {
        currentSpeed = Mathf.Lerp(currentSpeed, moveSpeed, step);
        moveDirection.x = currentSpeed;
    }

    public void SlowDown()
    {
        Debug.Log("SlowingDown");
        currentSpeed = Mathf.Lerp(currentSpeed, 0, step);
    }

    public void TurnAround(Quaternion rotation)
    {
        gameObject.transform.rotation = Quaternion.RotateTowards(gameObject.transform.rotation, rotation, (turnSpeed * Time.deltaTime));
    }

    public void RoamPlatform()
    {
        if (Physics.Raycast(transform.position, -Vector3.up + (moveDirection), distToGround + 1f))
        {

            {
                Debug.Log("Doing move");
            }

        }
        else
        {

                Debug.Log("Changing velocity");
                moveSpeed = -moveSpeed;

        }
    }
}
