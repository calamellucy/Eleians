using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public Vector2 inputVec;
    public float speed;
    public Scanner scanner;
    public ScanALot scans;

    public bool IsFacingRight { get; private set; } = true;
    public Vector2 MoveDir => inputVec;

    Rigidbody2D rigid;
    SpriteRenderer spriter;
    Animator anim;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriter = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        scanner = GetComponent<Scanner>();
        scans = GetComponent<ScanALot>();
    }

    void Update()
    {
        inputVec.x = Input.GetAxisRaw("Horizontal");
        inputVec.y = Input.GetAxisRaw("Vertical");
    }

    void FixedUpdate()
    {
        Vector2 nextVec = inputVec.normalized * speed * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + nextVec);
    }

    void LateUpdate()
    {
        anim.SetFloat("Speed", inputVec.magnitude);

        if (inputVec.x != 0) {
            spriter.flipX = inputVec.x > 0;
        }
    }

    /*
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!GameManager.instance.isLive)
            return;

        GameManager.instance.health -= Time.deltaTime * 10;

        if (GameManager.instance.health < 0)
        {
            // anim.SetTrigger("Dead");
        }
    }
    */

    public void ApplyDamage(float dmg)
    {
        if (!GameManager.instance.isLive) return;

        GameManager.instance.health -= dmg;

        if (GameManager.instance.health <= 0)
        {
            Die();
        }

        // hit �ִ�, �����ð�, ���� �߰� ����
    }

    public void Heal(float amount)
    {
        if (!GameManager.instance.isLive) return;

        // 절대값 보장
        amount = Mathf.Abs(amount);

        GameManager.instance.health += amount;

        // 최대 체력 초과 방지
        GameManager.instance.health =
            Mathf.Clamp(GameManager.instance.health, 0f, GameManager.instance.maxHealth);

        // 힐 이펙트, 힐 텍스트 같은 것 원하면 여기에 추가하면 된다
    }


    void Die()
    {
        GameManager.instance.isLive = false;
        // anim.SetTrigger("Dead");
        // rigid.simulated = false;
    }
}
