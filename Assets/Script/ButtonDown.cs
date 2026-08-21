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

    // 기존 진료실의 다음 버튼은 진료가 끝났으면 다음 환자를, 미제출 상태면 방치 결과를 기록한 뒤 대기실로 보냅니다.
    public void NextButtonDown()
    {
        HospitalSession.Ensure().SkipCurrentPatientAndContinue();
    }

    // WaitingScene의 기존 버튼에 연결돼 있던 메서드입니다.
    public void NextButtonDown2()
    {
        SceneManager.LoadScene("GameScene");
    }
}
