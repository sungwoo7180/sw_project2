using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerMove : MonoBehaviour
{   
    public float maxSpeed;
    public float jumpPower;
    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;
    Animator anim;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    private float curTime;
    public float coolTime = 0.5f;    
    public Transform pos;
    public Vector2 boxSize;

    void Update()
    {   
        //jump
        if((Input.GetKey(KeyCode.W) && !anim.GetBool("isJump")) || (Input.GetKey(KeyCode.UpArrow) && !anim.GetBool("isJump"))){
            rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            anim.SetBool("isJump",true);
        }

        //stop
        if(Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)){
            rigid.velocity = new Vector2(rigid.velocity.normalized.x*0.5f, rigid.velocity.y);
            spriteRenderer.flipX = false;
        }
        //turn
        if(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)){
            spriteRenderer.flipX = true;
            
        }
        //animation
        if(Mathf.Abs(rigid.velocity.x) < 0.01)
            anim.SetBool("isWalk", false);
        else
            anim.SetBool("isWalk", true);

        // 공격 잽은 q 다른 펀치는 w + 충돌감지
        if(curTime <= 0){
            if((Input.GetKey(KeyCode.R) && !anim.GetBool("isJump")) || (Input.GetKey(KeyCode.J) && !anim.GetBool("isJump")))
            {
                Collider2D[] collider2Ds = Physics2D.OverlapBoxAll(pos.position, boxSize, 0);
                foreach (Collider2D collider in collider2Ds)
                {
                    Debug.Log(collider.tag);
                }
                anim.SetTrigger("isJab");
                curTime = coolTime;
            }

            if((Input.GetKey(KeyCode.T) && !anim.GetBool("isJump")) || (Input.GetKey(KeyCode.K) && !anim.GetBool("isJump")))
            {
                Collider2D[] collider2Ds = Physics2D.OverlapBoxAll(pos.position, boxSize, 0);
                foreach (Collider2D collider in collider2Ds)
                {
                    Debug.Log(collider.tag);
                }
                anim.SetTrigger("isPunch");
                curTime = coolTime;
            }

            if(Input.GetKey(KeyCode.F) || Input.GetKey(KeyCode.N))
            {
                Collider2D[] collider2Ds = Physics2D.OverlapBoxAll(pos.position, boxSize, 0);
                foreach (Collider2D collider in collider2Ds)
                {
                    Debug.Log(collider.tag);
                }
                anim.SetTrigger("isKick");
                curTime = coolTime;
            }
            
            if(Input.GetKey(KeyCode.G) || Input.GetKey(KeyCode.M))
            {
                Collider2D[] collider2Ds = Physics2D.OverlapBoxAll(pos.position, boxSize, 0);
                foreach (Collider2D collider in collider2Ds)
                {
                    Debug.Log(collider.tag);
                }
                anim.SetTrigger("isJumpKick");
                curTime = coolTime;
            }
        }
        else
        {
            curTime -= Time.deltaTime;
        }
    }

    private void onDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(pos.position, boxSize);
    }

    // Update is called once per frame
    void FixedUpdate()
    {   
        //move
        float h = Input.GetAxisRaw("Horizontal");
        Vector2 targetVelocity = new Vector2(h * maxSpeed, rigid.velocity.y);
        Vector2 velocityChange = targetVelocity - rigid.velocity;

        rigid.AddForce(velocityChange, ForceMode2D.Impulse);
        if(rigid.velocity.x > maxSpeed)
            rigid.velocity = new Vector2(maxSpeed, rigid.velocity.y);
        else if(rigid.velocity.x < -maxSpeed)
            rigid.velocity = new Vector2(-maxSpeed, rigid.velocity.y);
        
        Debug.Log("rigid" + rigid.velocity.y);
        //Landing platform
        if(rigid.velocity.y < 0){
            Debug.DrawRay(rigid.position, Vector3.down, new Color(0, 1, 0));
            Debug.Log("before");    
            RaycastHit2D rayHit = Physics2D.Raycast(rigid.position, Vector3.down, 1, LayerMask.GetMask("PlatForm"));
            if(rayHit.collider != null){
                Debug.Log("hit");
                Debug.Log(rayHit.distance);
                if(rayHit.distance < 0.5f){
                    anim.SetBool("isJump", false);
                }
            }
            Debug.Log("End");
        }
    }

//     void OnCollisonEnter2D(collision2D collision)
//     {
//         if(collision.gameObject.tag == "Enemy") {
//             OnDamaged();
//         }
//     }

//     void OnDamaged(Vector2 targetPos)
//     {
//         gameObject.layer = 11;

//         spriteRenderer.color = new Color(1, 1, 1, 0.4f);

//         int dirc = transform.position.x-targetPos.x > 0 ? 1 : -1;
//         rigid.AddForce(new Vector2(dirc,1) ,ForceMode2D.Impulse);

//         Invoke("OffDamaged", 3);
//     }

//     void OffDamaged()
//     {
//         gameObject.layer = 10;
//         spriteRenderer.color = new Color(1,1,1,1);
//     }
}
