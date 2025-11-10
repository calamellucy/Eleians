using System.Collections;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    public float damage = 10f;
    public float radius = 1.5f;
    public float duration = 0.3f; // 스프라이트가 유지될 시간

    void OnEnable()
    {
        StartCoroutine(Explode());
    }

    IEnumerator Explode()
    {
        // 폭발 범위 내 적 탐색
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, LayerMask.GetMask("Enemy"));
        foreach (Collider2D hit in hits)
        {
            NormalMonster monster = hit.GetComponent<NormalMonster>();
            if (monster != null && monster.gameObject.activeSelf)
            {
                monster.health -= damage;
            }
        }

        // 지정된 시간 후 비활성화 (풀로 복귀)
        yield return new WaitForSeconds(duration);
        gameObject.SetActive(false);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
