using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed;  // �Ѿ��� �ӵ�
    public float distance;  // Raycast �Ÿ�
    public LayerMask isLayer;  // Raycast ��� ���̾�

    private Rigidbody2D rb;

    private Vector2 moveDirection;  // �Ѿ��� �̵� ����

    public void InitializeBullet(Vector2 dir)
    {
        moveDirection = dir;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        Invoke("DestroyBullet", 1);  // 1�� �Ŀ� �Ѿ� �ı�

        // ĳ������ ���⿡ ���� �Ѿ��� ������ ����
        moveDirection = transform.right * Mathf.Sign(transform.localScale.x);
        // �Ѿ��� �ð��� ���⵵ ���߱�
        transform.localScale = new Vector3(Mathf.Sign(transform.localScale.x), 1, 0);
    }

    
    void Update()
    {
        RaycastHit2D ray = Physics2D.Raycast(transform.position, moveDirection, distance, isLayer);
        if (ray.collider != null)
        {
            if (ray.collider.tag == "Player")
            {
                Debug.Log("����");
                DestroyBullet();
                // player �ǰ� ���� �߰�
                // 
            


            }
            // ������ �޴� ���� �߰�


        }

        transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);
    }


    void DestroyBullet()
    {
        Destroy(gameObject);
    }
}
