using UnityEngine;

public class Scanner : MonoBehaviour
{
    public float scanRange = 10f;
    public string targetTag = "Enemy"; // 찾을 태그
    public Transform nearestTarget;

    void FixedUpdate()
    {
        ScanByTag();
    }

    void ScanByTag()
    {
        Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, scanRange);

        Transform result = null;
        float minDist = Mathf.Infinity;

        foreach (Collider2D target in targets)
        {
            // 태그 비교
            if (!target.CompareTag(targetTag))
                continue;

            float dist = Vector2.Distance(transform.position, target.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                result = target.transform;
            }
        }

        nearestTarget = result;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, scanRange);
    }
}
