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
    public float step;
    public CharacterController controller;
    Vector3 rotationVector;

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

        moveDirection.x = moveSpeed;

        //Uses a modified variable of moveDirection to also calculate what direction the player should be facing.
        rotationVector = new Vector3 (moveDirection.x,0,0);

        Quaternion rotation = Quaternion.Euler(0, 0, 0);
        if (rotationVector != Vector3.zero)
            rotation = Quaternion.LookRotation(rotationVector);

        TurnAround(rotation);

        Debug.DrawLine(transform.position, groundChecker.position);

        Vector3 movement = new Vector3(currentSpeed, moveDirection.y, 0);
        controller.Move(movement * Time.deltaTime);

    }

    public void DoMove()
    {
        currentSpeed = Mathf.Lerp(currentSpeed, moveSpeed, step);
    }

    public void TurnAround(Quaternion rotation)
    {
        gameObject.transform.rotation = Quaternion.RotateTowards(gameObject.transform.rotation, rotation, (turnSpeed * Time.deltaTime));
    }

    public void RoamPlatform()
    {
        if (Physics.Raycast(transform.position, -Vector3.up + (moveDirection), distToGround + 1f))
        {

        }
        else
        {
            Debug.Log("No ground");

            moveSpeed = -moveSpeed;
        }
        RaycastHit hit;
        if (Physics.Raycast(transform.position, rotationVector, out hit, distToGround, groundLayer))
        {
            Debug.Log("Wall ahead");
            moveSpeed = -moveSpeed;
        }
    }
}
