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
    
    [SerializeField]
    GameObject _joiningObj;

    private List<PlayerIconView> _playerIcons = new List<PlayerIconView>();

    public bool IsPanelOpened { get; set; }



    

    public void OpenLobbyPanel(string title, string gameName)
    {
        IsPanelOpened = true;
        _lobbyPanel.SetActive(IsPanelOpened);
        _title.text = title;
        _gameName.text = gameName;

        SetJoiningObj(false);
    }
    public void Close()
    {
        IsPanelOpened = false;
        _lobbyPanel.SetActive(IsPanelOpened);
        RemoveAllPlayers();
    }

    internal void SpawnPlayerIcons(string playerID)
    {
        if (IsPanelOpened) return;

        RemoveAllPlayers();
        foreach (Player player in LobbyManager.Instance.players) 
        {
            var playerIcon = GameObject.Instantiate(playerIconPrefab, playersContainer);

            var playerId = player.Id;
            var playerName = player.Id == LobbyManager.playerId  ? "You":player.Data[LobbyManager.k_PlayerNameKey].Value;

            playerIcon.Initialize(playerId, playerName);

            _playerIcons.Add(playerIcon);
        }

        SequencePlayers(playerID);
    }

    internal void SetReadyState(string playerID, bool isReady)
    {
        var playerIcon = GetPlayerIcon(playerID);
        if (playerIcon)
            playerIcon.SetReady(isReady);
    }

    public void SetReadyStates()
    {
        foreach (Player player in LobbyManager.Instance.players)
        {
            var isReady = bool.Parse(player.Data[LobbyManager.k_IsReadyKey].Value);
            var playerIcon = GetPlayerIcon(player.Id);
            if (playerIcon)
                playerIcon.SetReady(isReady);
        }
    }

    private void SequencePlayers(string playerID)
    {
        var icon = GetPlayerIcon(playerID);
        if (icon == null) return;

        icon.transform.SetAsFirstSibling();
    }

    public void SetButtonsVisiblity(bool isInitiator)
    {
        _acceptBtn.gameObject.SetActive(!isInitiator);
        _rejectBtn.gameObject.SetActive(!isInitiator);
        _leaveBtn.gameObject.SetActive(isInitiator);
    }

    public void SetJoiningObj(bool state)
    {
        _joiningObj.SetActive(state);

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
