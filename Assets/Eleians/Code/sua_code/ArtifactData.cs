using UnityEditor.Experimental;
using UnityEngine;

[CreateAssetMenu(fileName = "ArtifactData", menuName = "Scriptable Objects/ArtifactData")]
public class ArtifactData : ScriptableObject
{
    public ArtifactID id;
    public string artifactName;
    [TextArea] public string description;
    public Sprite icon;
}
