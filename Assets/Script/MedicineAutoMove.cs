using UnityEngine;
using DG.Tweening;

public class MedicineAutoMove : MonoBehaviour
{
    public float moveDelay = 0.2f;
    public float moveDuration = 0.3f;
    private bool isMoving = false;

    private int maxId = 0;
    private int blockLayerMask;
    public static bool isPlayPressed = false;

    public GameObject[] medicinePrefabs;
    private GameObject currentMedicinePrefabInstance;

    public int currentTypeIndex = 0;

    void Start()
    {
        blockLayerMask = LayerMask.GetMask("BlockLayer");

        BlockID[] allBlocks = Object.FindObjectsByType<BlockID>(FindObjectsSortMode.None);
        maxId = 0;
        foreach (var block in allBlocks)
        {
            if (block.id > maxId)
                maxId = block.id;
        }
        maxId += 1;

        SpawnMedicinePrefab(currentTypeIndex);
    }

    void Update()
    {
        if (!isPlayPressed) return;

        if (!isMoving)
            TryMoveToNextBlock();
    }

    void TryMoveToNextBlock()
    {
        int currentId = -1;

        Collider2D currentBlock = Physics2D.OverlapCircle(transform.position, 0.1f, blockLayerMask);
        if (currentBlock != null && currentBlock.CompareTag("Block"))
        {
            BlockID blockIDComponent = currentBlock.GetComponent<BlockID>();
            if (blockIDComponent != null)
                currentId = blockIDComponent.id;
        }

        if (currentId == -1)
        {
            Debug.LogWarning("Medicine not on a valid block.");
            return;
        }

        int nextId = (currentId + 1) % maxId;

        BlockID[] allBlocks = Object.FindObjectsByType<BlockID>(FindObjectsSortMode.None);

        BlockID closestBlock = null;
        float closestDistance = float.MaxValue;

        foreach (var block in allBlocks)
        {
            if (block.id == nextId)
            {
                float distance = Vector3.Distance(transform.position, block.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestBlock = block;
                }
            }
        }

        if (closestBlock != null)
        {
            MoveToBlock(closestBlock.transform.position);
        }
        else
        {
            Debug.LogWarning($"No block found with ID {nextId}");
        }
    }

    void MoveToBlock(Vector3 destination)
    {
        isMoving = true;

        transform.DOMove(destination, moveDuration)
            .SetEase(Ease.InOutSine)
            .OnComplete(() =>
            {
                CheckForItem();

                DOVirtual.DelayedCall(moveDelay, () =>
                {
                    isMoving = false;
                });
            });
    }

    void CheckForItem()
    {
        Collider2D item = Physics2D.OverlapCircle(transform.position, 0.1f);
        if (item != null && item.CompareTag("Item"))
        {
            ItemType itemType = item.GetComponent<ItemType>();
            if (itemType != null)
            {
                int itemTypeIndex = itemType.typeIndex;
                if (itemTypeIndex != currentTypeIndex)
                {
                    currentTypeIndex = itemTypeIndex;
                    SpawnMedicinePrefab(currentTypeIndex);
                    Debug.Log($"Medicine type changed to {currentTypeIndex} due to item collision.");
                }
            }
        }
    }

    void SpawnMedicinePrefab(int typeIndex)
    {
        if (currentMedicinePrefabInstance != null)
            Destroy(currentMedicinePrefabInstance);

        if (typeIndex < 0 || typeIndex >= medicinePrefabs.Length)
        {
            Debug.LogWarning("Invalid medicine prefab index.");
            return;
        }

        currentMedicinePrefabInstance = Instantiate(medicinePrefabs[typeIndex], transform.position, Quaternion.identity);
        currentMedicinePrefabInstance.name = $"Medicine_Type_{typeIndex}";
        currentMedicinePrefabInstance.transform.parent = null;
    }
}
