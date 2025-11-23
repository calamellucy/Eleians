using UnityEngine;

public class LvUp : MonoBehaviour
{
    RectTransform rect;
    public Skill4 sk4;
    public FlameSword sk2;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    public void Show()
    {
        rect.localScale = Vector3.one;
        GameManager.instance.Stop();
    }

    public void Hide()
    {
        rect.localScale = Vector3.zero;
        GameManager.instance.Resume();
        sk4.GiveLevelSystemToSkill4();
        sk2.GiveLevelSystemToSkill2();
    }
}
