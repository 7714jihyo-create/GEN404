using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Hospital404
{
    /// <summary>
    /// 기존 GameScene의 클립보드(Chart), 모니터(Monitor), 다음 버튼을 중심으로 동작하는 진료실 제어기입니다.
    /// 이 스크립트는 GameScene의 InfoManager에 직접 연결됩니다.
    /// </summary>
    public sealed class HospitalGameUI : MonoBehaviour
    {
        public static HospitalGameUI Active { get; private set; }

        private HospitalSession session;
        private GameObject legacyChartPanel;
        private Text legacyChartText;
        private GameObject diagnosisModal;
        private GameObject reportModal;
        private InputField diagnosisInput;
        private InputField prescriptionInput;
        private Button submitButton;

        private void Awake()
        {
            Active = this;
        }

        private void Start()
        {
            session = HospitalSession.Ensure();
            legacyChartPanel = GameObject.Find("Panel");
            if (legacyChartPanel != null)
            {
                EnsureLegacyChartText();
                legacyChartPanel.SetActive(false);
            }

            BuildOverlayInterface();
            WireExistingRoomControls();
        }

        private void OnDestroy()
        {
            if (Active == this)
            {
                Active = null;
            }
        }

        /// <summary>기존 진료실의 클립보드 버튼이 호출합니다.</summary>
        public void OpenChart()
        {
            if (legacyChartPanel == null || legacyChartText == null)
            {
                return;
            }

            RefreshLegacyChart();
            legacyChartPanel.SetActive(true);
        }

        /// <summary>기존 진료실의 모니터 버튼이 호출합니다.</summary>
        public void OpenDiagnosis()
        {
            if (diagnosisModal == null)
            {
                return;
            }

            diagnosisModal.SetActive(true);
            diagnosisInput.Select();
        }

        private void EnsureLegacyChartText()
        {
            Transform existing = legacyChartPanel.transform.Find("Hospital404ChartText");
            if (existing != null)
            {
                legacyChartText = existing.GetComponent<Text>();
                return;
            }

            legacyChartText = HospitalUIFactory.CreateText(legacyChartPanel.transform, "Hospital404ChartText", string.Empty, 25, HospitalUIFactory.Ink, TextAnchor.UpperLeft);
            HospitalUIFactory.Stretch(legacyChartText.rectTransform, new Vector2(48f, 105f), new Vector2(-48f, -105f));
            legacyChartText.verticalOverflow = VerticalWrapMode.Overflow;
        }

        private void BuildOverlayInterface()
        {
            HospitalUIFactory.DestroyIfPresent("Hospital404OverlayCanvas");
            Canvas canvas = HospitalUIFactory.CreateCanvas("Hospital404OverlayCanvas", 100);
            BuildDiagnosisModal(canvas.transform);
            BuildReportModal(canvas.transform);
        }

        private void BuildDiagnosisModal(Transform parent)
        {
            diagnosisModal = HospitalUIFactory.CreatePanel(parent, "DiagnosisModal", HospitalUIFactory.Overlay,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            GameObject card = HospitalUIFactory.CreatePanel(diagnosisModal.transform, "DiagnosisCard", HospitalUIFactory.Paper,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-640f, -430f), new Vector2(640f, 430f));

            Text title = HospitalUIFactory.CreateText(card.transform, "Title", "진단서 작성", 46, HospitalUIFactory.AccentDark, TextAnchor.MiddleCenter);
            HospitalUIFactory.SetRect(title.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(50f, -95f), new Vector2(-50f, -20f));

            Text patient = HospitalUIFactory.CreateText(card.transform, "Patient", GetPatientHeading(), 28, HospitalUIFactory.Warning, TextAnchor.MiddleCenter);
            HospitalUIFactory.SetRect(patient.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(50f, -145f), new Vector2(-50f, -90f));

            Text diagnosisLabel = HospitalUIFactory.CreateText(card.transform, "DiagnosisLabel", "진단명", 25, HospitalUIFactory.Ink, TextAnchor.LowerLeft);
            HospitalUIFactory.SetRect(diagnosisLabel.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(65f, -205f), new Vector2(-65f, -150f));

            diagnosisInput = HospitalUIFactory.CreateInputField(card.transform, "DiagnosisInput", "예: 심야 감성 과다냉각증", false);
            HospitalUIFactory.SetRect(diagnosisInput.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(60f, -288f), new Vector2(-60f, -212f));

            Text prescriptionLabel = HospitalUIFactory.CreateText(card.transform, "PrescriptionLabel", "처방", 25, HospitalUIFactory.Ink, TextAnchor.LowerLeft);
            HospitalUIFactory.SetRect(prescriptionLabel.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(65f, -345f), new Vector2(-65f, -294f));

            prescriptionInput = HospitalUIFactory.CreateInputField(card.transform, "PrescriptionInput", "환자의 황당한 증상과 차트의 금기 사항을 고려해 자유롭게 처방해 보세요.", true);
            HospitalUIFactory.SetRect(prescriptionInput.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(60f, -545f), new Vector2(-60f, -350f));

            submitButton = HospitalUIFactory.CreateButton(card.transform, "SubmitButton", "처방 제출 · 치료 결과 보기", HospitalUIFactory.Accent);
            HospitalUIFactory.SetRect(submitButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-230f, 70f), new Vector2(430f, 78f));
            submitButton.onClick.AddListener(SubmitTreatment);

            Button closeButton = HospitalUIFactory.CreateButton(card.transform, "CloseButton", "닫기", HospitalUIFactory.AccentDark);
            HospitalUIFactory.SetRect(closeButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(255f, 70f), new Vector2(190f, 78f));
            closeButton.onClick.AddListener(() => diagnosisModal.SetActive(false));
            diagnosisModal.SetActive(false);
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
            HospitalUIFactory.SetRect(reportBody.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(78f, 145f), new Vector2(-78f, -110f));

            Button nextButton = HospitalUIFactory.CreateButton(card.transform, "NextRoundButton", "다음 환자 호출", HospitalUIFactory.Accent);
            HospitalUIFactory.SetRect(nextButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 60f), new Vector2(400f, 70f));
            nextButton.onClick.AddListener(() => session.ContinueAfterReport());
            reportModal.SetActive(false);
        }

        private void WireExistingRoomControls()
        {
            GameObject monitorObject = GameObject.Find("Monitor");
            if (monitorObject == null)
            {
                return;
            }

            Button monitorButton = monitorObject.GetComponent<Button>();
            if (monitorButton != null)
            {
                monitorButton.onClick.RemoveListener(OpenDiagnosis);
                monitorButton.onClick.AddListener(OpenDiagnosis);
            }
        }

        private void SubmitTreatment()
        {
            TreatmentResult result = session.SubmitDiagnosis(diagnosisInput.text, prescriptionInput.text);
            if (result == null)
            {
                return;
            }

            submitButton.interactable = false;
            diagnosisModal.SetActive(false);
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
        }

        private void RefreshLegacyChart()
        {
            PatientData patient = session.CurrentPatient;
            if (patient == null)
            {
                return;
            }

            legacyChartText.text = string.Format(
                "<b>종합병원 404 · 환자 차트</b>\n\n<b>이름</b>  {0}\n<b>종족</b>  {1}\n<b>연령대</b>  {2}\n\n<b>앓고 있는 병명</b>\n{3}\n\n<b>주요 증상</b>\n{4}\n\n<b>{5}</b>\n{6}",
                patient.Name, patient.Species, patient.AgeGroup, patient.Disease, patient.MainSymptom, patient.CautionLabel, patient.CautionDetail);
        }

        private string GetPatientHeading()
        {
            PatientData patient = session == null ? null : session.CurrentPatient;
            return patient == null ? "환자를 호출하는 중입니다." : patient.Name + " · " + patient.Species + " 환자";
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
