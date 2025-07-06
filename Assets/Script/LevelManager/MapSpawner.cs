using System.Collections.Generic;
using UnityEngine;
using System;

public class MapSpawner : MonoBehaviour
{
    [Header("Map Data List")]
    public GameMapList mapList;

    [Header("Block Prefabs")]
    public GameObject prefabUpLeft;
    public GameObject prefabUpRight;
    public GameObject prefabVertical;
    public GameObject prefabHorizontal;
    public GameObject prefabDownLeft;
    public GameObject prefabDownRight;
    public GameObject noBlockChildPrefab;

    [Header("Player Prefabs")]
    public GameObject[] playerPrefabs;

    [Header("Item Prefabs")]
    public GameObject[] itemPrefabs;

    [Header("Medicine Prefabs")]
    public GameObject[] medicinePrefabs;

    private MapData mapData;
    private Dictionary<int, Transform> spawnedBlocksById = new();
    public static event Action OnMapSpawned;
    public void SpawnMap()
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        spawnedBlocksById.Clear();
        BlockPositionManager.blockPositions.Clear();

        int index = LevelManager.Instance.SelectedMapIndex;
        if (mapList == null || index < 0 || index >= mapList.allMaps.Length) return;

        mapData = mapList.allMaps[index];

        // Gọi cập nhật map hiện tại và thời gian cho GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetCurrentMap(index);
        }

        SpawnBlocks();
        SpawnPlayers();
        SpawnItems();
        SpawnMedicines();

        OnMapSpawned?.Invoke();
    }


    public void ResetMap()
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);
    }

    void SpawnBlocks()
    {
        foreach (var data in mapData.blocks)
        {
            GameObject prefab = GetBlockPrefab(data.type);
            if (prefab == null) continue;

            GameObject block = Instantiate(prefab, (Vector3)data.position, Quaternion.identity, transform);
            block.tag = "Block";

            BlockID blockID = block.GetComponent<BlockID>();
            if (blockID != null)
                blockID.id = data.id;

            if (data.hasNoBlock && noBlockChildPrefab != null)
                Instantiate(noBlockChildPrefab, block.transform).transform.localPosition = Vector3.zero;

            spawnedBlocksById[data.id] = block.transform;

            Vector2Int posInt = new Vector2Int(Mathf.RoundToInt(data.position.x), Mathf.RoundToInt(data.position.y));
            BlockPositionManager.blockPositions.Add(posInt);
        }
    }

    void SpawnPlayers()
    {
        foreach (var p in mapData.players)
        {
            if (p.typeIndex < playerPrefabs.Length)
                Instantiate(playerPrefabs[p.typeIndex], (Vector3)p.position, Quaternion.identity, transform);
        }
    }

    void SpawnItems()
    {
        foreach (var item in mapData.items)
        {
            if (item.typeIndex < itemPrefabs.Length)
            {
                GameObject itemObj = Instantiate(itemPrefabs[item.typeIndex], (Vector3)item.position, Quaternion.identity, transform);
                var dragItem = itemObj.GetComponent<DragItem>();
                if (dragItem != null)
                    dragItem.SetStartPosition(item.position);
            }
        }
    }

    void SpawnMedicines()
    {
        foreach (var block in mapData.blocks)
        {
            if (!block.hasMedicine) continue;
            if (!spawnedBlocksById.TryGetValue(block.id, out Transform blockTransform)) continue;

            int typeIndex = block.medicineTypeIndices;
            if (typeIndex >= 0 && typeIndex < medicinePrefabs.Length)
                Instantiate(medicinePrefabs[typeIndex], blockTransform.position, Quaternion.identity, transform);
        }
    }

    GameObject GetBlockPrefab(BlockType type)
    {
        return type switch
        {
            BlockType.UpLeft => prefabUpLeft,
            BlockType.UpRight => prefabUpRight,
            BlockType.Vertical => prefabVertical,
            BlockType.Horizontal => prefabHorizontal,
            BlockType.DownLeft => prefabDownLeft,
            BlockType.DownRight => prefabDownRight,
            _ => null,
        };
    }
}

public static class BlockPositionManager
{
    public static HashSet<Vector2Int> blockPositions = new();
}
