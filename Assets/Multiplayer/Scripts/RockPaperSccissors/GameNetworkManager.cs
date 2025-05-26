using Unity.Netcode;
using UnityEngine;

public class GameNetworkManager : NetworkBehaviour
{
    public static GameNetworkManager Instance;

    private NetworkVariable<ChoiceType> player1Choice = new NetworkVariable<ChoiceType>(ChoiceType.None);
    private NetworkVariable<ChoiceType> player2Choice = new NetworkVariable<ChoiceType>(ChoiceType.None);

    private ulong player1Id;
    private ulong player2Id;



    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            var clients = NetworkManager.ConnectedClientsList;
            player1Id = clients[0].ClientId;
            player2Id = clients.Count > 1 ? clients[1].ClientId : 0;
        }

        Instance = this;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SubmitChoiceServerRpc(ChoiceType choice, ServerRpcParams rpcParams = default)
    {
        if (rpcParams.Receive.SenderClientId == player1Id)
            player1Choice.Value = choice;
        else if (rpcParams.Receive.SenderClientId == player2Id)
            player2Choice.Value = choice;

        if (player1Choice.Value != ChoiceType.None && player2Choice.Value != ChoiceType.None)
            CheckWinner();
    }

    private void CheckWinner()
    {
        ChoiceType p1 = player1Choice.Value;
        ChoiceType p2 = player2Choice.Value;

        string result;
        if (p1 == p2)
            result = "Draw!";
        else if ((p1 == ChoiceType.Rock && p2 == ChoiceType.Scissors) ||
                 (p1 == ChoiceType.Paper && p2 == ChoiceType.Rock) ||
                 (p1 == ChoiceType.Scissors && p2 == ChoiceType.Paper))
            result = "Player 1 Wins!";
        else
            result = "Player 2 Wins!";

        SendResultClientRpc(result);

        // Reset for next round
        player1Choice.Value = ChoiceType.None;
        player2Choice.Value = ChoiceType.None;
    }

    [ClientRpc]
    private void SendResultClientRpc(string result)
    {
        GameUI.Instance.DisplayResult(result);
    }
}

public enum ChoiceType
{
    None = -1,
    Rock = 0,
    Paper = 1,
    Scissors = 2
}
