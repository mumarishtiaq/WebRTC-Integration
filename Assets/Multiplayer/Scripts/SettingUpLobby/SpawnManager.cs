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

    public List<GameObject> MaleNetworkAvatars;
    public List<GameObject> FemaleNetworkAvatars;





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
        if (!_avatars) return;

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



    

    

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

}
