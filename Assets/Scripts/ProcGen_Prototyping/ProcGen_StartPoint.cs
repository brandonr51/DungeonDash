using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ProcGen_StartPoint : MonoBehaviour
{
    [Header("Map Generation Settings")]
    [Tooltip("The number of times the Generation Event will run -- ie. The bigger the number, the bigger the map.")]
    public int generationLoops;
    [Tooltip("The % Chance of a special tile being selected to spawn instead of a standard one.")]
    [Range(0, 100)]
    public float specialTileChance;
    public GameObject networkManager;


    public List<GameObject> startingTiles;

    public List<GameObject> standardTiles;

    //public List<GameObject> specialTiles;

    //[HideInInspector]
    public List<GameObject> connectionPoints;

    //[HideInInspector]
    public List<GameObject> activeTiles;

    //[HideInInspector]
    public ProcGen_StartPoint self;
    public void Awake()
    {
        self = gameObject.GetComponent<ProcGen_StartPoint>();
        GenerateStartingTile();
    }

    public void GenerateStartingTile()
    {
        int spawnIndex = Random.Range(0, startingTiles.Count);
        GameObject startingTile = Instantiate(startingTiles[spawnIndex].gameObject);
        activeTiles.Add(startingTile);
        
        for (int i = 0; i < startingTile.GetComponent<ProcGen_Tile>().connectionPoints.Count; i++)
        {
            connectionPoints.Add(startingTile.GetComponent<ProcGen_Tile>().connectionPoints[i]);
        }
        TileGenerationLoop();
    }

    public void TileGenerationLoop()
    {
        Debug.Log("Number of CPs to generate tiles for is: " + connectionPoints.Count);
        for (int i = 0; i < connectionPoints.Count; i++)
        {
            Debug.Log("Generating tile for Connection Point " + connectionPoints[i]);
            connectionPoints[i].GetComponent<ProcGen_ConnectionPoint>().StartGeneration(self);
        }

        Invoke("CompleteGenerationLoop", 0.1f);
    }

    public void CompleteGenerationLoop()
    {
        generationLoops--;

        if (generationLoops > 0)
        {
            Invoke("GenerateCPList", 0.03f);

            Debug.Log("Continuing tile generation loop. Loops remaining is: " + generationLoops);
            Invoke("TileGenerationLoop", 0.06f);
        }
        else if (generationLoops <= 0)
        {
            Debug.Log("Generation loops have been exhausted, finalizing map now...");

            //Before the generation is totally finished, regenerates the CP list one more time, then activates blockers on all of them
            //This should prevent any open spaces in the map where players can fall out
            GenerateCPList();

            for (int i = 0; i < connectionPoints.Count; i++)
            {
                connectionPoints[i].GetComponent<ProcGen_ConnectionPoint>().blocker.SetActive(true);
                Destroy(connectionPoints[i]);
            }

            //If you want things to spawn on the map after the layout has been generated, start new code from here
            if (networkManager.GetComponent<MyNetworkManager>().isHost)
            {
                networkManager.GetComponent<MyNetworkManager>().SpawnTiles();
            }
            return;
        }
    }

    public void GenerateCPList()
    {
        Debug.Log("Attempting to generate new list of Connection Points");
        connectionPoints.Clear(); //Both these lines should do the same thing, but I was experiencing some weird issue where the list wouldn't actually empty itself w/out them
        connectionPoints = new List<GameObject>(0);

        for (int i = 0; i < GameObject.FindGameObjectsWithTag("Connector").Length; i++)
        {
            Debug.Log("Adding new connector to list " + GameObject.FindGameObjectsWithTag("Connector")[i]);
            connectionPoints.Add(GameObject.FindGameObjectsWithTag("Connector")[i]);
        }
    }
}
