using System.Collections;
using UnityEngine;

public class Skill1 : MonoBehaviour
{
    [Header("Pooling / Prefab")]
    public int prefabId = 0;

    [Header("Fire Stats")]
    public float attackRate = 2f;
    public int projectileCount = 1;
    public float projectileSize = 0.2f;
    public float speed = 20f;
    public float lifetime = 5f;
    public float damage = 10f;
    public int per = 1;

    [Header("Element Level (Temp)")]
    public int electricCount = 0; // 전기 원소 수 (임시)

    float timer;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= 1f / attackRate)
        {
            TryFire();
            timer = 0f;
        }
    }

    void TryFire()
    {
        Transform target = FindNearestEnemy();
        if (target == null)
            return;

        int bulletCount = projectileCount;
        int penetration = per;

        // ⚡ 전기 진화 효과 (5 이상 시)
        if (electricCount >= 5)
        {
            bulletCount += 2;
            penetration += 1;
        }

        Vector3 dir = (target.position - transform.position).normalized;
        float baseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        for (int i = 0; i < bulletCount; i++)
        {
            float spread = (bulletCount > 1) ? (i - (bulletCount - 1) / 2f) * 8f : 0f;
            float angle = baseAngle + spread;
            Vector3 shootDir = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0);

            GameObject bulletObj = GameManager.instance.pool.Get(prefabId);
            bulletObj.transform.position = transform.position;
            bulletObj.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            bulletObj.transform.localScale = Vector3.one * projectileSize;

            BulletHoming bullet = bulletObj.GetComponent<BulletHoming>();
            if (bullet != null)
            {
                bullet.damage = damage;
                bullet.per = penetration;
                bullet.speed = speed;
                bullet.SetTarget(target);
            }

            BulletEvolution evo = bulletObj.GetComponent<BulletEvolution>();
            if (evo != null)
                evo.Setup(this);

            StartCoroutine(DisableAfter(bulletObj, lifetime));
        }
    }

    Transform FindNearestEnemy()
    {
        float nearestDist = float.MaxValue;
        Transform nearest = null;
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in enemies)
        {
            if (!enemy.activeSelf) continue;

            float dist = (enemy.transform.position - transform.position).sqrMagnitude;
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = enemy.transform;
            }
        }
        return nearest;
    }

    IEnumerator DisableAfter(GameObject go, float t)
    {
        yield return new WaitForSeconds(t);
        if (go == null || !go.activeSelf)
            yield break;  // 이미 비활성화된 경우 즉시 종료

        go.SetActive(false);
    }

}
