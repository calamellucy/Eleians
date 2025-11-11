using UnityEngine;

public class TowerMonster : NormalMonster
{
    // NormalMonster의 OnEnable() 대신 Tower를 타겟으로 지정
    protected override void OnEnable()
    {
        base.OnEnable(); // NormalMonster의 초기화 과정 유지

        // target을 Tower로 변경
        if (GameManager.instance.tower != null)
            target = GameManager.instance.tower.GetComponent<Rigidbody2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isLive) return;

        if (collision.CompareTag("Tower"))
        {
            Tower tower = collision.GetComponent<Tower>();
            if (tower != null)
            {
                Debug.Log("거점 타격 성공");
                tower.TakeDamage(10); // 데미지 값은 조정 가능
                isLive = false;
                coll.enabled = false;
                rigid.simulated = false;
                anim.SetBool("dead", true);
                GameManager.instance.kill++;
            }
        }
    }
}
