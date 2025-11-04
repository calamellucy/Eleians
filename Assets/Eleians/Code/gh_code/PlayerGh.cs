using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGh : MonoBehaviour 
{ 
    public Vector2 inputVec; 
    public float speed; 
    Rigidbody2D rigid; 
    SpriteRenderer spriter; 
    public bool IsFacingRight { get; private set; } = true; 
    public Vector2 MoveDir => inputVec; 
    void Awake() 
    { 
        rigid = GetComponent<Rigidbody2D>(); 
        spriter = GetComponent<SpriteRenderer>(); 
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
    void LateUpdate() { 
        if (inputVec.x != 0) { 
            spriter.flipX = inputVec.x > 0; 
        } 
    } 
}