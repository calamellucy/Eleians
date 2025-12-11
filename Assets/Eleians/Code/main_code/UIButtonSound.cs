using UnityEngine;
using UnityEngine.EventSystems; // ★ UI 이벤트를 쓰려면 이게 꼭 필요해!

// 이 스크립트를 버튼 오브젝트(GameStart, Achievement, Exit)에 각각 붙여주면 돼.
public class UIButtonSound : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    // 마우스가 버튼 위로 올라갔을 때 실행 (Hover)
    public void OnPointerEnter(PointerEventData eventData)
    {
        // AudioManager의 SFX 중 'Hover' 혹은 'Select' 재생
        // (네가 설정한 Enum 이름에 맞춰서 바꿔줘!)
        AudioManager.instance.PlaySfx(AudioManager.Sfx.mouse_on_button);
    }

    // 버튼을 클릭했을 때 실행 (Click)
    public void OnPointerClick(PointerEventData eventData)
    {
        // AudioManager의 SFX 중 'Click' 재생
        AudioManager.instance.PlaySfx(AudioManager.Sfx.click);
    }
}