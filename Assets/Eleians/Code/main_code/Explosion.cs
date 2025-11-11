using System.Collections;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    public float damage = 10f;
    public float radius = 1.5f;
    public float duration = 0.3f;

    static int enemyLayerMask = -1;

    void OnEnable()
    {
        if (enemyLayerMask == -1)
            enemyLayerMask = LayerMask.GetMask("Enemy");

        StartCoroutine(Explode());
    }

    IEnumerator Explode()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, enemyLayerMask);
        foreach (Collider2D hit in hits)
        {
            NormalMonster monster = hit.GetComponent<NormalMonster>();
            if (monster != null && monster.gameObject.activeSelf)
                monster.health -= damage;
        }

        yield return new WaitForSeconds(duration);
        gameObject.SetActive(false);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
