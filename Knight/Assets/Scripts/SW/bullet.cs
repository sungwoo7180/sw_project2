using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bullet : MonoBehaviour
{
    public float speed;
    // Start is called before the first frame update

    public float distance;
    public LayerMask isLayer;

    void Start()
    {
        // Invok("���� �� �Լ���", �����ð�); : ���� �ð��� ���� �� Ư�� �Լ��� ȣ�� �� �� ����.
        Invoke("DestroyBullet", 1);
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit2D ray = Physics2D.Raycast(transform.position, transform.right, distance, isLayer);
        if(ray.collider != null)
        {
            if(ray.collider.tag == "Enemy")
            {
                Debug.Log("����");
            }
            DestroyBullet();
        }
        if(transform.rotation.y == 0)
        {
            transform.Translate(transform.right * speed * Time.deltaTime);
        } else
        {
            transform.Translate(transform.right * -1 * speed * Time.deltaTime);
        }

    }

    void DestroyBullet()
    {
        Destroy(gameObject);
    }
}
