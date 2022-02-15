using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProcGen_ConnectionPoint : MonoBehaviour
{
    public int generationRetryAttempts;

    public float timeDelay = 0.03f;

    //[HideInInspector]
    public GameObject spawnedTile;
    //[HideInInspector]
    public ProcGen_ConnectionPoint otherConnectionPoint;
    //[HideInInspector]
    public ProcGen_OverlapChecker overlapChecker;

    private ProcGen_StartPoint startingTile;

    public enum ConnectionType
    {
        Horizontal,
        Vertical
    };

    public enum ConnectionSide
    {
        Left,
        Right,
        Top,
        Bottom
    };

    public ConnectionType connectionType;
    public ConnectionSide connectionSide;

    public GameObject blocker;

    public void Awake()
    {
        startingTile = GameObject.FindGameObjectWithTag("StartingPoint").GetComponent<ProcGen_StartPoint>();
    }

    public void StartGeneration(ProcGen_StartPoint startPoint)
    {
        //Attempts to generate a new tile for this connection point as long as there are generation retries remaining on the script
        if (generationRetryAttempts > 0)
        {
            //Choose a random tile to spawn from the list of Standard Tiles
            int listSelectionNumber = Random.Range(0, startPoint.standardTiles.Count);
            GameObject tileToSpawn = startPoint.standardTiles[listSelectionNumber];
            //Debug.Log("Selected tile " + tileToSpawn + " will be spawned at " + transform.position);

            //Spawns the selected tile in
            //Debug.Log("Generating " + tileToSpawn + " at " + transform.position);
            spawnedTile = Instantiate(tileToSpawn);
            startingTile.activeTiles.Add(spawnedTile);
            overlapChecker = spawnedTile.GetComponent<ProcGen_Tile>().antiOverlapTrigger.GetComponent<ProcGen_OverlapChecker>();

            int connectorsToCheck = spawnedTile.GetComponent<ProcGen_Tile>().connectionPoints.Count - 1;

            //Checks each connector on the tile for the first matching connection point
            for (int i = 0; i < spawnedTile.GetComponent<ProcGen_Tile>().connectionPoints.Count; i++)
            {
                //Views the potential connection point within a variable.
                otherConnectionPoint = spawnedTile.GetComponent<ProcGen_Tile>().connectionPoints[i].GetComponent<ProcGen_ConnectionPoint>();

                //Checks if the Connector matches the Type (Horizontal or Vertical) and is opposite to the connection side (Left/Right or Top/Bottom)
                if (otherConnectionPoint.connectionSide != connectionSide && otherConnectionPoint.connectionType == connectionType)
                {
                    Debug.Log("Compatible connector found: " + otherConnectionPoint.gameObject + " will be spawned for " + gameObject);

                    //Moves the spawaned tile to the position of this connector
                    spawnedTile.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, 0);

                    //Offsets the tile based on the tile's compatible connector -- This should mean that the two connection points share the same transform position.
                    Vector3 connectionOffset = new Vector3(otherConnectionPoint.gameObject.transform.localPosition.x, otherConnectionPoint.gameObject.transform.localPosition.y, 0);
                    spawnedTile.transform.position -= connectionOffset;

                    //Breaks the for loop, tells the spawned tile to check if it is colliding with an already established tile
                    Invoke("BufferMethod", timeDelay);
                    break;
                }
                else if (connectorsToCheck <= 0)
                {
                    Debug.Log("Spawned tile " + spawnedTile + " is incompatible with " + gameObject + " at (" + transform.position + ")");
                    RetryGeneration();
                    break;
                }
                else
                {
                    Debug.Log("Connector " + otherConnectionPoint + " is not a valid connection, checking the next one");
                    connectorsToCheck--;
                }   
            }
        }
        else if (generationRetryAttempts <= 0) //The script has run out of retry attempts, and will instead just activate the attached blocker
        {
            Debug.Log("Retry attempts for " + gameObject + "at (" + transform.position + ") have been exhausted, activating blocker instead");
            blocker.SetActive(true);
            Destroy(gameObject);
            //return; //If you want to implement detection of other connectors, start here
        }
    }

    public void BufferMethod()
    {
        overlapChecker.CheckForOverlap(gameObject);
    }

    public void CompleteGeneration()
    {
        Debug.Log("Tile generation for " + gameObject + "at (" + transform.position + ") has been completed and was successful.");
        Destroy(otherConnectionPoint.gameObject);
        Destroy(gameObject);
        return;
    }

    public void RetryGeneration()
    {
        Debug.Log("Retrying Generation for " + gameObject);
        startingTile.activeTiles.Remove(spawnedTile);
        Destroy(spawnedTile);
        spawnedTile = null;
        generationRetryAttempts--;
        StartGeneration(startingTile);
    }
}
