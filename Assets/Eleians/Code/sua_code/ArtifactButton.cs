using UnityEngine;
using UnityEngine.UI;

public class ArtifactButton : MonoBehaviour
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

    public void OnClick()
    {
        ArtifactManager.instance.AcquireArtifact(data);
        GameManager.instance.uiSelectArt.Hide();
    }
}
