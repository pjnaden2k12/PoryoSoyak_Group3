using DG.Tweening;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    public float detectionRadius = 0.01f;
    public LayerMask medicineLayer;

    private Animator animator;
    private GameObject matchedMedicine;
    private bool hasEnded = false;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void OnEnable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameEnded += OnGameEnd;
    }

    void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameEnded -= OnGameEnd;
    }

    void OnGameEnd(bool isWin)
    {
        if (hasEnded) return; // tránh gọi nhiều lần

        hasEnded = true;

        if (isWin)
        {
            StartCoroutine(HandleWinSequence());
        }
        else
        {
            StartCoroutine(HandleLoseSequence());
        }
    }

    void Update()
    {
        if (hasEnded) return;

        Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position, detectionRadius, medicineLayer);

        foreach (var col in nearby)
        {
            if (col.CompareTag(gameObject.tag))
            {
                matchedMedicine = col.gameObject;
                StartCoroutine(PlaySoundWithDelay("correct", 1.0f));
                hasEnded = true;
                StartCoroutine(HandleWinSequence());
                break;
            }
        }
    }
    private IEnumerator PlaySoundWithDelay(string soundName, float delay)
    {
        yield return new WaitForSeconds(delay);
        AudioManager.Instance.Play(soundName);
    }
    private IEnumerator HandleWinSequence()
    {
        if (matchedMedicine != null)
        {
            matchedMedicine.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack);
            yield return new WaitForSeconds(0.3f);
            matchedMedicine.SetActive(false);
        }

        animator.SetTrigger("Win");
        yield return new WaitForSeconds(GetAnimationLength("Win"));

        animator.SetTrigger("Smoke");
        yield return new WaitForSeconds(GetAnimationLength("Smoke"));

        gameObject.SetActive(false);
    }

    private IEnumerator HandleLoseSequence()
    {
        animator.SetTrigger("Lose");

        

        while (true)
        {
            yield return null;
        }
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
