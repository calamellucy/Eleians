using System.Collections;
using UnityEngine;

public class Chest : MonoBehaviour
{
    Animator anim;
    bool opened = false;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (opened) return;

        if (collision.collider.CompareTag("Player"))
        {
            opened = true;
            anim.SetTrigger("open");
            StartCoroutine(WaitAndShowUI());
        }
    }

    IEnumerator WaitAndShowUI()
    {
        // 현재 재생되는 클립 길이 가져오기
        AnimatorClipInfo[] info = anim.GetCurrentAnimatorClipInfo(0);
        float clipLength = info[0].clip.length;

        yield return new WaitForSeconds(clipLength + 0.8f);

        GameManager.instance.uiSelectArt.Show();
        Destroy(gameObject);
    }
}
