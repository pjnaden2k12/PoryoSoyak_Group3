using DG.Tweening;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    public float detectionRadius = 0.01f;
    public LayerMask medicineLayer;

    private Animator animator;
    private bool hasWon = false;
    private GameObject matchedMedicine; // 🆕 lưu object chạm vào

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (hasWon) return;

        Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position, detectionRadius, medicineLayer);

        foreach (var col in nearby)
        {
            if (col.CompareTag(gameObject.tag))
            {
                matchedMedicine = col.gameObject; // 🆕 lưu lại object chạm
                StartCoroutine(HandleWinSequence());
                break;
            }
        }
    }

    private IEnumerator HandleWinSequence()
    {
        hasWon = true;
        if (matchedMedicine != null)
        {
            // Tween thu nhỏ trước khi ẩn
            matchedMedicine.transform.DOScale(Vector3.zero, 0.3f)
                .SetEase(Ease.InBack);

            yield return new WaitForSeconds(0.3f); // chờ tween chạy xong

            matchedMedicine.SetActive(false);
        }
        animator.SetTrigger("Win");
        yield return new WaitForSeconds(GetAnimationLength("Win"));

        animator.SetTrigger("Smoke");
        yield return new WaitForSeconds(GetAnimationLength("Smoke"));

        

        gameObject.SetActive(false);
    }


    float GetAnimationLength(string animName)
    {
        RuntimeAnimatorController ac = animator.runtimeAnimatorController;

        foreach (var clip in ac.animationClips)
        {
            if (clip.name == animName)
                return clip.length;
        }

        return 1f;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
