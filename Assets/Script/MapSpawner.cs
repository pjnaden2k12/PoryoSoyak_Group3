using System.Collections.Generic;
using UnityEngine;

public class MapSpawner : MonoBehaviour
{
    [Header("Map Data")]
    public GameMapList mapList;     // <-- ScriptableObject chứa danh sách các map
    public int selectedMapIndex;    // <-- chọn map nào để spawn

    private MapData mapData;        // <-- map đang spawn

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
    [Header("Teleporter Prefabs")]
    public GameObject[] teleporterPrefabs;

    [Header("Item Prefabs")]
    public GameObject[] itemPrefabs;

    [Header("Medicine Prefabs")]
    public GameObject[] medicinePrefabs;

    private Dictionary<int, Transform> spawnedBlocksById = new();
    public static Dictionary<Vector2Int, BlockID> blockMap = new();

    private void Awake()
    {
        blockMap.Clear();
    }
    void Start()
    {
        if (mapList == null || selectedMapIndex < 0 || selectedMapIndex >= mapList.allMaps.Length)
        {
            Debug.LogError("Map không hợp lệ hoặc chưa được cấu hình.");
            return;
        }

        mapData = mapList.allMaps[selectedMapIndex]; // lấy map cần spawn

        SpawnBlocks();
        SpawnPlayers();
        SpawnItems();
        SpawnMedicines();
       
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
            if (!block.hasMedicine || block.medicineTypeIndices == null)
                continue;

            if (!spawnedBlocksById.TryGetValue(block.id, out Transform blockTransform))
                continue;

            foreach (var typeIndex in block.medicineTypeIndices)
            {
                if (typeIndex >= 0 && typeIndex < medicinePrefabs.Length)
                {
                    // Spawn medicine làm child của đối tượng chứa MapSpawner
                    Instantiate(medicinePrefabs[typeIndex], blockTransform.position, Quaternion.identity, transform); // 'transform' là đối tượng chứa script MapSpawner
                }
                else
                {
                    Debug.LogWarning($"Invalid medicine typeIndex {typeIndex} on block ID {block.id}");
                }
            }
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
