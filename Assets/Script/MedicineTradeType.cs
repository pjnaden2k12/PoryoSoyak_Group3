using UnityEngine;

[RequireComponent(typeof(MedicineAutoMove))]
public class MedicineTypeSwitcher : MonoBehaviour
{
    public LayerMask itemLayerMask;
    private MedicineAutoMove medicineAutoMove;

    // Prefabs tương ứng các loại medicine
    public GameObject[] medicinePrefabs;

    // Hiện prefab medicine đang active
    private GameObject currentMedicinePrefabInstance;

    // Kiểu hiện tại
    private int currentTypeIndex = 0;

    void Awake()
    {
        medicineAutoMove = GetComponent<MedicineAutoMove>();
    }

    void Start()
    {
        SpawnMedicinePrefab(currentTypeIndex);
    }

    void Update()
    {
        if (!MedicineAutoMove.isPlayPressed) return;

        if (medicineAutoMove == null) return;

        if (medicineAutoMoveIsMoving()) return;

        CheckAndSwitchType();
    }

    bool medicineAutoMoveIsMoving()
    {
        // Đơn giản dùng biến isMoving private nên check khoảng cách gần đúng
        return medicineAutoMove != null && (medicineAutoMove.transform.position - transform.position).sqrMagnitude > 0.001f;
    }

    void CheckAndSwitchType()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, 0.3f, itemLayerMask);
        if (hit != null && hit.CompareTag("Item"))
        {
            ItemType itemType = hit.GetComponent<ItemType>();
            if (itemType != null && currentTypeIndex != itemType.typeIndex)
            {
                Debug.Log($"Switching medicine type from {currentTypeIndex} to {itemType.typeIndex}");
                currentTypeIndex = itemType.typeIndex;
                SpawnMedicinePrefab(currentTypeIndex);
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

        currentMedicinePrefabInstance = Instantiate(medicinePrefabs[typeIndex], transform.position, Quaternion.identity, transform);
    }
}
