using System.Collections;
using UnityEngine;

public class Skill1 : MonoBehaviour
{
    // public PoolManager pool;
    public int prefabId = 0;
    public float attackRate = 2f;
    public int projectileCount = 1;
    public float projectileSize = 0.2f;

    public float speed = 20f;   // BulletWj의 speed 대신
    public float lifetime = 8f; // BulletWj의 lifetime 대신
    public float damage = 10f;  // BulletGh용 damage
    public int per = 1;         // BulletGh용 관통 수

    float timer;

    void Update()
    {
        timer += Time.deltaTime;

        // 자동 발사
        if (timer >= 1f / attackRate)
        {
            Fire();
            timer = 0f;
        }
    }

    void Fire()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 dir = mousePos - transform.position;

        Vector2 fireDir = dir.normalized;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        for (int i = 0; i < projectileCount; i++)
        {
            // GameObject bulletObj = pool.Get(prefabId);
            GameObject bulletObj = GameManager.instance.pool.Get(prefabId);
            bulletObj.transform.position = transform.position;
            bulletObj.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            bulletObj.transform.localScale = Vector3.one * projectileSize;

            Bullet bullet = bulletObj.GetComponent<Bullet>();
            if (bullet != null)
                bullet.Init(damage, per, fireDir * speed);

            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.linearVelocity = fireDir * speed;

            // 일정 시간 뒤 비활성화 (기존 OnEnable → Invoke 구조를 대체)
            bulletObj.SetActive(true);
            bulletObj.GetComponent<MonoBehaviour>().StartCoroutine(DisableAfter(bulletObj, lifetime));

            /*
            BulletWj bullet = bulletObj.GetComponent<BulletWj>();
            bullet.Fire(dir);
            */
        }
    }

    IEnumerator DisableAfter(GameObject go, float t)
    {
        yield return new WaitForSeconds(t);
        if (go) go.SetActive(false);
    }
}
