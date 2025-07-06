using System;
using System.Collections.Generic;
using UnityEngine;

public class MapSpawner : MonoBehaviour
{
    [Header("Map Data")]
    public GameMapList mapList;

    [Header("Block Prefabs")]
    public GameObject prefabUpLeft;
    public GameObject prefabUpRight;
    public GameObject prefabVertical;
    public GameObject prefabHorizontal;
    public GameObject prefabDownLeft;
    public GameObject prefabDownRight;
    public GameObject noBlockChildPrefab;

    [Header("Other Prefabs")]
    public GameObject[] playerPrefabs;
    public GameObject[] itemPrefabs;
    public GameObject[] medicinePrefabs;

    private MapData currentMapData;
    public static Dictionary<Vector2Int, BlockID> blockMap;

    // Thêm sự kiện để GameManagerUI có thể lắng nghe
    public static event Action OnMapSpawned;

    void Awake()
    {
        // Luôn dọn dẹp và khởi tạo lại các biến tĩnh
        blockMap = new Dictionary<Vector2Int, BlockID>();
        InstantBridge.ActiveBridges.Clear();
    }

    // Hàm public để các script khác gọi đến và bắt đầu spawn map
    public void SpawnMap(int mapIndex)
    {
        ClearCurrentMap();

        if (mapList == null || mapIndex < 0 || mapIndex >= mapList.allMaps.Length)
        {
            Debug.LogError("Map index không hợp lệ!");
            return;
        }

        currentMapData = mapList.allMaps[mapIndex];

        // Truyền thời gian giới hạn vào GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetCurrentMap(mapIndex);  // Điều này đã bao gồm cả việc thiết lập thời gian
        }

        // Thực hiện spawn các đối tượng
        SpawnBlocks();
        SpawnPlayers();
        SpawnItems();
        SpawnMedicines();

        // Gửi thông báo rằng map đã được spawn xong
        OnMapSpawned?.Invoke();
    }


    // Hàm public để LevelManager gọi khi cần reset
    public void ResetMap()
    {
        ClearCurrentMap();
    }

    private void ClearCurrentMap()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        blockMap.Clear();
        InstantBridge.ActiveBridges.Clear();
    }

    void SpawnBlocks()
    {
        if (currentMapData.blocks == null) return;
        foreach (var data in currentMapData.blocks)
        {
            GameObject prefab = GetBlockPrefab(data.type);
            if (prefab == null) continue;

            GameObject block = Instantiate(prefab, (Vector3)data.position, Quaternion.identity, transform);
            block.layer = LayerMask.NameToLayer("BlockLayer");

            BlockID blockID = block.GetComponent<BlockID>();
            if (blockID != null)
            {
                blockID.id = data.id;
                blockID.exitDirection = data.exitDirection;
            }
            if (data.hasNoBlock && noBlockChildPrefab != null)
            {

                GameObject child = Instantiate(noBlockChildPrefab, block.transform);
                child.transform.localPosition = Vector3.zero;
            }

            Vector2Int posInt = new Vector2Int(Mathf.RoundToInt(data.position.x), Mathf.RoundToInt(data.position.y));
            if (blockID != null && !blockMap.ContainsKey(posInt))
            {
                blockMap.Add(posInt, blockID);
            }
        }
    }

    // Cập nhật lại hàm SpawnMedicines để dùng mảng medicineTypeIndices
    void SpawnMedicines()
    {
        if (currentMapData.blocks == null) return;
        foreach (var blockData in currentMapData.blocks)
        {
            if (blockData.hasMedicine && blockData.medicineTypeIndices != null)
            {
                if (blockMap.TryGetValue(new Vector2Int(Mathf.RoundToInt(blockData.position.x), Mathf.RoundToInt(blockData.position.y)), out BlockID block))
                {
                    foreach (var typeIndex in blockData.medicineTypeIndices)
                    {
                        if (typeIndex >= 0 && typeIndex < medicinePrefabs.Length)
                        {
                            Instantiate(medicinePrefabs[typeIndex], block.transform.position, Quaternion.identity, transform);
                        }
                    }
                }
            }
        }
    }

    // Các hàm SpawnPlayers và SpawnItems không có lỗi, có thể giữ nguyên
    void SpawnPlayers()
    {
        if (currentMapData.players == null) return;
        foreach (var p in currentMapData.players)
        {
            if (p.typeIndex < playerPrefabs.Length)
            {
                Instantiate(playerPrefabs[p.typeIndex], (Vector3)p.position, Quaternion.identity, transform);
            }
        }
    }

    void SpawnItems()
    {
        if (currentMapData.items == null) return;
        foreach (var item in currentMapData.items)
        {
            if (item.typeIndex < itemPrefabs.Length)
            {
                Instantiate(itemPrefabs[item.typeIndex], (Vector3)item.position, Quaternion.identity, transform);
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
}