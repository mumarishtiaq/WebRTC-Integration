using GLTFast.Schema;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance;



    [Header("Avatars Data")]
    [SerializeField] private AvatarsData _avatars;


    [Header("Male And Female Spawn Positions")]
    [SerializeField] private Transform _localPlayerReferenceTransform;
    [SerializeField] private Transform _remotePlayerReferenceTransform;

    [HideInInspector]
    public int AvatarIndex = 0;





   public PlayerAnimationController LocalPlayerAvatar;
   public PlayerAnimationController RemotePlayerAvatar;

   




    private async void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }
    private void Start()
    {
        LobbyManager.Instance.OnGameStarted.AddListener(()=>TogglePlayersVisiblity());

    }

    public void SpawnLocalPlayer(PlayerGender gender)
    {
        if (!_avatars) return;

        var character = SpawnPlayer(AvatarIndex, gender);

        LocalPlayerAvatar = character.GetComponent<PlayerAnimationController>();
    }

    public void SetTransform_LocalPlayer()
    {
        SetTransform(LocalPlayerAvatar.gameObject, GetTargetTransform(true));
    }
    
    public void SpawnRemotePlayer(int avatarIndex , PlayerGender gender)
    {
        if (!_avatars || RemotePlayerAvatar != null) return;


        var character = SpawnPlayer(avatarIndex, gender);
        RemotePlayerAvatar = character.GetComponent<PlayerAnimationController>();

        SetTransform(RemotePlayerAvatar.gameObject, GetTargetTransform(false));
    }

    private GameObject SpawnPlayer(int avatarIndex, PlayerGender gender)
    {
        var prefab = gender == PlayerGender.Male ? _avatars.MalePrefabs[avatarIndex] : _avatars.FemalePrefabs[avatarIndex];

        var avatar = Instantiate(prefab);
        DontDestroyOnLoad(avatar);
        return avatar;

    }

    private void SetTransform(GameObject player,Transform p_transform)
    {
        if(player == null || p_transform == null) return;

        player.transform.position = p_transform.position;
        player.transform.rotation = p_transform.rotation;
        player.transform.localScale = p_transform.localScale;
    }

    private Transform GetTargetTransform(bool isLocal)
    {
        var targetTransforms = GameObject.FindGameObjectsWithTag("TargetTransform");

        string key = isLocal ? "local" : "remote";

        //return targetTransforms.FirstOrDefault(obj => obj.name.ToLower() == key).transform;
        return targetTransforms.FirstOrDefault(obj => obj.name.ToLower().Contains(key)).transform;
        
    }

    public void SetupLipSyncComponents(ParticipantType pType, GameObject obj = null)
    {
        SkinnedMeshRenderer rend = pType == ParticipantType.Local ? LocalPlayerAvatar.HeadMesh : RemotePlayerAvatar.HeadMesh;
        bool loopback = pType == ParticipantType.Local ? false : true;

        if (obj != null)
        {
            var lipSync = obj.AddComponent<OVRLipSyncContext>();

            //getting and setting up audio source
            var src = obj.GetComponent<AudioSource>();
          
            lipSync.audioSource = src;
            lipSync.audioLoopback = loopback;

            var morph = obj.AddComponent<OVRLipSyncContextMorphTarget>();
            morph.visemeToBlendTargets = new int[15];
            for (int i = 0; i < 15; i++)
            {
                morph.visemeToBlendTargets[i] = i + 1;
            }
            morph.skinnedMeshRenderer = rend;
        }
        //if obj is null so we are setting lipSync components for local player
        else
        {
            Debug.Log("Tap Obj is null");
            GameObject lipSyncHolder = new GameObject("LocalPlayerLipSyncComponents");
            SetupLipSyncComponents(pType, lipSyncHolder);
            lipSyncHolder.AddComponent<OVRLipSyncMicInput>();
            DontDestroyOnLoad(lipSyncHolder);
        }
    }

    public void TriggerAnimations(ParticipantType pType , AnimationType animType)
    {
        var player = pType == ParticipantType.Local ? LocalPlayerAvatar : RemotePlayerAvatar;

        player.TriggerAnimation(animType);

    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
        LobbyManager.Instance.OnGameStarted.RemoveListener(() => TogglePlayersVisiblity());

    }

    public void TogglePlayersVisiblity(bool state = false)
    {
        if(LocalPlayerAvatar)
            LocalPlayerAvatar.gameObject.SetActive(state);  
        
        if(RemotePlayerAvatar)
            RemotePlayerAvatar.gameObject.SetActive(state);
    }

}

public enum AnimationType
{
    None,
    Idle,
    Sit
}

