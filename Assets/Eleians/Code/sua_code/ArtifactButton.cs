using UnityEngine;

public class ArtifactButton : MonoBehaviour
{
    public int artifactID;

    public void OnClickArtifact()
    {
        ArtifactManager.instance.Apply(artifactID);
        GameManager.instance.uiSelectArt.Hide();
    }
}
