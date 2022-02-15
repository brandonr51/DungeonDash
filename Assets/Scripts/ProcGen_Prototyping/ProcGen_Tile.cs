using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProcGen_Tile : MonoBehaviour
{
    public List<GameObject> connectionPoints;

    public GameObject antiOverlapTrigger;


    public void Awake()
    {
        if (antiOverlapTrigger == null)
        {
            Debug.LogError("The tile named '" + gameObject + "' is missing an anti-overlap trigger, please add one to the prefab before attempting to generate a map.");
        }
    }
}
