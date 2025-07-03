using UnityEngine;
using System.Collections.Generic;

public class MapSpawner : MonoBehaviour
{
    public MapData mapData;

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

    private Dictionary<int, Transform> spawnedBlocksById = new();

    void Start()
    {
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

            GameObject block = Instantiate(prefab, (Vector3)data.position, Quaternion.identity);
            block.tag = "Block";

            BlockID blockID = block.GetComponent<BlockID>();
            if (blockID != null)
                blockID.id = data.id;

            if (data.hasNoBlock && noBlockChildPrefab != null)
            {
                GameObject child = Instantiate(noBlockChildPrefab, block.transform);
                child.transform.localPosition = Vector3.zero;
            }

            spawnedBlocksById[data.id] = block.transform;
        }
    }

    void SpawnPlayers()
    {
        foreach (var p in mapData.players)
        {
            if (p.typeIndex < playerPrefabs.Length)
                Instantiate(playerPrefabs[p.typeIndex], (Vector3)p.position, Quaternion.identity);
        }
    }

    void SpawnItems()
    {
        foreach (var item in mapData.items)
        {
            if (item.typeIndex < itemPrefabs.Length)
                Instantiate(itemPrefabs[item.typeIndex], (Vector3)item.position, Quaternion.identity);
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
                    Instantiate(medicinePrefabs[typeIndex], blockTransform.position, Quaternion.identity);
                    // Nếu muốn làm child:
                    // Instantiate(medicinePrefabs[typeIndex], blockTransform).transform.localPosition = Vector3.zero;
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
