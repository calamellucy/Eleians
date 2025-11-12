using System.Collections;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    public float damage = 10f;
    public float radius = 1.5f;
    public float duration = 0.3f;

    void OnEnable()
    {
        transform.localScale = Vector3.one * (radius * 2f);
        StartCoroutine(DisableAfter(duration));
    }

    IEnumerator DisableAfter(float t)
    {
        yield return new WaitForSeconds(t);
        gameObject.SetActive(false);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
