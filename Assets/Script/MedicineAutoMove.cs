using UnityEngine;
using DG.Tweening;

public class MedicineAutoMove : MonoBehaviour
{
    public float moveDelay = 0.2f;       // Thời gian nghỉ giữa các lần di chuyển
    public float moveDuration = 0.3f;    // Thời gian để di chuyển 1f
    private bool isMoving = false;

    private int maxId = 0;
    private int blockLayerMask;
    public static bool isPlayPressed = false;
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
            Debug.LogWarning("Player not on a valid block.");
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

        // Di chuyển 1f trong thời gian cố định (ví dụ 0.3 giây)
        transform.DOMove(destination, moveDuration)
            .SetEase(Ease.InOutSine)
            .OnComplete(() =>
            {
                DOVirtual.DelayedCall(moveDelay, () =>
                {
                    isMoving = false;
                });
            });
    }
}
