using Mono.Cecil;
using UnityEngine;

public class NormalMonsterSu : MonoBehaviour
{
    public float speed;
    public Rigidbody2D target;

    bool isLive = true;

    Rigidbody2D rigid;
    SpriteRenderer spriter;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriter = GetComponent<SpriteRenderer>();
    }

    private void FixedUpdate()
    {
        if (!isLive)
            return;

        Vector2 dirVec = target.position - rigid.position;
        Vector2 nextVec = dirVec.normalized * speed * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + nextVec);
        rigid.linearVelocity = Vector2.zero;
    }
    private void LateUpdate()
    {
        if (!isLive)
            return;

        spriter.flipX = target.position.x > rigid.position.x;
    }

    private void OnEnable()
    {
        // target = GameManager.instance.Circle.GetComponent<Rigidbody2D>();
        target = GameManagerSu.instance.player.GetComponent<Rigidbody2D>();
    }

}
