using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Hospital404
{
    /// <summary>
    /// GameScene에 자동 생성되는 진료실 인터페이스입니다.
    /// 기존 배경과 차트 버튼은 남겨 두고, 실제 진료 경험만 위에 덧씌웁니다.
    /// </summary>
    public sealed class HospitalGameUI : MonoBehaviour
    {
        public static HospitalGameUI Active { get; private set; }

        private HospitalSession session;
        private GameObject chartModal;
        private GameObject reportModal;
        private InputField diagnosisInput;
        private InputField prescriptionInput;
        private Text roundText;
        private Text totalScoreText;
        private Button submitButton;

        private void Awake()
        {
            Active = this;
        }

        private void Start()
        {
            session = HospitalSession.Ensure();
            if (session.AssignedPatients.Count == 0 || session.IsRunFinished)
            {
                session.StartNewGame();
            }

            BuildInterface();
            DisableLegacyPanel();
        }

        private void OnDestroy()
        {
            if (Active == this)
            {
                Active = null;
            }
        }

        public void OpenChart()
        {
            if (chartModal == null)
            {
                return;
            }

            RefreshChart();
            chartModal.SetActive(true);
        }

        private void BuildInterface()
        {
            HospitalUIFactory.DestroyIfPresent("Hospital404GameCanvas");
            Canvas canvas = HospitalUIFactory.CreateCanvas("Hospital404GameCanvas", 100);

            BuildTopBar(canvas.transform);
            BuildDiagnosisForm(canvas.transform);
            BuildChartModal(canvas.transform);
            BuildReportModal(canvas.transform);
        }

        private void BuildTopBar(Transform parent)
        {
            GameObject bar = HospitalUIFactory.CreatePanel(parent, "StatusBar", HospitalUIFactory.AccentDark,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(30f, -115f), new Vector2(-30f, -30f));

            roundText = HospitalUIFactory.CreateText(bar.transform, "RoundText", string.Empty, 32, Color.white, TextAnchor.MiddleLeft);
            HospitalUIFactory.SetRect(roundText.rectTransform, new Vector2(0f, 0f), new Vector2(0.56f, 1f), new Vector2(28f, 0f), new Vector2(-28f, 0f));

            totalScoreText = HospitalUIFactory.CreateText(bar.transform, "ScoreText", string.Empty, 32, new Color(1f, 0.88f, 0.45f), TextAnchor.MiddleRight);
            HospitalUIFactory.SetRect(totalScoreText.rectTransform, new Vector2(0.56f, 0f), new Vector2(1f, 1f), new Vector2(0f, 0f), new Vector2(-28f, 0f));

            RefreshTopBar();
        }

        private void BuildDiagnosisForm(Transform parent)
        {
            GameObject form = HospitalUIFactory.CreatePanel(parent, "DiagnosisForm", HospitalUIFactory.Paper,
                new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(-640f, 50f), new Vector2(-50f, -145f));

            Text title = HospitalUIFactory.CreateText(form.transform, "Title", "진단서 작성", 42, HospitalUIFactory.Ink, TextAnchor.MiddleCenter);
            HospitalUIFactory.SetRect(title.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(30f, -75f), new Vector2(-30f, 0f));

            Text patientHint = HospitalUIFactory.CreateText(form.transform, "PatientHint", "차트를 먼저 확인한 뒤 진단과 처방을 적어 주세요.", 21, HospitalUIFactory.AccentDark, TextAnchor.MiddleCenter);
            HospitalUIFactory.SetRect(patientHint.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(28f, -118f), new Vector2(-28f, -75f));

            Text diagnosisLabel = HospitalUIFactory.CreateText(form.transform, "DiagnosisLabel", "진단명", 24, HospitalUIFactory.Ink, TextAnchor.LowerLeft);
            HospitalUIFactory.SetRect(diagnosisLabel.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(38f, -175f), new Vector2(-38f, -125f));

            diagnosisInput = HospitalUIFactory.CreateInputField(form.transform, "DiagnosisInput", "예: 심야 감성 과다냉각증", false);
            HospitalUIFactory.SetRect(diagnosisInput.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(35f, -260f), new Vector2(-35f, -180f));

            Text prescriptionLabel = HospitalUIFactory.CreateText(form.transform, "PrescriptionLabel", "처방", 24, HospitalUIFactory.Ink, TextAnchor.LowerLeft);
            HospitalUIFactory.SetRect(prescriptionLabel.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(38f, -320f), new Vector2(-38f, -270f));

            prescriptionInput = HospitalUIFactory.CreateInputField(form.transform, "PrescriptionInput", "환자만의 황당한 증상을 고려한 처방을 자유롭게 적어 보세요.", true);
            HospitalUIFactory.SetRect(prescriptionInput.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(35f, -525f), new Vector2(-35f, -325f));

            submitButton = HospitalUIFactory.CreateButton(form.transform, "SubmitButton", "처방 제출 · 결과 보기", HospitalUIFactory.Accent);
            HospitalUIFactory.SetRect(submitButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 75f), new Vector2(450f, 82f));
            submitButton.onClick.AddListener(SubmitTreatment);

            Button chartButton = HospitalUIFactory.CreateButton(form.transform, "OpenChartButton", "환자 차트 열기", HospitalUIFactory.Warning);
            HospitalUIFactory.SetRect(chartButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 175f), new Vector2(450f, 60f));
            chartButton.onClick.AddListener(OpenChart);
        }

        private void BuildChartModal(Transform parent)
        {
            chartModal = HospitalUIFactory.CreatePanel(parent, "ChartModal", HospitalUIFactory.Overlay,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            GameObject card = HospitalUIFactory.CreatePanel(chartModal.transform, "ChartCard", HospitalUIFactory.Paper,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-610f, -380f), new Vector2(610f, 380f));

            Text title = HospitalUIFactory.CreateText(card.transform, "ChartTitle", "[ 기밀 ] 종합병원 404 환자 차트", 42, HospitalUIFactory.AccentDark, TextAnchor.MiddleCenter);
            HospitalUIFactory.SetRect(title.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(45f, -85f), new Vector2(-45f, -20f));

            Text chartBody = HospitalUIFactory.CreateText(card.transform, "ChartBody", string.Empty, 29, HospitalUIFactory.Ink, TextAnchor.UpperLeft);
            chartBody.name = "ChartBody";
            HospitalUIFactory.SetRect(chartBody.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(75f, 115f), new Vector2(-75f, -110f));

            Button closeButton = HospitalUIFactory.CreateButton(card.transform, "CloseChartButton", "차트 덮기", HospitalUIFactory.AccentDark);
            HospitalUIFactory.SetRect(closeButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 58f), new Vector2(330f, 65f));
            closeButton.onClick.AddListener(() => chartModal.SetActive(false));

            chartModal.SetActive(false);
        }

        private void BuildReportModal(Transform parent)
        {
            reportModal = HospitalUIFactory.CreatePanel(parent, "ReportModal", HospitalUIFactory.Overlay,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            GameObject card = HospitalUIFactory.CreatePanel(reportModal.transform, "ReportCard", HospitalUIFactory.Paper,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-670f, -420f), new Vector2(670f, 420f));

            Text title = HospitalUIFactory.CreateText(card.transform, "ReportTitle", "AI 치료 결과 보고서", 45, HospitalUIFactory.AccentDark, TextAnchor.MiddleCenter);
            HospitalUIFactory.SetRect(title.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(45f, -92f), new Vector2(-45f, -20f));

            Text reportBody = HospitalUIFactory.CreateText(card.transform, "ReportBody", string.Empty, 29, HospitalUIFactory.Ink, TextAnchor.UpperLeft);
            reportBody.name = "ReportBody";
            HospitalUIFactory.SetRect(reportBody.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(78f, 145f), new Vector2(-78f, -110f));

            Button nextButton = HospitalUIFactory.CreateButton(card.transform, "NextRoundButton", "다음 환자 호출", HospitalUIFactory.Accent);
            HospitalUIFactory.SetRect(nextButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 60f), new Vector2(400f, 70f));
            nextButton.onClick.AddListener(() => session.ContinueAfterReport());
            reportModal.SetActive(false);
        }

        private void SubmitTreatment()
        {
            if (reportModal.activeSelf)
            {
                return;
            }

            TreatmentResult result = session.SubmitDiagnosis(diagnosisInput.text, prescriptionInput.text);
            if (result == null)
            {
                return;
            }

            submitButton.interactable = false;
            StartCoroutine(HospitalAIReportClient.EnrichReport(result, ShowReport));
        }

        private void ShowReport(TreatmentResult result)
        {
            Text reportBody = reportModal.transform.Find("ReportCard/ReportBody").GetComponent<Text>();
            reportBody.text = string.Format("<b>{0}</b>\n\n{1}\n\n<b>진료 결과: {2}  |  획득 점수: +{3}점</b>",
                result.Patient.Name, result.Story, GetGradeLabel(result.Grade), result.Score);

            Button nextButton = reportModal.transform.Find("ReportCard/NextRoundButton").GetComponent<Button>();
            Text nextLabel = nextButton.GetComponentInChildren<Text>();
            nextLabel.text = session.CurrentRound + 1 >= HospitalSession.TotalRounds ? "최종 진료 결과 보기" : "다음 환자 호출";
            reportModal.SetActive(true);
            RefreshTopBar();
        }

        private void RefreshChart()
        {
            PatientData patient = session.CurrentPatient;
            if (patient == null)
            {
                return;
            }

            Text chartBody = chartModal.transform.Find("ChartCard/ChartBody").GetComponent<Text>();
            chartBody.text = string.Format(
                "<b>이름</b>  {0}\n<b>종족</b>  {1}\n<b>연령대</b>  {2}\n\n<b>추정 병명</b>  {3}\n\n<b>주요 증상</b>\n{4}\n\n<b>{5}</b>\n{6}",
                patient.Name, patient.Species, patient.AgeGroup, patient.Disease, patient.MainSymptom, patient.CautionLabel, patient.CautionDetail);
        }

        private void RefreshTopBar()
        {
            roundText.text = string.Format("진료실 404  |  ROUND {0} / {1}", session.CurrentRound + 1, HospitalSession.TotalRounds);
            totalScoreText.text = string.Format("현재 점수  {0}점", session.TotalScore);
        }

        private void DisableLegacyPanel()
        {
            GameObject legacyPanel = GameObject.Find("Panel");
            if (legacyPanel != null)
            {
                legacyPanel.SetActive(false);
            }
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

    /// <summary>
    /// GameScene이 열릴 때 HospitalGameUI를 자동으로 붙입니다.
    /// </summary>
    public static class HospitalGameSceneBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (SceneManager.GetActiveScene().name != "GameScene")
            {
                return;
            }

            if (Object.FindObjectOfType<HospitalGameUI>() == null)
            {
                new GameObject("Hospital404GameUI").AddComponent<HospitalGameUI>();
            }
        }
    }
}
