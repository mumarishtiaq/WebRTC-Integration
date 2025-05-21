using System;
using System.Drawing;
using TMPro;
using UnityEngine;

public class PlayerIconView : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI _playerNameText;

    [SerializeField]
    GameObject _checkMarkImage;
    
    [SerializeField]
    GameObject _readyStateObj;
    public string playerId { get; private set; }
    internal void Initialize(string playerId, string playerName)
    {
        this.playerId = playerId;
        _playerNameText.text = playerName;
    } 
    internal void InitializeNew(string playerId, string playerName, bool isReady)
    {
        this.playerId = playerId;
        _playerNameText.text = playerName;
        SetReady(isReady);
    }

    public void SetReady(bool isReady)
    {
        _checkMarkImage.SetActive(isReady);
        _readyStateObj.SetActive(isReady);

    }

    public void SwitchReadyState()
    {
        var isReady = !_checkMarkImage.activeSelf;
        SetReady(isReady);
    }
}
