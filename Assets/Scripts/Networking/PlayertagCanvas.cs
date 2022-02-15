using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayertagCanvas : MonoBehaviour
{
    [SerializeField] private Transform transformPosition;
    void Update()
    {
        gameObject.transform.position = transformPosition.transform.position;
    }
}
