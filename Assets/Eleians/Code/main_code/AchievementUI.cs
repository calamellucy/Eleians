using UnityEngine;

public class AchievementUI : MonoBehaviour
{
    // 수동으로 만들어둔 슬롯 16개를 여기에 다 넣을 거야
    public AchievementSlot[] slots;

    // 창이 켜질 때마다(SetActive true 될 때마다) 실행
    void OnEnable()
    {
        UpdateAllSlots();
    }

    public void UpdateAllSlots()
    {
        // 모든 슬롯을 돌면서 상태 갱신 명령
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
            {
                slots[i].UpdateSlotState();
            }
        }
    }
}