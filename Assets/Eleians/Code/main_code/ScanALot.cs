using UnityEngine;
using System.Linq;   // 정렬을 위해 필요

public class ScanALot : MonoBehaviour
{
    public float scanRange;
    public LayerMask targetLayer;
    public RaycastHit2D[] targets;

    public Transform[] nearestTargets; // 여러 개 저장

    void FixedUpdate()
    {
        targets = Physics2D.CircleCastAll(transform.position, scanRange, Vector2.zero, 0, targetLayer);
        nearestTargets = GetNearest(4);
    }

    public Transform[] GetNearest(int num)
    {
        if (targets == null || targets.Length == 0)
            return new Transform[0];

        Vector3 myPos = transform.position;

        // 거리 순 정렬
        var sorted = targets.OrderBy(t => Vector3.Distance(myPos, t.transform.position));

        return sorted.Take(num).Select(t => t.transform).ToArray();
    }
}
