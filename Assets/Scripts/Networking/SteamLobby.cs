using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;

public class SteamLobby : MonoBehaviour
{
    [SerializeField] private GameObject buttons = null;

    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    protected Callback<LobbyEnter_t> lobbyEntered;

    private const string HostAddressKey = "HostAddress";

    private MyNetworkManager networkManager;

    public static CSteamID LobbyId { get; private set; }

    private void Start()
    {
        Debug.Log("Start SteamLobby script");
        networkManager = GetComponent<MyNetworkManager>();

        if(!SteamManager.Initialized) { return; }


    }

    private void OnEnable()
    {
        Debug.Log("OnEnable SteamLobby script");
        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
    }

    public void HostLobby()
    {
        Debug.Log("Doing HostLobby");
        buttons.SetActive(false);

        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, networkManager.maxConnections);

    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        Debug.Log("Doing OnLobbyCreated");
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            buttons.SetActive(true);
            return;
        }

        LobbyId = new CSteamID(callback.m_ulSteamIDLobby);

        networkManager.StartHost();

        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey, SteamUser.GetSteamID().ToString());

        networkManager.ServerChangeScene("SampleScene");
        networkManager.isHost = true;
    }

    private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        Debug.Log("Doing OnGameLobbyJoinRequested");
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        Debug.Log("Doing OnLobbyEntered");
        if (NetworkServer.active) { return; }

        string hostAdress = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey);

        networkManager.networkAddress = hostAdress;
        networkManager.StartClient();

        buttons.SetActive(false);

    }
}
