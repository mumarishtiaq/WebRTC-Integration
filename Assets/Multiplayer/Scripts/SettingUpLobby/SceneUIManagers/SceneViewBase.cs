using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SceneViewBase : MonoBehaviour
{
    [SerializeField]
    List<Selectable> allSelectables;

    [SerializeField]
    GameObject playerDataHolder; 
    
    [SerializeField]
    TextMeshProUGUI playerNameText;

    [SerializeField]
    MessagePopup messagePopup;

    protected bool isInteractable { get; private set; }





    public virtual void SetPlayerData(string playerName)
    {
        playerDataHolder.SetActive(true);
        playerNameText.text = $"{playerName}";
    }
   

    public void SetInteractable(bool isInteractable)
    {
        foreach (var selectable in allSelectables)
        {
            selectable.interactable = isInteractable;
        }

        this.isInteractable = isInteractable;
    }

    public void ShowPopup(string title, string text)
    {
        messagePopup.Show(title, text);
    }
}
