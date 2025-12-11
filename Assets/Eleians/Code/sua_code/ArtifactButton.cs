using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class ArtifactButton : MonoBehaviour, IPointerEnterHandler
{
    public Image iconImage;
    public Text nameText;
    public Text descText;

    public ArtifactData data;

    public void SetData(ArtifactData artifact)
    {
        data = artifact;
        iconImage.sprite = artifact.icon;
        nameText.text = artifact.artifactName;
        descText.text = artifact.description;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        AudioManager.instance.PlaySfx(AudioManager.Sfx.mouse_on_button);
    }

    public void OnClick()
    {
        AudioManager.instance.PlaySfx(AudioManager.Sfx.click);
        ArtifactManager.instance.AcquireArtifact(data);
        GameManager.instance.uiSelectArt.Hide();
    }
}
