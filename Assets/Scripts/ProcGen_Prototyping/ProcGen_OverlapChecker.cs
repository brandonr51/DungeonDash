using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProcGen_OverlapChecker : MonoBehaviour
{
    [HideInInspector]
    public bool isOverlapping = false;

    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "OverlapChecker")
        {
            Debug.Log("Tile Collision Detected at (" + transform.position + ")");
            isOverlapping = true;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        Debug.Log("Tile moved out of overlapping position");
        isOverlapping = false;
    }

    public void CheckForOverlap(GameObject whoSpawnedMe)
    {
        Debug.Log("CHECKING FOR OVERLAP");

        //Re-attempts tile selection if there was an overlap
        if (isOverlapping)
        {
            whoSpawnedMe.GetComponent<ProcGen_ConnectionPoint>().RetryGeneration();
        }
        else //Generation for this tile should be completed
        {
            whoSpawnedMe.GetComponent<ProcGen_ConnectionPoint>().CompleteGeneration();
        }
    }
}