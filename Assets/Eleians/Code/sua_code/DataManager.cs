using System;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance; // 어디서든 접근 가능하게 함

    // 도전과제 데이터 (예: ID와 해금 여부)
    // 딕셔너리나 리스트로 관리. 여기선 간단히 배열로 예시
    public bool[] achievementUnlocks = new bool[16];

    void Awake()
    {
        // 싱글톤 패턴: 단 하나만 존재해야 함
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ★ 씬이 넘어가도 파괴되지 않음! ★
            LoadData(); // 게임 켜질 때 저장된 거 불러오기
        }
        else
        {
            Destroy(gameObject); // 이미 있으면 난 사라진다
        }
    }

    // 도전과제 달성 시 호출할 함수
    public void UnlockAchievement(int id)
    {
        // 들어온 ID는 1, 2, 3... 형식이므로 배열 인덱스(0, 1, 2...)로 변환
        int index = id - 1;

        // 예외 처리 (범위 벗어나면 무시)
        if (index < 0 || index >= achievementUnlocks.Length)
        {
            Debug.LogError($"[DataManager] 잘못된 업적 ID입니다: {id}");
            return;
        }

        // 아직 안 깬 업적이라면?
        if (!achievementUnlocks[index])
        {
            // 1. 데이터 저장
            achievementUnlocks[index] = true;
            SaveData();
            Debug.Log($"도전과제 {id} (Index: {index}) 달성 및 저장 완료!");

            // 2. ★★★ AchievementManager에게 알림 띄우라고 명령 ★★★
            if (AchievementManager.instance != null)
            {
                AchievementManager.instance.ShowNotification(index);
            }
        }
    }

    // 저장 (PlayerPrefs 사용 - 간단함)
    void SaveData()
    {
        for (int i = 0; i < achievementUnlocks.Length; i++)
        {
            // bool을 0과 1로 저장 (참이면 1, 거짓이면 0)
            PlayerPrefs.SetInt($"Achiev_{i}", achievementUnlocks[i] ? 1 : 0);
        }
        PlayerPrefs.Save();
    }

    // 불러오기
    void LoadData()
    {
        for (int i = 0; i < achievementUnlocks.Length; i++)
        {
            int value = PlayerPrefs.GetInt($"Achiev_{i}", 0);
            achievementUnlocks[i] = (value == 1);
        }
    }

    [ContextMenu("초기화 (Reset Data)")]
    public void ClearAllData()
    {
        // 1. 하드디스크(PlayerPrefs)에서 삭제
        for (int i = 0; i < achievementUnlocks.Length; i++)
        {
            // 주의: 저장할 때 썼던 키 이름("Achiev_")이랑 똑같아야 지워집니다!
            PlayerPrefs.DeleteKey($"Achiev_{i}");

            // 2. 현재 메모리(변수)에서도 false로 초기화 (게임 껐다 킬 필요 없이 바로 반영되게)
            achievementUnlocks[i] = false;
        }

        // 변경사항 저장
        PlayerPrefs.Save();

        Debug.Log("★ 모든 도전과제 데이터가 초기화되었습니다! ★");

        // (선택사항) 만약 UI가 켜져 있다면 UI도 즉시 갱신해주고 싶을 때
        // FindObjectOfType<AchievementUI>()?.UpdateAllSlots();
    }
}