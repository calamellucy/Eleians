using UnityEngine;
using System.Linq;

public class EarthBumpSkill : MonoBehaviour
{
    public Player player;
    public ScanALot scans;
    public int bumpCount = 4;
    public int poolIndex = 6;

    [Header("Spawn Offset")]
    public float yOffset = -0.5f;   // 필요하면 Inspector에서 조정

    void Awake()
    {
        if (player == null)
            player = GetComponentInParent<Player>();

        if (scans == null)
            scans = GetComponentInParent<ScanALot>();
    }

    public void ActiveEarthBump()
    {
        if (scans == null || player == null)
            return;

        Transform[] targets = scans.GetNearest(bumpCount);

        foreach (Transform target in targets)
        {
            if (target == null) continue;

            GameObject bumpObj = GameManager.instance.pool.Get(poolIndex);

            // ★ 오프셋 적용된 위치
            Vector3 pos = target.position + new Vector3(0, yOffset, 0);
            bumpObj.transform.position = pos;

            // 방향 flip
            SpriteRenderer sr = bumpObj.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                bool isLeft = target.position.x < player.transform.position.x;
                sr.flipX = isLeft;
            }

            bumpObj.SetActive(true);
        }
    }
}
