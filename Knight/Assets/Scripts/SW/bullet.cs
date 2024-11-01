using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed;  // 총알의 속도
    public float distance;  // Raycast 거리
    public LayerMask isLayer;  // Raycast 대상 레이어

    private Rigidbody2D rb;

    private Vector2 moveDirection;  // 총알의 이동 방향

    public void InitializeBullet(Vector2 dir)
    {
        moveDirection = dir;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        Invoke("DestroyBullet", 1);  // 1초 후에 총알 파괴

        // 캐릭터의 방향에 따라 총알의 방향을 설정
        moveDirection = transform.right * Mathf.Sign(transform.localScale.x);
        // 총알의 시각적 방향도 맞추기
        transform.localScale = new Vector3(Mathf.Sign(transform.localScale.x), 1, 0);
    }

    
    void Update()
    {
        RaycastHit2D ray = Physics2D.Raycast(transform.position, moveDirection, distance, isLayer);
        if (ray.collider != null)
        {
            if (ray.collider.tag == "Player")
            {
                Debug.Log("명중");
                DestroyBullet();
                // player 피격 로직 추가
                // 
            


            }
            // 데미지 받는 로직 추가


        }

        transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);
    }


    void DestroyBullet()
    {
        Destroy(gameObject);
    }
}
