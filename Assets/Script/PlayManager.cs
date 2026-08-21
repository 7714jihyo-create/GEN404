using Hospital404;
using UnityEngine;

public class PlayManager : MonoBehaviour
{
    [Header("기존 팝업 패널 (새 진료 UI가 없을 때의 예비 처리)")]
    public GameObject panel;

    private void Start()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    // 기존 Chart 버튼과 HowToPlay 버튼의 연결을 보존합니다.
    public void OpenPanel()
    {
        if (HospitalGameUI.Active != null)
        {
            HospitalGameUI.Active.OpenChart();
            return;
        }

        if (panel != null)
        {
            panel.SetActive(true);
        }
    }

    public void ClosePanel()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }
}
