using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public PoolManager pool;
    public int bulletIndex = 0;
    public float attackRate = 2f;
    public int projectileCount = 1;
    public float projectileSize = 0.2f;

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
        dir.z = 10f;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        for (int i = 0; i < projectileCount; i++)
        {
            GameObject bulletObj = pool.Get(bulletIndex);
            bulletObj.transform.position = transform.position;
            bulletObj.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            bulletObj.transform.localScale = Vector3.one * projectileSize;

            // 여기 핵심! GameObject → Bullet 컴포넌트 가져오기
            Bullet bullet = bulletObj.GetComponent<Bullet>();
            bullet.Fire(dir);
        }
    }
}
