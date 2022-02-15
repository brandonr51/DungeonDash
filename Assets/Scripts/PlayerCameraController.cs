using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using Mirror;

public class PlayerCameraController : NetworkBehaviour
{
    [Header("Camera")]
    [SerializeField] private Vector2 maxFollowOffset = new Vector2(-1f, 6f);
    [SerializeField] private Vector2 cameraVelocity = new Vector2(4f, 0.25f);
    [SerializeField] private Transform playerTransform = null;
    [SerializeField] private CinemachineVirtualCamera virtualCamera = null;

    private ControlScript controls;
    private CinemachineTransposer transposer;

    public override void OnStartAuthority()
    {
        transposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();

        virtualCamera.gameObject.SetActive(true);

        enabled = true;

        controls = new ControlScript();
        controls.Player.Look.performed += ctx => Look(ctx.ReadValue<Vector2>());
    }

    [ClientCallback]
    private void OnEnable() => controls.Enable();
    [ClientCallback]
    private void OnDisable() => controls.Disable();

    private void Look(Vector2 lookAxis)
    {
        float deltaTime = Time.deltaTime;

        transposer.m_FollowOffset.y = Mathf.Clamp(transposer.m_FollowOffset.y - (lookAxis.y * cameraVelocity.y * deltaTime), 
            maxFollowOffset.x, maxFollowOffset.y);

        playerTransform.Rotate(0f, lookAxis.x * cameraVelocity.x * deltaTime, 0f);
    }
}
