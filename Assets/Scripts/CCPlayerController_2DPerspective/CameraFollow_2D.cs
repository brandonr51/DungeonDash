using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow_2D : MonoBehaviour
{
    [HideInInspector]
    public GameObject whoSpawnedMe;

    [Header("Lerp Speed")]
    [Tooltip("The speed at which the camera will attempt to follow the player that spawned it.")]
    [Range(0.01f, 0.5f)]
    public float lerpSpeed;

    public void Awake()
    {
        Invoke("AssignCameraToCanvas", 0.1f);
    }

    // Update is called once per frame
    void Update()
    {
        if (whoSpawnedMe != null)
            FollowPlayer();
    }

    public void FollowPlayer()
    {
        float posX = Mathf.Lerp(transform.position.x, whoSpawnedMe.transform.position.x, lerpSpeed);
        float posY= Mathf.Lerp(transform.position.y, whoSpawnedMe.transform.position.y, lerpSpeed);

        transform.position = new Vector3(posX, posY, transform.position.z);
    }

    public void AssignCameraToCanvas()
    {
        whoSpawnedMe.GetComponentInChildren<Canvas>().worldCamera = gameObject.GetComponentInChildren<Camera>();
    }
}
