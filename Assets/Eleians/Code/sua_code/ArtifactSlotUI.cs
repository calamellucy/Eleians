using UnityEngine;
using UnityEngine.UI;

public class ArtifactSlotUI : MonoBehaviour
{
    public static ArtifactSlotUI instance;

    public Image[] slots; // Slot æ»¿« Icon ¿ÃπÃ¡ˆ
    private int currentIndex = 0;

    void Awake()
    {
        instance = this;
    }

    public void AddArtifactIcon(Sprite icon)
    {
        if (currentIndex >= slots.Length)
        {
            Debug.Log("æ∆∆º∆—∆Æ ΩΩ∑‘¿Ã ¿ÃπÃ ∞°µÊ √°Ω¿¥œ¥Ÿ.");
            return;
        }

        slots[currentIndex].sprite = icon;
        slots[currentIndex].gameObject.SetActive(true);
        currentIndex++;
    }
}
