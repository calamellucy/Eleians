using UnityEngine;

public class EarthBumpCodeForP : MonoBehaviour
{
    private Collider2D coll;

    void Awake()
    {
        coll = GetComponent<Collider2D>();

        if (coll != null)
            coll.enabled = false;
    }

    public void GetTag()
    {
        gameObject.tag = "Bump";

        if (coll != null)
            coll.enabled = true;
    }

    public void Dead()
    {
        gameObject.tag = "Untagged";

        if (coll != null)
            coll.enabled = false;

        gameObject.SetActive(false);
    }
}
