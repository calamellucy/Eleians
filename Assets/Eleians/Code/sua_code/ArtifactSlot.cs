using UnityEngine;
using UnityEngine.UI;

public class ArtifactSlot : MonoBehaviour
{
    public Image icon;
    public Text stackText; // ★ [추가] 스택 표시용 텍스트 (UI에서 연결)

    public ArtifactID artifactID; // ★ [추가] 이 슬롯이 어떤 아티팩트인지 알아야 함

    // ★ [추가] 시작할 때 빈 슬롯으로 만드는 함수
    public void Clear()
    {
        if (icon != null) icon.gameObject.SetActive(false); // (O) 오브젝트 자체를 끕니다.
        if (stackText != null)
        {
            stackText.text = ""; // 내용도 비우고
            stackText.gameObject.SetActive(false); // ★ 꺼버리기
        }
        artifactID = (ArtifactID)(-1); // ID 초기화 (선택사항)
    }

    // 초기화 (아이콘 설정)
    public void Init(ArtifactData data)
    {
        if (data == null) return;

        Debug.Log($"[UI] {data.artifactName}의 Init 함수 실행됨. Icon enabled: {icon != null}");

        // 1. 아이콘 켜기
        if (icon != null)
        {
            icon.sprite = data.icon;
            icon.gameObject.SetActive(true);
        }
        this.artifactID = data.id;

        // 2. 스택 텍스트 설정
        if (stackText != null)
        {
            if (data.id == ArtifactID.OverloadCrystal || data.id == ArtifactID.EarthResonance)
            {
                stackText.text = "0"; // 스택형은 0 표시
                stackText.gameObject.SetActive(true);
            }
            else
            {
                stackText.text = "";  // 나머지는 빈칸
                stackText.gameObject.SetActive(false);
            }
        }
    }

    // ★ [추가] 외부에서 스택 업데이트 호출
    public void UpdateStackText(int stack)
    {
        if (stackText == null) return;
        // 혹시 모르니 업데이트할 때도 켜져 있는지 확인
        if (!stackText.gameObject.activeSelf) stackText.gameObject.SetActive(true);
        stackText.text = stack.ToString();
    }
}