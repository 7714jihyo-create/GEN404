using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Hospital404
{
    [Serializable]
    public class PatientData
    {
        public string Id;
        public string Name;
        public string Species;
        public string AgeGroup;
        public string Disease;
        public string MainSymptom;
        public string CautionLabel;
        public string CautionDetail;
        public string StoryHook;
        public List<string> HelpfulKeywords;
        public List<string> RiskKeywords;

        public PatientData(
            string id,
            string name,
            string species,
            string ageGroup,
            string disease,
            string mainSymptom,
            string cautionLabel,
            string cautionDetail,
            string storyHook,
            params string[] helpfulKeywords)
        {
            Id = id;
            Name = name;
            Species = species;
            AgeGroup = ageGroup;
            Disease = disease;
            MainSymptom = mainSymptom;
            CautionLabel = cautionLabel;
            CautionDetail = cautionDetail;
            StoryHook = storyHook;
            HelpfulKeywords = new List<string>(helpfulKeywords);
            RiskKeywords = new List<string>();
        }
    }

    [Serializable]
    public class TreatmentResult
    {
        public PatientData Patient;
        public string Diagnosis;
        public string Prescription;
        public TreatmentGrade Grade;
        public int Score;
        public string Story;
    }

    public enum TreatmentGrade
    {
        Untreated,
        Failure,
        PartialSuccess,
        Success
    }

    /// <summary>
    /// 씬이 바뀌어도 유지되는 종합병원 404의 단일 게임 상태입니다.
    /// API나 저장 파일 없이도 3라운드 게임이 완결되도록 설계했습니다.
    /// </summary>
    public sealed class HospitalSession : MonoBehaviour
    {
        public const int TotalRounds = 3;
        public static HospitalSession Instance { get; private set; }

        public int CurrentRound { get; private set; }
        public IReadOnlyList<PatientData> AssignedPatients => assignedPatients;
        public IReadOnlyList<TreatmentResult> Results => results;
        public PatientData CurrentPatient => HasActivePatient ? assignedPatients[CurrentRound] : null;
        public int TotalScore => results.Sum(result => result.Score);
        public bool HasActivePatient => CurrentRound >= 0 && CurrentRound < assignedPatients.Count;
        public bool IsRunFinished => results.Count >= TotalRounds;

        private readonly List<PatientData> assignedPatients = new List<PatientData>();
        private readonly List<TreatmentResult> results = new List<TreatmentResult>();
        private readonly List<PatientData> patientPool = new List<PatientData>();
        private System.Random random;

        public static HospitalSession Ensure()
        {
            if (Instance != null)
            {
                return Instance;
            }

            GameObject sessionObject = new GameObject("Hospital404Session");
            return sessionObject.AddComponent<HospitalSession>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            random = new System.Random();
            BuildPatientPool();
        }

        public void StartNewGame()
        {
            if (patientPool.Count == 0)
            {
                BuildPatientPool();
            }

            assignedPatients.Clear();
            results.Clear();
            CurrentRound = 0;

            foreach (PatientData patient in patientPool.OrderBy(_ => random.Next()).Take(TotalRounds))
            {
                assignedPatients.Add(patient);
            }
        }

        public TreatmentResult SubmitDiagnosis(string diagnosis, string prescription)
        {
            if (!HasActivePatient)
            {
                return null;
            }

            if (results.Count > CurrentRound)
            {
                return results[CurrentRound];
            }

            TreatmentResult result = DiagnosisResultBuilder.Build(CurrentPatient, diagnosis, prescription);
            results.Add(result);
            return result;
        }

        public void ContinueAfterReport()
        {
            if (results.Count <= CurrentRound)
            {
                return;
            }

            CurrentRound++;
            SceneManager.LoadScene(IsRunFinished ? "ResultScene" : "WaitingScene");
        }

        public string GetFinalGrade()
        {
            if (TotalScore >= 270)
            {
                return "전설의 기적 의사";
            }
            if (TotalScore >= 180)
            {
                return "수상하지만 유능한 전문의";
            }
            if (TotalScore >= 90)
            {
                return "야간 당직 생존자";
            }
            return "병원 404의 새로운 미스터리";
        }

        private void BuildPatientPool()
        {
            patientPool.Clear();

            PatientData refrigerator = new PatientData(
                "fridge-poet", "시인 냉장고 프로스트", "사물", "성체", "심야 감성 과다냉각증",
                "매일 새벽 2시가 되면 냉기를 뿜으며 사랑 시를 40분간 낭송합니다.",
                "종족 특약", "문학상 추천서와 얼음틀을 동시에 제공하면 냉각 기능이 과열될 수 있습니다.",
                "새벽 2시의 시 낭송은 병동 전체의 야식을 얼려 버릴 뻔했습니다.",
                "절전", "타이머", "수면", "온도", "제습");
            refrigerator.RiskKeywords.AddRange(new[] { "전자레인지", "불", "가열", "드라이기" });
            patientPool.Add(refrigerator);

            PatientData dog = new PatientData(
                "dog-weight", "몽실", "강아지", "성체", "체중 폭증 짖음증",
                "한 번 짖을 때마다 체중이 정확히 5kg씩 늘어나 소파가 구조 요청을 보냅니다.",
                "알레르기", "뼈다귀 모양 보조제에 알레르기가 있어 간식 처방은 금기입니다.",
                "몽실의 짖음은 체중계의 경보음과 완벽한 2중주를 이룹니다.",
                "훈련", "산책", "조용", "호흡", "음소거");
            dog.RiskKeywords.AddRange(new[] { "간식", "뼈다귀", "치킨", "사료" });
            patientPool.Add(dog);

            PatientData microwave = new PatientData(
                "microwave-rebel", "말썽이", "전자레인지", "유아기", "보호자 지시 거부열",
                "엄마의 말을 들을 때마다 삐- 소리를 내며 회전판을 멈추고 방으로 들어가 버립니다.",
                "금기 사항", "금쪽같은 내새끼 출연 예약과 5분 이상 혼내기는 반항 전력을 2배로 올립니다.",
                "말썽이는 식은 밥만 골라 데우며 독립 선언문을 출력했습니다.",
                "대화", "약속", "보상", "타이머", "가족상담");
            microwave.RiskKeywords.AddRange(new[] { "금쪽", "오은영", "출연", "혼내", "벌" });
            patientPool.Add(microwave);

            PatientData router = new PatientData(
                "router-rude", "와이파이 공유기 공유", "사물", "노년기", "비밀번호 욕설성 접속장애",
                "비밀번호를 물어보면 신호 세기를 0칸으로 낮추고 상상 이상으로 거친 욕설을 송출합니다.",
                "종족 특약", "전원 코드를 갑자기 뽑으면 기억 속 비밀번호가 전부 초기화됩니다.",
                "공유는 복도 끝까지 신호를 뿌리며 '비번은 예의로 묻는 것'이라 외쳤습니다.",
                "비밀번호", "재설정", "상담", "매너", "업데이트");
            router.RiskKeywords.AddRange(new[] { "전원", "뽑", "망치", "물", "초기화" });
            patientPool.Add(router);

            PatientData robot = new PatientData(
                "robot-dust", "먼지 박사 R-2", "로봇", "성체", "청소 강박 역주행증",
                "먼지를 발견하면 청소기를 밀고 반대 방향으로 도망가며 '먼지도 삶이 있다'고 주장합니다.",
                "금기 사항", "자석 치료를 하면 내부 나사가 춤을 춰 복도에 로봇 군단을 만들 수 있습니다.",
                "먼지 박사 R-2는 먼지 한 톨에게 퇴원 축하 파티를 열어 주었습니다.",
                "필터", "지도", "충전", "정리", "청소" );
            robot.RiskKeywords.AddRange(new[] { "자석", "물", "망치", "분해" });
            patientPool.Add(robot);

            PatientData alien = new PatientData(
                "alien-laugh", "루루-404", "외계인", "노년기", "웃음 역행 시간재채기",
                "웃을 때마다 어제의 재채기가 오늘 병실에 도착해 간호사들의 달력을 뒤엎습니다.",
                "알레르기", "지구산 민트 향에 노출되면 시간대가 두 칸 뒤로 밀립니다.",
                "루루-404의 재채기 한 번에 접수대의 월요일이 두 번 반복되었습니다.",
                "호흡", "달력", "시간", "휴식", "차" );
            alien.RiskKeywords.AddRange(new[] { "민트", "타임머신", "폭발", "향수" });
            patientPool.Add(alien);
        }
    }

    public static class DiagnosisResultBuilder
    {
        public static TreatmentResult Build(PatientData patient, string diagnosis, string prescription)
        {
            string cleanDiagnosis = (diagnosis ?? string.Empty).Trim();
            string cleanPrescription = (prescription ?? string.Empty).Trim();
            TreatmentGrade grade = DetermineGrade(patient, cleanPrescription);
            int score = GetScore(grade, cleanDiagnosis);

            return new TreatmentResult
            {
                Patient = patient,
                Diagnosis = cleanDiagnosis,
                Prescription = cleanPrescription,
                Grade = grade,
                Score = score,
                Story = BuildStory(patient, cleanDiagnosis, cleanPrescription, grade, score)
            };
        }

        private static TreatmentGrade DetermineGrade(PatientData patient, string prescription)
        {
            if (string.IsNullOrWhiteSpace(prescription))
            {
                return TreatmentGrade.Untreated;
            }

            string lowerPrescription = prescription.ToLowerInvariant();
            if (patient.RiskKeywords.Any(keyword => lowerPrescription.Contains(keyword.ToLowerInvariant())))
            {
                return TreatmentGrade.Failure;
            }

            int usefulKeywordCount = patient.HelpfulKeywords.Count(keyword => lowerPrescription.Contains(keyword.ToLowerInvariant()));
            if (usefulKeywordCount >= 2)
            {
                return TreatmentGrade.Success;
            }
            return usefulKeywordCount == 1 ? TreatmentGrade.PartialSuccess : TreatmentGrade.Failure;
        }

        private static int GetScore(TreatmentGrade grade, string diagnosis)
        {
            int baseScore;
            switch (grade)
            {
                case TreatmentGrade.Success:
                    baseScore = 100;
                    break;
                case TreatmentGrade.PartialSuccess:
                    baseScore = 60;
                    break;
                case TreatmentGrade.Failure:
                    baseScore = 20;
                    break;
                default:
                    baseScore = 0;
                    break;
            }

            return baseScore + (string.IsNullOrWhiteSpace(diagnosis) ? 0 : 10);
        }

        private static string BuildStory(PatientData patient, string diagnosis, string prescription, TreatmentGrade grade, int score)
        {
            string quotedPrescription = string.IsNullOrWhiteSpace(prescription)
                ? "아무 처방도 남기지 않음"
                : "\"" + prescription + "\"";
            string diagnosisNote = string.IsNullOrWhiteSpace(diagnosis)
                ? "진단명은 공란으로 남아 있었지만"
                : "의사가 적은 '" + diagnosis + "' 진단을 바탕으로";

            switch (grade)
            {
                case TreatmentGrade.Success:
                    return string.Format(
                        "{0} 환자에게 {1} 처방이 전달되었습니다. {2} 병동은 조심스럽게 치료를 시작했습니다. {3} 이후 증상이 눈에 띄게 잦아들어, 대기실의 다른 환자들까지 박수를 보냈습니다. 예상 밖의 부작용은 없었고 오늘의 당직 일지에는 '기적에 가까운 진료'라고 기록되었습니다. 치료 성공! +{4}점.",
                        patient.Name, quotedPrescription, diagnosisNote, patient.StoryHook, score);

                case TreatmentGrade.PartialSuccess:
                    return string.Format(
                        "{0} 환자에게 {1} 처방이 전달되었습니다. {2} {3} 처방의 절반은 훌륭하게 작동했지만, 나머지 절반은 환자 특유의 고집 앞에서 길을 잃었습니다. 그래도 병동의 혼란은 조금 줄었고 환자는 다음 진료를 약속했습니다. 부분 성공! +{4}점.",
                        patient.Name, quotedPrescription, diagnosisNote, patient.StoryHook, score);

                case TreatmentGrade.Failure:
                    return string.Format(
                        "{0} 환자에게 {1} 처방이 전달되었습니다. {2} 그러나 차트의 '{3}: {4}'를 놓친 탓에 치료는 예상과 전혀 다른 방향으로 흘렀습니다. {5} 간호사들은 침착하게 상황을 수습했지만 환자는 더욱 당당해졌습니다. 치료 실패! +{6}점.",
                        patient.Name, quotedPrescription, diagnosisNote, patient.CautionLabel, patient.CautionDetail, patient.StoryHook, score);

                default:
                    return string.Format(
                        "{0}의 진료 차트는 한참 동안 열린 채로 남아 있었습니다. 처방전이 비어 있는 것을 본 환자는 '스스로 회복하겠다'며 병실의 규칙을 새로 쓰기 시작했습니다. {1} 결국 대기실 전체가 환자의 즉흥 치료법에 휘말렸습니다. 오늘의 교훈은 간단합니다. 이상한 환자일수록 먼저 말을 걸어야 합니다. 방치됨. +{2}점.",
                        patient.Name, patient.StoryHook, score);
            }
        }
    }
}
