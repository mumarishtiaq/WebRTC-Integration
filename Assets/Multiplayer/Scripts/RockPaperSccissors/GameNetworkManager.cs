using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class GameNetworkManager : NetworkBehaviour
{
    public static GameNetworkManager Instance;

    private Dictionary<ulong, ChoiceType> playerChoices = new();

    private float countdownTime = 5f;
    private float timer;
    private bool roundInProgress = false;

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            playerChoices.Clear();
            StartNewRound();
        }

    }

    private void Update()
    {
        if (!IsServer || !roundInProgress) return;

        timer -= Time.deltaTime;

        UpdateTimerClientRpc(timer);

        if (timer <= 0f)
        {
            CompleteRoundWithRandomChoices();
        }
    }

    private void StartNewRound()
    {
        playerChoices.Clear();
        foreach (var client in NetworkManager.ConnectedClientsList)
        {
            playerChoices[client.ClientId] = ChoiceType.None;
        }

        timer = countdownTime;
        roundInProgress = true;
        StartCountdownClientRpc(countdownTime);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SubmitChoiceServerRpc(ChoiceType choice, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        if (playerChoices.ContainsKey(clientId) && playerChoices[clientId] == ChoiceType.None)
        {
            playerChoices[clientId] = choice;
        }

        if (AllChoicesMade())
        {
            EvaluateResult();
        }
    }

    private bool AllChoicesMade()
    {
        foreach (var choice in playerChoices.Values)
        {
            if (choice == ChoiceType.None) return false;
        }
        return true;
    }

    private void CompleteRoundWithRandomChoices()
    {
        foreach (var clientId in playerChoices.Keys)
        {
            if (playerChoices[clientId] == ChoiceType.None)
            {
                playerChoices[clientId] = (ChoiceType)Random.Range(0, 3);
            }
        }

        EvaluateResult();
    }

    private void EvaluateResult()
    {
        roundInProgress = false;

        var enumerator = playerChoices.GetEnumerator();
        enumerator.MoveNext();
        ulong p1Id = enumerator.Current.Key;
        ChoiceType p1Choice = enumerator.Current.Value;

        enumerator.MoveNext();
        ulong p2Id = enumerator.Current.Key;
        ChoiceType p2Choice = enumerator.Current.Value;

        string result;

        if (p1Choice == p2Choice)
            result = "Draw!";
        else if ((p1Choice == ChoiceType.Rock && p2Choice == ChoiceType.Scissors) ||
                 (p1Choice == ChoiceType.Paper && p2Choice == ChoiceType.Rock) ||
                 (p1Choice == ChoiceType.Scissors && p2Choice == ChoiceType.Paper))
            result = "Player 1 Wins!";
        else
            result = "Player 2 Wins!";

        SendResultClientRpc(result, p1Id, p1Choice, p2Id, p2Choice);

        Invoke(nameof(StartNewRound), 3f);
    }

    [ClientRpc]
    private void StartCountdownClientRpc(float time)
    {
        GameUI.Instance.StartCountdown(time);
    }

    [ClientRpc]
    private void UpdateTimerClientRpc(float timeLeft)
    {
        GameUI.Instance.UpdateCountdown(timeLeft);
    }

    [ClientRpc]
    private void SendResultClientRpc(string result, ulong p1Id, ChoiceType p1Choice, ulong p2Id, ChoiceType p2Choice)
    {
        GameUI.Instance.DisplayResult(result, p1Id, p1Choice, p2Id, p2Choice);
    }
}

public enum ChoiceType
{
    None = -1,
    Rock = 0,
    Paper = 1,
    Scissors = 2
}

