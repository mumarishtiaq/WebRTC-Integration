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

   

   

    [SerializeField] private GameObject _localPlayerAvatar;
    [SerializeField] private GameObject _remotePlayerAvatar;

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
        _localPlayerAvatar = character;
    }

    public void SetTransform_LocalPlayer()
    {
        SetTransform(_localPlayerAvatar, GetTargetTransform(true));
    }
    
    public void SpawnRemotePlayer(int avatarIndex , PlayerGender gender)
    {
        if (!_avatars) return;

        var character = SpawnPlayer(avatarIndex, gender);
        _remotePlayerAvatar = character;

        SetTransform(_remotePlayerAvatar, GetTargetTransform(false));
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
        var aa =targetTransforms.FirstOrDefault(obj => obj.name.ToLower().Contains(key)).transform;
        Debug.Log(aa.name, aa.gameObject);
        return aa;
    }



    

    

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

}
