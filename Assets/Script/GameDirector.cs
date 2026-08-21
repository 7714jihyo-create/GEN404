using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDirector : MonoBehaviour
{
    public GameObject Panel;

    // 게임 방법 버튼을 눌렀을 때 실행
    public void OpenHowToPlay()
    {
        Panel.SetActive(true);
    }

    // 닫기 버튼을 눌렀을 때 실행
    public void CloseHowToPlay()
    {
        Panel.SetActive(false);
    }
}
