using System.Collections;
using UnityEngine;

public class Skill1 : MonoBehaviour
{
    public int prefabId = 0;
    public float attackRate = 2f;
    public int projectileCount = 1;
    public float projectileSize = 0.2f;

    public float speed = 20f;
    public float lifetime = 5f;
    public float damage = 10f;
    public int per = 1;

    // ⚡ 임시 전기 원소 개수 (나중에 GameManager 등에서 가져오게 변경 예정)
    public int electricCount = 0;

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
        Debug.Log($"[Skill1] Fire! time={Time.time:F2}");

        Transform target = FindNearestEnemy();
        if (target == null)
            return;

        //  전기 진화 효과 적용
        int bulletCount = projectileCount;
        int penetration = per;

        if (electricCount >= 5)
        {
            bulletCount += 2;
            penetration += 1;
        }

        Vector3 dir = (target.position - transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        for (int i = 0; i < bulletCount; i++)
        {
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
                bullet.GetComponent<BulletEvolution>().Setup(this); // ⚡ 진화 관리 스크립트 추가
            }

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

            float dist = Vector3.SqrMagnitude(enemy.transform.position - transform.position);
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
        if (go != null)
            go.SetActive(false);
    }
}
