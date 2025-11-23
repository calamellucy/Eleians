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
            if (exp != null) monster.ApplyDamage(exp.damage);
            return;
        }

        if (collision.CompareTag("Bullet"))
        {
            var b = collision.GetComponent<Bullet>();
            if (b != null)
            {
                monster.ApplyDamage(b.damage);
                b.per--;
                if (b.per < 0) b.gameObject.SetActive(false);
            }
        }

        if (collision.CompareTag("dust"))
        {
            float baseDamage = (StatsManager.instance.Attack + (StatsManager.instance.EarthCnt * 2)) * 0.3f;
            monster.ApplyDamage(baseDamage);
        }

        if (collision.CompareTag("Bump"))
        {
            float baseDamage = (StatsManager.instance.Attack + (StatsManager.instance.EarthCnt * 8)) * 2f;
            monster.ApplyDamage(baseDamage);
        }

        if (collision.CompareTag("Jeonjapa"))
        {
            var br = collision.GetComponent<Bullet_Re>();
            if (br != null) monster.ApplyDamage(br.damage);
        }

        if (collision.CompareTag("Seori"))
        {
            Seori_Shuri seo = collision.GetComponent<Seori_Shuri>();
            if (seo == null) return;

            monster.ApplyDamage(seo.damage);
            monster.ApplySlow(seo.slowRate);
        }

        if (collision.CompareTag("dhwyy"))
        {
            BlizzardArea dhw = collision.GetComponent<BlizzardArea>();
            if (dhw == null) return;

            monster.ApplyDamage(dhw.baseDamage);
            monster.ApplySlow(dhw.slowRate);
        }

        if (collision.CompareTag("FireSlashShots"))
        {
            FireSlashShots bu = collision.GetComponent<FireSlashShots>();
            monster.ApplyDamage(bu.damage);
            return;
        }
    }
}
