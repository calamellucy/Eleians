using UnityEngine;

public class StoneDust : MonoBehaviour
{
    [Header("Stone Dust Explosion")]
    public int explosionPrefabId = 5;
    public float forwardOffset = 0.3f; 
    public float clockwiseRotationOffset = 170f;
    public Vector3 Scale = Vector3.one;


    public void SpawnExplosion(Vector3 hitPos, Vector2 dir)
    {
        if (GameManager.instance == null || GameManager.instance.pool == null)
            return;

        GameObject exp = GameManager.instance.pool.Get(explosionPrefabId);
        if (!exp) return;

        Transform t = exp.transform;

        Vector3 pos = hitPos;
        if (dir.sqrMagnitude > 0.0001f)
        {
            Vector3 forward = (Vector3)dir.normalized;
            pos += forward * forwardOffset;
        }

        t.position = pos;
        t.localScale = Scale * (1f + StatsManager.instance.EarthCnt * 0.08f);


        float baseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        float finalAngle = baseAngle - clockwiseRotationOffset;

        t.rotation = Quaternion.Euler(0, 0, finalAngle);
        exp.SetActive(true);
    }
    void delete()
    {
        gameObject.SetActive(false);
    }
}
