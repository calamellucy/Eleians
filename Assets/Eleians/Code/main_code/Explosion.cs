using System.Collections;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    public float damage = 10f;
    public float radius = 1f;
    public float duration = 0.3f;

    void OnEnable()
    {
        // Skill1_Re에서 현재 전자파(스킬1)의 대미지를 가져옴
        Skill1_Re skill = GameManager.instance.player.GetComponentInChildren<Skill1_Re>();
        if (skill != null)
        {
            damage = skill.damage * 0.25f; // 전자파 스킬 대미지의 25%
        }

        // 폭발 반경에 따라 시각적 크기 조정
        //transform.localScale = Vector3.one * (radius * 2f);

        // 일정 시간 뒤 자동 비활성화
        StartCoroutine(DisableAfter(duration));
    }

    IEnumerator DisableAfter(float t)
    {
        yield return new WaitForSeconds(t);
        gameObject.SetActive(false);
    }

    //void OnDrawGizmosSelected()
    //{
    //    Gizmos.color = Color.yellow;
    //    Gizmos.DrawWireSphere(transform.position, radius);
    //}
}
