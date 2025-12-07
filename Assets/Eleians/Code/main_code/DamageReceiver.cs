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

        // --- [불] 속성 공격들 ---

        // 1. [NEW] 화염 검 (FlameSword)
        if (collision.CompareTag("FireSlash"))
        {
            FlameSword sword = collision.GetComponent<FlameSword>();
            if (sword != null)
            {
                // 화염 검 데미지와 불 속성(도트뎀) 적용
                monster.ApplyDamage(sword.damage, ElementType.Fire);
            }
            return;
        }

        // 2. 화염 투사체 (FlameSword에서 나가는 검기)
        if (collision.CompareTag("FireSlashShots"))
        {
            FireSlashShots bu = collision.GetComponent<FireSlashShots>();
            if (bu != null)
                monster.ApplyDamage(bu.damage, ElementType.Fire);
            return;
        }

        // 3. 화염 폭발
        if (collision.CompareTag("FireExplosion"))
        {
            FireExplosion fireExp = collision.GetComponent<FireExplosion>();
            if (fireExp != null)
            {
                monster.ApplyDamage(fireExp.finalDamage, ElementType.Fire);
            }
            return;
        }


        // --- [얼음] ---
        if (collision.CompareTag("Seori"))
        {
            Seori_Shuri seo = collision.GetComponent<Seori_Shuri>();
            if (seo != null){
                AudioManager.instance.PlaySfx(AudioManager.Sfx.Ice);
                monster.ApplyDamage(seo.damage, ElementType.Ice);
            }
            return;
        }

        if (collision.CompareTag("dhwyy"))
        {
            BlizzardArea dhw = collision.GetComponent<BlizzardArea>();
            if (dhw != null)
            {
                monster.ApplyDamage(dhw.baseDamage, ElementType.Ice);
                // ★ [추가] 업적 매니저에게 "나 한 놈 쳤어!"라고 알림
                AchievementManager.instance.OnIceChargeHit();
            }
            return;
        }


        // --- [전기] ---
        if (collision.CompareTag("Explosion"))
        {
            var exp = collision.GetComponent<Explosion>();
            if (exp != null)
                monster.ApplyDamage(exp.damage, ElementType.Lightning);
            return;
        }

        if (collision.CompareTag("Jeonjapa"))
        {
            var br = collision.GetComponent<Bullet_Re>();
            if (br != null)
                monster.ApplyDamage(br.damage, ElementType.Lightning);
            return;
        }


        // --- [흙] ---
        if (collision.CompareTag("Bullet"))
        {
            var b = collision.GetComponent<Bullet>();
            if (b != null)
            {
                monster.ApplyDamage(b.damage, ElementType.Earth);
                b.OnHit();
            }
            return;
        }

        if (collision.CompareTag("dust"))
        {
            float baseDamage = (StatsManager.instance.Attack + (StatsManager.instance.EarthCnt * 2)) * 0.3f;
            monster.ApplyDamage(baseDamage, ElementType.Earth);
            return;
        }

        if (collision.CompareTag("Bump"))
        {
            float baseDamage = (StatsManager.instance.Attack + (StatsManager.instance.EarthCnt * 8)) * 2f;
            monster.ApplyDamage(baseDamage, ElementType.Earth);
            return;
        }
    }
}