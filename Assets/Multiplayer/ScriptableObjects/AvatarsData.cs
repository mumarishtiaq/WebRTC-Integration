using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AvatarsData", menuName = "Scriptable Objects/AvatarsData")]
public class AvatarsData : ScriptableObject
{
   public List<GameObject> MalePrefabs;
   public List<GameObject> FemalePrefabs;
}
