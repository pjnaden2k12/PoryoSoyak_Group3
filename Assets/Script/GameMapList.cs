using UnityEngine;

[CreateAssetMenu(fileName = "GameMapList", menuName = "Data/Game Map List")]
public class GameMapList : ScriptableObject
{
    public MapData[] allMaps;
}
