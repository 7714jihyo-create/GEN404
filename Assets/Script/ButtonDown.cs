using Hospital404;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonDown : MonoBehaviour
{
    // 타이틀의 시작 버튼에서 새 진료 세션을 명시적으로 초기화합니다.
    public void StartButtonDown()
    {
        HospitalSession.Ensure().StartNewGame();
        SceneManager.LoadScene("GameScene");
    }

    // 기존 씬 버튼과의 호환성을 유지합니다. 실제 라운드 전환은 세션이 담당합니다.
    public void NextButtonDown()
    {
        HospitalSession.Ensure().ContinueAfterReport();
    }

    // WaitingScene의 기존 버튼에 연결돼 있던 메서드입니다.
    public void NextButtonDown2()
    {
        SceneManager.LoadScene("GameScene");
    }
}
