using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using UnityEngine;

public class NetworkServiceManager : MonoBehaviour
{
    public static NetworkServiceManager Instance { get; private set; }

    [SerializeField]
    UnityTransport m_UnityTransport;

    bool m_NetworkManagerInitialized = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    public async Task<string> InitializeHost(int maxPlayerCount)
    {
            string joinCode = "";
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayerCount);
            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            NetworkEndpoint endPoint = NetworkEndpoint.Parse(allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port);

            var ipAddress = endPoint.Address.Split(':')[0];

            m_UnityTransport.SetHostRelayData(ipAddress, endPoint.Port,
                allocation.AllocationIdBytes, allocation.Key,
                allocation.ConnectionData, false);

            Debug.Log($"Initialized Relay Host and received join code: {joinCode}");

            NetworkManager.Singleton.StartHost();

            m_NetworkManagerInitialized = true;
        }
        catch (RelayServiceException e) { Debug.LogWarning(e); }

        return joinCode;
    }

    public async Task InitializeClient(string relayJoinCode)
    {
        try
        {
            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);
            var endPoint = NetworkEndpoint.Parse(joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port);

            var ipAddress = endPoint.Address.Split(':')[0];

            m_UnityTransport.SetClientRelayData(ipAddress, endPoint.Port,
                joinAllocation.AllocationIdBytes, joinAllocation.Key,
                joinAllocation.ConnectionData, joinAllocation.HostConnectionData, false);

            NetworkManager.Singleton.StartClient();

            m_NetworkManagerInitialized = true;
        }

        catch (RelayServiceException e) { Debug.LogWarning(e); }
    }

    public void Uninitialize()
    {
        if (m_NetworkManagerInitialized)
        {
            m_NetworkManagerInitialized = false;
            NetworkManager.Singleton.Shutdown();
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
