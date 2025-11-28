using UnityEngine;

public class ShieldReflect : MonoBehaviour
{
    public float reflectDamage = 0f;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            MonsterBase mob = collision.GetComponent<MonsterBase>();
            if (mob != null)
            {
                mob.ApplyDamageWithoutKonckback(reflectDamage);
            }
        }
    }
}
