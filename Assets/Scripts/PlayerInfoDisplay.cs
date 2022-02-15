using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using Mirror;
using TMPro;

public class PlayerInfoDisplay : NetworkBehaviour
{
    [SyncVar(hook = nameof(HandleSteamIdUpdated))]
    private ulong steamId;

    [SerializeField] private TextMeshProUGUI displayNameText = null;

    #region Server

    public void SetSteamId(ulong steamId)
    {
        this.steamId = steamId;
    }

    #endregion

    #region Client
    private void HandleSteamIdUpdated(ulong oldSteamId, ulong newSteamId)
    {
        var cSteamId = new CSteamID(newSteamId);

        displayNameText.text = SteamFriends.GetFriendPersonaName(cSteamId);
    }

    #endregion

}
