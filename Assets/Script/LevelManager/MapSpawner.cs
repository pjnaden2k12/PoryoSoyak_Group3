using System;
using System.Collections.Generic;
using UnityEngine;

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

    public void SpawnMap(int mapIndex)
    {
        ClearMap();

        if (mapList == null || mapIndex < 0 || mapIndex >= mapList.allMaps.Length)
        {
            Debug.LogError("Map index không hợp lệ hoặc mapList chưa được gán.");
            return;
        }

        mapData = mapList.allMaps[mapIndex];

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetCurrentMap(mapIndex);
        }

        SpawnBlocks();
        SpawnPlayers();
        SpawnItems();
        SpawnMedicines();

        OnMapSpawned?.Invoke();
    }

    public void ResetMap()
    {
        ClearMap();
    }

    private void ClearMap()
    {
        // Xóa toàn bộ con để reset
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        spawnedBlocksById.Clear();
        BlockPositionManager.blockPositions.Clear();
    }

    void SpawnBlocks()
    {
        foreach (var data in mapData.blocks)
        {
            GameObject prefab = GetBlockPrefab(data.type);
            if (prefab == null) continue;

            // Spawn block làm child của đối tượng chứa MapSpawner
            GameObject block = Instantiate(prefab, (Vector3)data.position, Quaternion.identity, transform);  // 'transform' là đối tượng chứa script MapSpawner
            block.tag = "Block";

            BlockID blockID = block.GetComponent<BlockID>();
            if (blockID != null)
            {
                blockID.id = data.id;
                blockID.exitDirection = data.exitDirection;
            }

            if (data.hasNoBlock && noBlockChildPrefab != null)
            {
                // Spawn child nếu có
                GameObject child = Instantiate(noBlockChildPrefab, block.transform);
                child.transform.localPosition = Vector3.zero;
            }

            spawnedBlocksById[data.id] = block.transform;
            Vector2Int posInt = new Vector2Int(Mathf.RoundToInt(data.position.x), Mathf.RoundToInt(data.position.y));
            BlockPositionManager.blockPositions.Add(posInt);

            if (blockID != null && !blockMap.ContainsKey(posInt))
            {
                blockMap.Add(posInt, blockID);
            }
        }
    }

    void SpawnPlayers()
    {
        foreach (var p in mapData.players)
        {
            if (p.typeIndex < playerPrefabs.Length)
            {
                // Spawn player làm child của đối tượng chứa MapSpawner
                Instantiate(playerPrefabs[p.typeIndex], (Vector3)p.position, Quaternion.identity, transform); // 'transform' là đối tượng chứa script MapSpawner
            }
        }
    }

    void SpawnItems()
    {
        foreach (var item in mapData.items)
        {
            if (item.typeIndex < itemPrefabs.Length)
            {
                // Spawn item làm child của đối tượng chứa MapSpawner
                Instantiate(itemPrefabs[item.typeIndex], (Vector3)item.position, Quaternion.identity, transform); // 'transform' là đối tượng chứa script MapSpawner
            }
        }
    }

    void SpawnMedicines()
    {
        foreach (var block in mapData.blocks)
        {
            if (!block.hasMedicine)
                continue;

            if (!spawnedBlocksById.TryGetValue(block.id, out Transform blockTransform))
                continue;

            int typeIndex = block.medicineTypeIndex;  // Lấy một chỉ số duy nhất

            if (typeIndex >= 0 && typeIndex < medicinePrefabs.Length)
            {
                Instantiate(medicinePrefabs[typeIndex], blockTransform.position, Quaternion.identity, transform);
            }
            else
            {
                Debug.LogWarning($"Invalid medicine typeIndex {typeIndex} on block ID {block.id}");
            }
        }
    }


    private GameObject GetBlockPrefab(BlockType type)
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

    // Để DragMove có thể gọi set lại vị trí bắt đầu
    public static Dictionary<Vector2Int, BlockID> blockMap = new();
}

public static class BlockPositionManager
{
    public static HashSet<Vector2Int> blockPositions = new();
}
