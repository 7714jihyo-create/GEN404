using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Hospital404
{
    /// <summary>
    /// 선택적 원격 AI 보고서 연결입니다.
    /// StreamingAssets의 설정 파일에 HTTPS 서버 주소가 있을 때만 호출하며,
    /// 없거나 실패하면 즉시 로컬 결과 엔진의 서사를 유지합니다.
    /// 모델 API 키는 절대로 Unity 프로젝트나 설정 파일에 넣지 않아야 합니다.
    /// </summary>
    public static class HospitalAIReportClient
    {
        private const string ConfigFileName = "hospital404-ai-config.json";
        private const int DefaultTimeoutSeconds = 12;

        [Serializable]
        private class AIConfig
        {
            public string endpointUrl;
            public int timeoutSeconds = DefaultTimeoutSeconds;
        }

        [Serializable]
        private class StoryRequest
        {
            public string patientName;
            public string species;
            public string ageGroup;
            public string disease;
            public string symptom;
            public string caution;
            public string diagnosis;
            public string prescription;
            public string localOutcome;
            public int localScore;
            public string instruction;
        }

        [Serializable]
        private class StoryResponse
        {
            public string story;
        }

        public static IEnumerator EnrichReport(TreatmentResult result, Action<TreatmentResult> onCompleted)
        {
            AIConfig config = LoadConfig();
            if (config == null || string.IsNullOrWhiteSpace(config.endpointUrl))
            {
                onCompleted?.Invoke(result);
                yield break;
            }

            StoryRequest requestPayload = new StoryRequest
            {
                patientName = result.Patient.Name,
                species = result.Patient.Species,
                ageGroup = result.Patient.AgeGroup,
                disease = result.Patient.Disease,
                symptom = result.Patient.MainSymptom,
                caution = result.Patient.CautionLabel + ": " + result.Patient.CautionDetail,
                diagnosis = result.Diagnosis,
                prescription = result.Prescription,
                localOutcome = result.Grade.ToString(),
                localScore = result.Score,
                instruction = "플레이어의 처방을 첫 문장에 직접 인용해 주세요. 치료 결과를 포함한 한국어 메디컬 코미디 후일담을 정확히 4~5문장으로 작성하세요. 금기 사항을 지켰는지 반영하되 점수는 변경하지 마세요."
            };

            byte[] body = Encoding.UTF8.GetBytes(JsonUtility.ToJson(requestPayload));
            using (UnityWebRequest request = new UnityWebRequest(config.endpointUrl, UnityWebRequest.kHttpVerbPOST))
            {
                request.uploadHandler = new UploadHandlerRaw(body);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json; charset=utf-8");
                request.timeout = Mathf.Max(1, config.timeoutSeconds);
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    TryApplyStory(request.downloadHandler.text, result);
                }
            }

            onCompleted?.Invoke(result);
        }

        private static AIConfig LoadConfig()
        {
            try
            {
                string configPath = Path.Combine(Application.streamingAssetsPath, ConfigFileName);
                if (!File.Exists(configPath))
                {
                    return null;
                }

                string json = File.ReadAllText(configPath, Encoding.UTF8);
                return JsonUtility.FromJson<AIConfig>(json);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[Hospital404] AI 설정 파일을 읽지 못해 로컬 결과 엔진을 사용합니다. " + exception.Message);
                return null;
            }
        }

        private static void TryApplyStory(string responseJson, TreatmentResult result)
        {
            try
            {
                StoryResponse response = JsonUtility.FromJson<StoryResponse>(responseJson);
                if (response != null && !string.IsNullOrWhiteSpace(response.story))
                {
                    result.Story = response.story.Trim();
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[Hospital404] AI 응답 형식이 올바르지 않아 로컬 결과 엔진을 유지합니다. " + exception.Message);
            }
        }
    }
}
