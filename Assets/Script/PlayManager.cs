using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayManager : MonoBehaviour
{
    [Header("노출시킬 팝업 패널")]
    public GameObject panel; // Inspector에서 Panel 오브젝트를 드래그 앤 드롭 연결

    void Start()
    {
        // 게임 시작 시 패널이 열려있다면 자동으로 닫아둡니다.
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    // HowToPlay 버튼 클릭 시 실행
    public void OpenPanel()
    {
        if (panel != null)
        {
            panel.SetActive(true);
        }
    }

    // CloseButton 클릭 시 실행
    public void ClosePanel()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }
}
