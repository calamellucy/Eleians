using UnityEngine;

public class SelectArtifact : MonoBehaviour
{
    public ArtifactButton[] buttons; // 버튼 3개 연결

    RectTransform rect;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    public void Show()
    {
        rect.localScale = Vector3.one;
        GameManager.instance.Stop();

        // 랜덤 아티팩트 3개 가져오기
        var randomList = ArtifactManager.instance.GetRandomArtifacts(3);

        // 버튼에 데이터 넣기
        for (int i = 0; i < buttons.Length; i++)
        {
            if (i < randomList.Count)
            {
                buttons[i].gameObject.SetActive(true);
                buttons[i].SetData(randomList[i]);
            }
            else
            {
                buttons[i].gameObject.SetActive(false);
            }
        }
    }

    public void Hide()
    {
        rect.localScale = Vector3.zero;
        GameManager.instance.Resume();
    }

}
