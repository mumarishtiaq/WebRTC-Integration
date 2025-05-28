using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine.SocialPlatforms;
using System;

namespace Games.RockPaperScissors
{
    public class GameNetworkManager : NetworkBehaviour
    {
        public static GameNetworkManager Instance;

        private Dictionary<ulong, ChoiceType> playerChoices = new();
        private Dictionary<ulong, int> playerScores = new();

        private ulong localPlayerId => NetworkManager.Singleton.LocalClientId;

        private bool roundInProgress = false;

        private void Awake()
        {
            Instance = this;

        }

        private void Start()
        {
            //will only run in real scenario
            if (IsServer)
            {
                OnGameStarted();
            }
        }



        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                OnGameStarted();
                //will only binf when testing and will not work on real sceneario
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            }
        }

        private void OnGameStarted()
        {
            playerChoices.Clear();
            playerScores.Clear();

            foreach (var client in NetworkManager.ConnectedClientsList)
            {
                playerScores[client.ClientId] = 0;
            }
            StartNewRound();
        }
        private void OnClientConnected(ulong obj)
        {
            playerChoices.Clear();
            playerScores.Clear();

            foreach (var client in NetworkManager.ConnectedClientsList)
            {
                playerScores[client.ClientId] = 0;
            }
            StartNewRound();
        }

        private void StartNewRound()
        {
            playerChoices.Clear();
            foreach (var client in NetworkManager.ConnectedClientsList)
            {
                playerChoices[client.ClientId] = ChoiceType.None;
            }

            roundInProgress = true;
            StartNewRoundClientRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        public void SubmitChoiceServerRpc(ChoiceType choice, ServerRpcParams rpcParams = default)
        {
            ulong clientId = rpcParams.Receive.SenderClientId;

            if (!roundInProgress || !playerChoices.ContainsKey(clientId) || playerChoices[clientId] != ChoiceType.None)
                return;

            playerChoices[clientId] = choice;

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

            string result = "";
            ulong winnerId = 0;

            //setting wrong winner id because we have only two players here, so this will draw the match
            if (p1Choice == p2Choice)
            {
                //result = "Draw!";
                winnerId = 5;

            }


            else
            {
                bool player1Wins =
                    (p1Choice == ChoiceType.Rock && p2Choice == ChoiceType.Scissors) ||
                     (p1Choice == ChoiceType.Paper && p2Choice == ChoiceType.Rock) ||
                     (p1Choice == ChoiceType.Scissors && p2Choice == ChoiceType.Paper);

                winnerId = player1Wins ? p1Id : p2Id;


                if (playerScores.ContainsKey(winnerId))
                    playerScores[winnerId]++;

            }

            SendResultClientRpc(winnerId, p1Id, p1Choice, p2Id, p2Choice);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestRematchServerRpc(ServerRpcParams rpcParams = default)
        {
            if (!IsServer) return;
            StartNewRound();
        }

        [ClientRpc]
        private void SendResultClientRpc(ulong winnerId, ulong p1Id, ChoiceType p1Choice, ulong p2Id, ChoiceType p2Choice)
        {
            GameUI.Instance.DisplayResult(GetMatchResultData(winnerId, p1Id, p1Choice, p2Id, p2Choice));
        }

        private MatchResultData GetMatchResultData(ulong winnerId, ulong p1Id, ChoiceType p1Choice, ulong p2Id, ChoiceType p2Choice)
        {
            bool isLocalPlayerFirst = p1Id == localPlayerId;

            var localChoice = isLocalPlayerFirst ? p1Choice : p2Choice;
            var remoteChoice = isLocalPlayerFirst ? p2Choice : p1Choice;

            // Optional: replace hardcoded names with dynamic player names if available
            string remoteName = MultiplayerManager.Instance == null ? "Opponent" : MultiplayerManager.Instance.PeerData.RP.Name;


            var localScore = playerScores.ContainsKey(localPlayerId) ? playerScores[localPlayerId] : 0;
            var remoteId = isLocalPlayerFirst ? p2Id : p1Id;
            var remoteScore = playerScores.ContainsKey(remoteId) ? playerScores[remoteId] : 0;

            var resultData = new MatchResultData
            {
                localPlayer = new PlayerInfo("You", localChoice,localScore),
                remotePLayer = new PlayerInfo(remoteName, remoteChoice,remoteScore),
                result = winnerId > 1
                    ? "Draw!"
                    : (winnerId == localPlayerId ? "You Win!" : "You Lose!")
            };

            return resultData;
        }
        [ClientRpc]
        private void StartNewRoundClientRpc()
        {
            GameUI.Instance.ResetUIForNewRound();
        }

        public ulong GetLocalPlayerID()
        {
            return localPlayerId;
        }
    }

    public enum ChoiceType
    {
        None = -1,
        Rock = 0,
        Paper = 1,
        Scissors = 2
    }

    public class PlayerInfo
    {
        public string name;
        public ChoiceType choice;
        public int score;

        public PlayerInfo(string name, ChoiceType choice, int score = 0)
        {
            this.name = name;
            this.choice = choice;
            this.score = score;
        }
    }

    public class MatchResultData
    {
        public PlayerInfo localPlayer;
        public PlayerInfo remotePLayer;
        public string result;
    }

}




