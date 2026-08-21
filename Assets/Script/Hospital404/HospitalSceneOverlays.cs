using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Hospital404
{
    public sealed class HospitalWaitingUI : MonoBehaviour
    {
        private void Start()
        {
            HospitalSession session = HospitalSession.Ensure();
            if (session.AssignedPatients.Count == 0 || session.IsRunFinished)
            {
                session.StartNewGame();
            }

            GameObject legacyNext = GameObject.Find("NextButton");
            if (legacyNext != null)
            {
                legacyNext.SetActive(false);
            }

            HospitalUIFactory.DestroyIfPresent("Hospital404WaitingCanvas");
            Canvas canvas = HospitalUIFactory.CreateCanvas("Hospital404WaitingCanvas", 100);
            GameObject overlay = HospitalUIFactory.CreatePanel(canvas.transform, "Overlay", HospitalUIFactory.Overlay, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            GameObject card = HospitalUIFactory.CreatePanel(overlay.transform, "CallCard", HospitalUIFactory.Paper,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-600f, -300f), new Vector2(600f, 300f));

            Text header = HospitalUIFactory.CreateText(card.transform, "Header", "진료 기록 저장 완료", 46, HospitalUIFactory.AccentDark, TextAnchor.MiddleCenter);
            HospitalUIFactory.SetRect(header.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(50f, -110f), new Vector2(-50f, -35f));

            PatientData nextPatient = session.CurrentPatient;
            string copy = string.Format("ROUND {0} / {1}\n\n다음 호출 환자: <b>{2}</b>\n종족: {3} · 연령대: {4}\n\n차트를 열어 보기 전에는 어떤 치료도 단정하지 마세요.\n여기는 종합병원 404입니다.",
                session.CurrentRound + 1, HospitalSession.TotalRounds, nextPatient.Name, nextPatient.Species, nextPatient.AgeGroup);
            Text body = HospitalUIFactory.CreateText(card.transform, "Body", copy, 31, HospitalUIFactory.Ink, TextAnchor.MiddleCenter);
            HospitalUIFactory.SetRect(body.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(80f, 115f), new Vector2(-80f, -150f));

            Button beginButton = HospitalUIFactory.CreateButton(card.transform, "BeginRoundButton", "진료실 입장", HospitalUIFactory.Accent);
            HospitalUIFactory.SetRect(beginButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 65f), new Vector2(385f, 78f));
            beginButton.onClick.AddListener(() => SceneManager.LoadScene("GameScene"));
        }
    }

    public sealed class HospitalFinalUI : MonoBehaviour
    {
        private void Start()
        {
            HospitalSession session = HospitalSession.Ensure();
            HospitalUIFactory.DestroyIfPresent("Hospital404FinalCanvas");
            Canvas canvas = HospitalUIFactory.CreateCanvas("Hospital404FinalCanvas", 100);
            GameObject overlay = HospitalUIFactory.CreatePanel(canvas.transform, "Overlay", HospitalUIFactory.Overlay, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            GameObject card = HospitalUIFactory.CreatePanel(overlay.transform, "FinalCard", HospitalUIFactory.Paper,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-720f, -455f), new Vector2(720f, 455f));

            Text title = HospitalUIFactory.CreateText(card.transform, "Title", "종합병원 404 · 최종 진료 결과", 46, HospitalUIFactory.AccentDark, TextAnchor.MiddleCenter);
            HospitalUIFactory.SetRect(title.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(50f, -100f), new Vector2(-50f, -25f));

            Text summary = HospitalUIFactory.CreateText(card.transform, "Summary", string.Format("<b>{0}점</b>  |  {1}", session.TotalScore, session.GetFinalGrade()), 36, HospitalUIFactory.Warning, TextAnchor.MiddleCenter);
            HospitalUIFactory.SetRect(summary.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(50f, -172f), new Vector2(-50f, -105f));

            Text record = HospitalUIFactory.CreateText(card.transform, "Record", BuildRecord(session), 27, HospitalUIFactory.Ink, TextAnchor.UpperLeft);
            HospitalUIFactory.SetRect(record.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(100f, 150f), new Vector2(-100f, -195f));

            Button restartButton = HospitalUIFactory.CreateButton(card.transform, "RestartButton", "새 진료 시작", HospitalUIFactory.Accent);
            HospitalUIFactory.SetRect(restartButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-230f, 65f), new Vector2(390f, 78f));
            restartButton.onClick.AddListener(() =>
            {
                session.StartNewGame();
                SceneManager.LoadScene("GameScene");
            });

            Button titleButton = HospitalUIFactory.CreateButton(card.transform, "TitleButton", "타이틀로", HospitalUIFactory.AccentDark);
            HospitalUIFactory.SetRect(titleButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(230f, 65f), new Vector2(280f, 78f));
            titleButton.onClick.AddListener(() => SceneManager.LoadScene("StartScene"));
        }

        private string BuildRecord(HospitalSession session)
        {
            if (session.Results.Count == 0)
            {
                return "완료된 진료가 없습니다. 다음 환자가 당신을 기다리고 있습니다.";
            }

            StringBuilder record = new StringBuilder("<b>오늘의 진료 기록</b>\n\n");
            for (int i = 0; i < session.Results.Count; i++)
            {
                TreatmentResult result = session.Results[i];
                record.AppendFormat("{0}. {1}  —  {2}  (+{3}점)\n", i + 1, result.Patient.Name, GetGradeLabel(result.Grade), result.Score);
            }
            record.Append("\n환자들의 황당함은 내일도 계속됩니다. 그래도 당신은 오늘 세 번의 진료를 끝까지 마쳤습니다.");
            return record.ToString();
        }

        private string GetGradeLabel(TreatmentGrade grade)
        {
            switch (grade)
            {
                case TreatmentGrade.Success:
                    return "치료 성공";
                case TreatmentGrade.PartialSuccess:
                    return "부분 성공";
                case TreatmentGrade.Failure:
                    return "치료 실패";
                default:
                    return "방치";
            }
        }
    }

    public static class HospitalOverlayBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            if (sceneName == "WaitingScene" && Object.FindObjectOfType<HospitalWaitingUI>() == null)
            {
                new GameObject("Hospital404WaitingUI").AddComponent<HospitalWaitingUI>();
            }
            else if (sceneName == "ResultScene" && Object.FindObjectOfType<HospitalFinalUI>() == null)
            {
                new GameObject("Hospital404FinalUI").AddComponent<HospitalFinalUI>();
            }
        }
    }
}
