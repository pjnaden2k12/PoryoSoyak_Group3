﻿using UnityEngine;

[CreateAssetMenu(fileName = "MapData", menuName = "Data/MapData")]
public class MapData : ScriptableObject
{
    public BlockEntry[] blocks;
    public SpawnEntry[] players;
    public SpawnEntry[] items;
    public float playTimeLimit;
}

[System.Serializable]
public struct BlockEntry
{
    public int id;
    public Vector2 position;
    public BlockType type;
    public bool hasNoBlock;
    public Vector2 exitDirection;
    public int[] medicineTypeIndices; 

    public bool hasMedicine;
   
}


[System.Serializable]
public struct SpawnEntry
{
    public Vector2 position;
    public int typeIndex;
}

[System.Serializable]
public struct MedicineEntry
{
    public int blockId;       // spawn theo id block
    public int typeIndex;
}

public enum BlockType
{
    UpLeft,
    UpRight,
    Vertical,
    Horizontal,
    DownLeft,
    DownRight
}
