using UnityEngine;

public class DustEffect : MonoBehaviour
{
    private PolygonCollider2D polyCol;

    private void Awake()
    {
        polyCol = GetComponent<PolygonCollider2D>();
        if (polyCol != null)    polyCol.enabled = false;
    }

    public void EnableCollider()
    {
        if (polyCol != null)
            polyCol.enabled = true;
    }

    public void delete()
    {
        gameObject.SetActive(false);
    }
}