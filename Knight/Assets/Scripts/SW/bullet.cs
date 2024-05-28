using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed;  // �Ѿ��� �ӵ�
    public float distance;  // Raycast �Ÿ�
    public LayerMask isLayer;  // Raycast ��� ���̾�

    private Vector2 moveDirection;  // �Ѿ��� �̵� ����

    public void InitializeBullet(Vector2 dir)
    {
        moveDirection = dir;
    }

    void Start()
    {
        Invoke("DestroyBullet", 1);  // 1�� �Ŀ� �Ѿ� �ı�

        // ĳ������ ���⿡ ���� �Ѿ��� ������ ����
        moveDirection = transform.right * Mathf.Sign(transform.localScale.x);
        // �Ѿ��� �ð��� ���⵵ ���߱�
        transform.localScale = new Vector3(Mathf.Sign(transform.localScale.x), 1, 1);
    }

    void Update()
    {
        RaycastHit2D ray = Physics2D.Raycast(transform.position, moveDirection, distance, isLayer);
        if (ray.collider != null)
        {
            if (ray.collider.tag == "Enemy")
            {
                Debug.Log("����");
                DestroyBullet();
            }
        }

        transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);
    }


    void DestroyBullet()
    {
        Destroy(gameObject);
    }
}
