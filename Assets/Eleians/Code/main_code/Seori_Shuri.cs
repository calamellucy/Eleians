using UnityEngine;

public class Seori_Shuri : MonoBehaviour
{
    public float damage;
    public int per;
    public float slowRate;

    public void Init(float damage, int per, float slowRate)
    {
        this.damage = damage;
        this.per = per;
        this.slowRate = slowRate;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Enemy 태그로 비교
        if (!collision.CompareTag("Enemy"))
            return;

        // Enemy → MonsterBase 가져와서 데미지 적용
        MonsterBase monster = collision.GetComponent<MonsterBase>();
        if (monster == null)
            return;

        // 데미지 적용
        monster.ApplyDamageWithoutKonckback(damage);

        // 슬로우 적용 (서리 스킬 특징)
   
        monster.ApplySlow(slowRate);

        // 관통(per) 처리
        
    }
}
