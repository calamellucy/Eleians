using UnityEngine;
using System.Collections;

public class IceZone : MonoBehaviour
{
    public float damagePerSecond = 5f;
    public float healPerSecond = 10f;

    float duration = 5f;
    bool isRunning = false;

    void OnEnable()
    {
        if (!isRunning)
            StartCoroutine(ZoneLife());
    }

    IEnumerator ZoneLife()
    {
        isRunning = true;
        yield return new WaitForSeconds(duration);
        gameObject.SetActive(false);
        isRunning = false;
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        // 몬스터 피해
        if (collision.CompareTag("Enemy"))
        {
            MonsterBase mob = collision.GetComponent<MonsterBase>();
            if (mob != null)
                mob.ApplyDamage(damagePerSecond * Time.deltaTime);
        }

        // 플레이어 회복
        if (collision.CompareTag("Player"))
        {
            Player player = collision.GetComponent<Player>();
            if (player != null)
                player.Heal(healPerSecond * Time.deltaTime);
        }
    }
}
