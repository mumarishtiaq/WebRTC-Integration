using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPanelView : MonoBehaviour
{
    [SerializeField] GameObject _lobbyPanel;
    [SerializeField] TextMeshProUGUI _title;
    [SerializeField] TextMeshProUGUI _gameName;

    [SerializeField]
    Transform playersContainer;

    [SerializeField]
    PlayerIconView playerIconPrefab;

    [SerializeField]
    Button _acceptBtn;

    [SerializeField]
    Button _rejectBtn;

    [SerializeField]
    Button _leaveBtn;

    private List<PlayerIconView> _playerIcons = new List<PlayerIconView>();




    public void OpenLobbyPanel(string title, string gameName)
    {
        _lobbyPanel.SetActive(true);
        _title.text = title;
        _gameName.text = gameName;
    }

    internal void SpawnPlayerIcons(string playerID)
    {
        RemoveAllPlayers();
        foreach (Player player in LobbyManager.Instance.players) 
        {
            var playerIcon = GameObject.Instantiate(playerIconPrefab, playersContainer);

            var playerId = player.Id;
            var playerName = playerID == player.Id ? "You":player.Data[LobbyManager.k_PlayerNameKey].Value;

            playerIcon.Initialize(playerId, playerName);

            _playerIcons.Add(playerIcon);
        }
    }

    internal void SetReadyState(string playerID, bool isReady)
    {
        var playerIcon = GetPlayerIcon(playerID);
        if (playerIcon)
            playerIcon.SetReady(isReady);
    }

    internal void SequencePlayers(string playerID, bool isInitiator)
    {
        var icon = GetPlayerIcon(playerID);
        if (icon == null) return;

        if (isInitiator)
            icon.transform.SetAsFirstSibling();
        else
            icon.transform.SetAsLastSibling();
    } 
    
    internal void SetButtonsState(bool isInitiator)
    {
        _acceptBtn.gameObject.SetActive(!isInitiator);
        _rejectBtn.gameObject.SetActive(!isInitiator);
        _leaveBtn.gameObject.SetActive(isInitiator);
    }
    public void SwitchReadyState(string playerID)
    {
        var icon = GetPlayerIcon(playerID);
        if (icon == null) return;

        icon.SwitchReadyState();
    }

    private PlayerIconView  GetPlayerIcon(string playerID)
    {
        return _playerIcons.FirstOrDefault(p => p.playerId == playerID);
    }

    void RemoveAllPlayers()
    {
        foreach (var playerIcon in _playerIcons)
        {
            Destroy(playerIcon.gameObject);
        }
        _playerIcons.Clear();
    }
}
