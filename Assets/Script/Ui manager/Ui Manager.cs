using UnityEngine;
using DG.Tweening; // Đảm bảo bạn đã import DOTween

public class UiManager : MonoBehaviour
{
    public GameObject HomePanel;
    public GameObject HelpPanel;
    public GameObject Selectlevelpanel;

    private CanvasGroup homeGroup;
    private CanvasGroup helpGroup;
    private CanvasGroup selectGroup;

    private void Start()
    {
        homeGroup = SetupCanvasGroup(HomePanel);
        helpGroup = SetupCanvasGroup(HelpPanel);
        selectGroup = SetupCanvasGroup(Selectlevelpanel);

        ShowPanel(homeGroup);
        HidePanel(helpGroup, false);
        HidePanel(selectGroup, false);
    }

    private CanvasGroup SetupCanvasGroup(GameObject panel)
    {
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        if (cg == null) cg = panel.AddComponent<CanvasGroup>();
        return cg;
    }

    private void ShowPanel(CanvasGroup cg, float duration = 0.5f)
    {
        cg.gameObject.SetActive(true);
        cg.transform.localScale = Vector3.one * 0.8f;
        cg.alpha = 0;

        cg.transform.DOScale(1f, duration).SetEase(Ease.OutBack).SetUpdate(true).SetId(cg);
        cg.DOFade(1f, duration).SetEase(Ease.Linear).SetUpdate(true).SetId(cg);
    }

    private void HidePanel(CanvasGroup cg, bool deactivateAfter = true, float duration = 0.3f)
    {
        DOTween.Kill(cg); 

        cg.transform.DOScale(0.8f, duration).SetEase(Ease.InBack).SetUpdate(true).SetId(cg);
        cg.DOFade(0f, duration).SetEase(Ease.Linear).SetUpdate(true).SetId(cg).OnComplete(() =>
        {
            if (deactivateAfter) cg.gameObject.SetActive(false);
        });
    }

    public void GoHelp()
    {
        HidePanel(homeGroup);
        ShowPanel(helpGroup);
    }

    public void BackHome()
    {
        HidePanel(helpGroup);
        HidePanel(selectGroup);
        ShowPanel(homeGroup);
    }

    public void GoSelectLevel()
    {
        HidePanel(homeGroup);
        ShowPanel(selectGroup);
    }
}
