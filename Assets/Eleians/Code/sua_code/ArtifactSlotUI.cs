using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArtifactSlotUI : MonoBehaviour
{
    public static ArtifactSlotUI instance;
    // 인스펙터에서 4개의 슬롯을 순서대로 연결하세요 (Slot 0 ~ Slot 3)
    public ArtifactSlot[] uiSlots;
    private int currentIndex = 0;

    

    void Awake()
    {
        instance = this;

        // ★ 시작할 때 모든 슬롯을 "빈 상태"로 초기화 (슬롯 자체는 꺼지지 않음)
        foreach (var slot in uiSlots)
        {
            slot.Clear();
        }
    }

    public void AddArtifact(ArtifactData data)
    {
        // 1. 4개 꽉 찼는지 확인
        if (currentIndex >= uiSlots.Length)
        {
            Debug.Log("아티팩트 슬롯이 가득 찼습니다.");
            return;
        }

        // 2. 현재 순서의 슬롯 가져오기
        ArtifactSlot slot = uiSlots[currentIndex];

        // 3. ★ 슬롯을 켜는 게 아니라(이미 켜져있음), 내용물을 채워넣음
        slot.Init(data);

        // 4. 인덱스 증가
        currentIndex++;
    }

    // ★ [추가] 특정 아티팩트의 스택 UI 갱신
    public void UpdateArtifactStack(ArtifactID id, int stack)
    {
        for (int i = 0; i < currentIndex; i++)
        {
            if (uiSlots[i].artifactID == id)
            {
                uiSlots[i].UpdateStackText(stack);
                break;
            }
        }
    }
}
