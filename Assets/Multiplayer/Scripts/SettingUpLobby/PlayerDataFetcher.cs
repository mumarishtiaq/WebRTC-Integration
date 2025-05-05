using System;
using System.Threading.Tasks;
using UnityEngine;

public static class PlayerDataFetcher
{

    /// <summary>
    /// Simulates fetching real player data from the app or backend.
    /// </summary>
    /// <returns>A Task returning real PlayerData.</returns>
    public static async Task<PeerData> FetchDataFromApp()
    {
        // Simulated delay for fetching real data
        await Task.Delay(500);

        Debug.Log("Fetching real player data from the app...");

        // TODO: Replace with actual implementation
        return new PeerData();
    }

    /// <summary>
    /// Creates and returns dummy player data for development/testing.
    /// </summary>
    /// <returns>Dummy PlayerData object.</returns>
    public static async Task<PeerData> PopulateDummyData(bool isHost = true)
    {
        // Simulated delay for fetching real data
        await Task.Delay(500);
        Debug.Log("Populating dummy player data for development...");

        var peerData = new PeerData();

        if (isHost)
        {
            peerData.LP = GetPlayerData("dummy-id-001", "John", PlayerGender.Male, PlayerRole.Host);
            peerData.RP = GetPlayerData("ddummy-id-002", "Elizabeth", PlayerGender.Female, PlayerRole.Client);
        }
        else
        {
            peerData.LP = GetPlayerData("ddummy-id-002", "Elizabeth", PlayerGender.Female, PlayerRole.Client);
            peerData.RP = GetPlayerData("dummy-id-001", "John", PlayerGender.Male, PlayerRole.Host);
        }
            peerData.CommonRoomName = "John_Elizabeth";


        return peerData;
    }

    private static PlayerData GetPlayerData(string id, string name,PlayerGender gender,PlayerRole role)
    {
        return new PlayerData { 
            ID = id,
            Name = name,
            Gender = gender,
            Role = role
        };
    }
}

[Serializable]
public class PeerData
{
    public PlayerData LP; //local player data
    public PlayerData RP; //Remote player data
    public string CommonRoomName;
}

[Serializable]
public class PlayerData
{
    public string ID;
    public string Name;
    public PlayerGender Gender;
    public PlayerRole Role;
}
public enum PlayerGender
{
    None,
    Male,
    Female
}
public enum PlayerRole
{
    Host = 0,
    Client = 1
}

