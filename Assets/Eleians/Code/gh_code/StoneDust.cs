using UnityEngine;

// 이 스크립트는 꼭 [Player] 오브젝트에 붙어 있어야 해!
public class StoneDust : MonoBehaviour
{
    [Header("Stone Dust Explosion")]
    public int explosionPrefabId = 5;
    public float forwardOffset = 0.3f;
    public Vector3 Scale = Vector3.one;

    public void SpawnExplosion(Vector3 hitPos, Vector2 dir)
    {
        if (GameManager.instance == null || GameManager.instance.pool == null)
            return;

        GameObject exp = GameManager.instance.pool.Get(explosionPrefabId);
        if (!exp) return;

        Transform t = exp.transform;

        // 위치 설정 (충돌 지점에서 살짝 앞)
        Vector3 pos = hitPos;
        if (dir.sqrMagnitude > 0.0001f)
        {
            Vector3 forward = (Vector3)dir.normalized;
            pos += forward * forwardOffset;
        }

        t.position = pos;
        t.localScale = Scale * (1f + StatsManager.instance.EarthCnt * 0.08f) * 0.7f;

        // 각도 설정 (탄알 진행 방향 기준)
        float Angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        t.rotation = Quaternion.Euler(0, 0, Angle);
        AudioManager.instance.PlaySfx(AudioManager.Sfx.stoneDust);

        exp.SetActive(true);
    }
}