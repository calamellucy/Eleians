using UnityEngine;
using System.Collections;

public class IceZone : MonoBehaviour
{
    public float damagePerSecond = 5f;
    public float healPerSecond = 10f;

    float duration = 5f;
    bool isRunning = false;

    bool playerInside = false;
    bool monsterInside = false;

    float healTimer = 1f;
    float damageTimer = 1f;

    void OnEnable()
    {
        healTimer = 1f;
        damageTimer = 1f;

        playerInside = false;
        monsterInside = false;

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

    void Update()
    {
        // 1) 플레이어 힐 처리
        if (playerInside)
        {
            healTimer -= Time.deltaTime;
            if (healTimer <= 0f)
            {
                playerCache.Heal(healPerSecond);
                Debug.Log("HEAL!!");
                healTimer = 1f;
            }
        }

        // 2) 몬스터 피해 처리
        if (monsterInside)
        {
            damageTimer -= Time.deltaTime;
            if (damageTimer <= 0f)
            {
                if (monsterCache != null && monsterCache.isLive)
                    monsterCache.ApplyDamageWithoutKonckback(damagePerSecond);

                Debug.Log("DAMAGE!!");
                damageTimer = 1f;
            }
        }
    }

    Player playerCache;
    MonsterBase monsterCache;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInside = true;
            playerCache = collision.GetComponent<Player>();
        }

        if (collision.CompareTag("Enemy"))
        {
            monsterInside = true;
            monsterCache = collision.GetComponent<MonsterBase>();
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            playerInside = false;

        if (collision.CompareTag("Enemy"))
            monsterInside = false;
    }
}
