using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;

public class MyNetworkManager : NetworkManager
{
    public GameObject procGenObject;
    GameObject procGenInstantiated;
    public bool isHost = false;

    public override void OnClientSceneChanged()
    {
        base.OnClientSceneChanged();
        if (isHost) { AddProcGenObject(); }

    }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        base.OnServerAddPlayer(conn);

        CSteamID steamId = SteamMatchmaking.GetLobbyMemberByIndex(SteamLobby.LobbyId, numPlayers - 1);

        var playerInfoDisplay = conn.identity.GetComponent<PlayerInfoDisplay>();

        playerInfoDisplay.SetSteamId(steamId.m_SteamID);

        //playerManager.players.Add(conn.identity.transform);

        
    }

    public void AddProcGenObject()
    {
        procGenInstantiated = Instantiate(procGenObject);
        procGenInstantiated.GetComponent<ProcGen_StartPoint>().networkManager = gameObject;
    }

    public void SpawnTiles()
    {
        foreach (GameObject tile in procGenInstantiated.GetComponent<ProcGen_StartPoint>().activeTiles)
        {
            Debug.Log("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            NetworkServer.Spawn(tile);
        }
    }

}
