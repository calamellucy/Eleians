using System.Threading;
using UnityEngine;

public class DamageReceiver : MonoBehaviour
{
    MonsterBase monster;
    private void Awake()
    {
        monster = GetComponent<MonsterBase>();   
    }

    protected void OnTriggerEnter2D(Collider2D collision)
    {
        if (!monster.isLive) return;

        if (collision.CompareTag("Explosion"))
        {
            var exp = collision.GetComponent<Explosion>();
            if (exp != null) monster.ApplyDamage(exp.damage, 1);
            return;
        }

        if (collision.CompareTag("Bullet"))
        {
            var b = collision.GetComponent<Bullet>();
            if (b != null)
            {
                monster.ApplyDamage(b.damage, 1);
                b.per--;
                if (b.per < 0) b.gameObject.SetActive(false);
            }
        }

        if (collision.CompareTag("dust"))
        {
            float baseDamage = (StatsManager.instance.Attack + (StatsManager.instance.EarthCnt * 2)) * 0.3f;
            monster.ApplyDamage(baseDamage, 4);
        }

        if (collision.CompareTag("Bump"))
        {
            float baseDamage = (StatsManager.instance.Attack + (StatsManager.instance.EarthCnt * 8)) * 2f;
            monster.ApplyDamage(baseDamage, 4);
        }

        if (collision.CompareTag("Jeonjapa"))
        {
            var br = collision.GetComponent<Bullet_Re>();
            if (br != null) monster.ApplyDamage(br.damage, 1);
        }

        if (collision.CompareTag("Seori"))
        {
            Seori_Shuri seo = collision.GetComponent<Seori_Shuri>();
            if (seo == null) return;

            monster.ApplyDamage(seo.damage, 3);
            monster.ApplySlow(seo.slowRate);
        }

        if (collision.CompareTag("dhwyy"))
        {
            BlizzardArea dhw = collision.GetComponent<BlizzardArea>();
            if (dhw == null) return;

            monster.ApplyDamage(dhw.baseDamage, 3);
            monster.ApplySlow(dhw.slowRate);
        }
    }
}
