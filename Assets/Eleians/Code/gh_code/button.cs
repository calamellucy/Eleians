using UnityEngine;
using UnityEngine.EventSystems; // 1. 마우스 감지를 위해 필수!

// 2. IPointerEnterHandler 인터페이스 추가
public class button : MonoBehaviour, IPointerEnterHandler
{
    public int type;

    // 마우스가 버튼 위에 올라갈 때 실행 (호버 사운드)
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 아까 만든 AudioManager를 한 줄로 호출!
        // Enum 이름(mouse_on_button)이 정확한지 확인해줘
        AudioManager.instance.PlaySfx(AudioManager.Sfx.mouse_on_button);
    }

    // 버튼을 클릭할 때 실행
    public void OnClick()
    {
        // 클릭 사운드 호출
        // Enum 이름(click)이 정확한지 확인해줘
        AudioManager.instance.PlaySfx(AudioManager.Sfx.click);

        switch (type)
        {
            case 0:
                StatsManager.instance.ElectricCnt++;
                break;
            case 1:
                StatsManager.instance.FireCnt++;
                break;
            case 2:
                StatsManager.instance.IceCnt++;
                break;
            case 3:
                StatsManager.instance.EarthCnt++;
                break;
        }

        StatsManager.instance.RecalculateStats();
    }
}