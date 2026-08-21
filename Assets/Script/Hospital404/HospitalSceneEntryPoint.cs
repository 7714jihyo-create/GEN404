using UnityEngine;
using UnityEngine.SceneManagement;

namespace Hospital404
{
    /// <summary>
    /// WaitingScene과 ResultScene에 직접 배치되는 씬 시작 제어기입니다.
    /// 정적 초기화 누락 여부와 무관하게 각 화면의 실제 UI를 생성합니다.
    /// </summary>
    public sealed class HospitalSceneEntryPoint : MonoBehaviour
    {
        private void Start()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            if (sceneName == "WaitingScene" && FindObjectOfType<HospitalWaitingUI>() == null)
            {
                new GameObject("Hospital404WaitingUI").AddComponent<HospitalWaitingUI>();
            }
            else if (sceneName == "ResultScene" && FindObjectOfType<HospitalFinalUI>() == null)
            {
                new GameObject("Hospital404FinalUI").AddComponent<HospitalFinalUI>();
            }
        }
    }
}
