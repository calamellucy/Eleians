using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f;
    public float lifetime = 8f;

    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnEnable()
    {
        CancelInvoke();              // 혹시 이전 Invoke가 남아 있다면 취소
        Invoke(nameof(Disable), lifetime);
    }

    void OnDisable()
    {
        CancelInvoke();
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

    public void Fire(Vector2 dir)
    {
        // ������ ����ȭ�ؼ� �ӵ� ����
        rb.linearVelocity = dir.normalized * speed;
    }

    void Disable()
    {
        gameObject.SetActive(false);
    }
}
