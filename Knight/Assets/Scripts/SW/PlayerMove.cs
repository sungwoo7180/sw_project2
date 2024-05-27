using System;
using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class PlayerMove : MonoBehaviour
    {

        public Transform groundCheck;      // �ٴ� ������ �� GameObject
        public float groundCheckRadius;    // �ٴ��� üũ�� �ݰ�
        public LayerMask groundLayer;      // �ٴ����� ����� ���̾�

        public float dashPower = 15f; // �뽬 �Ŀ� ����
        private bool isDashing = false; // �뽬 �������� Ȯ���ϴ� ����
        private bool isMoving = false; // ������ ���¸� �����մϴ�.
        public float movePower = 1f;   //move �Ŀ�
        public float jumpPower = 3000f;       // ���� �Ŀ� ����
        
        //���� ����
        private int jumpCount = 0; // ���� ���� Ƚ��
        private bool isGrounded = false; // �ٴڿ� ��Ҵ��� ��Ÿ��
        private bool isDead = false; // ��� ����
        private int maxJump = 2; // �ִ� ���� Ƚ���� ���� (���� ����)
        bool isJumping = false;

        private bool isDefending = false; // ��� ���¸� ��Ÿ���� ���� �߰�


        private Rigidbody2D rigid;          // ����� ������ٵ� ������Ʈ
        private Animator animator;          // ����� �ִϸ����� ������Ʈ
        private AudioSource playerAudio;     // ����� ����� �ҽ� ������Ʈ
                                             // Start is called before the first frame update

        // ��ų ����
        public GameObject swordTrailEffect;  // �˱� ����Ʈ ������
        public Transform pos;
        private bool isUsingSkill = false; // ��ų ��� ����


    private void Start()
        {
            // ���� ������Ʈ�κ��� ����� �����͵��� ������ ������ �Ҵ�
            rigid = gameObject.GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            playerAudio = GetComponent<AudioSource>();


        }
        void Awake()
        {
            rigid = GetComponent<Rigidbody2D>();
        }

    // Update is called once per frame
    void Update()
    {
        // ����� ó���� �� �̻� �������� �ʰ� ����
        if (isDead) return;

        float h = Input.GetAxisRaw("Horizontal");

        // �̵� ���¸� Animator�� �ݿ�
        animator.SetFloat("Speed", Mathf.Abs(h));

        // �̵� �Է��� ������ �̵�
        if (Mathf.Abs(h) > 0.01f)
        {
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }

        // ���� �Է� ó��
        if (Input.GetKeyDown(KeyCode.K) && jumpCount < maxJump)
        {
            // ���� Ƚ�� ����
            jumpCount++;
            // ���� ������ �ӵ��� ���������� ����(0, 0)�� ����
            rigid.velocity = Vector2.zero;
            // ������ٵ� �������� ���� �ֱ�
            rigid.AddForce(new Vector2(0, jumpPower));
            isJumping = true;
            animator.SetBool("isJumping", true);

        }
        else if (Input.GetKeyDown(KeyCode.K) && rigid.velocity.y > 0)
        {
            // ���콺 ���� ��ư���� ���� ���� ���� && �ӵ��� y ���� ������ (���� ��� ��)
            // ���� �ӵ��� �������� ����
            rigid.velocity = rigid.velocity * 0.5f;
        }

        // ��� ��� ó��
        if (Input.GetKey(KeyCode.S))
        {
            if (!isDefending)
            {
                isDefending = true; // ��� ���� Ȱ��ȭ
                animator.SetBool("isDefending", true);
            }
        } else if (isDefending)
        {
            isDefending = false; // ��� ���� ��Ȱ��ȭ
            animator.SetBool("isDefending", false);
        }

        // �뽬 �Է� ó��
        if (Input.GetKeyDown(KeyCode.L) && !isDashing)
        {
            isDashing = true;
            animator.SetBool("isDashing", true);
        }
        // �ִϸ������� Grounded �Ķ���͸� isGrounded ������ ����
        animator.SetBool("Grounded", isGrounded);

        // ��ų ��� �Է� ó��
        if (Input.GetKeyDown(KeyCode.U))
        {
            UseSkill();
        }
    }

    //Physics engine Updates
    void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // �뽬 ����
        if (isDashing)
            Dash();
        else
        {
            Move();
            //           if (isJumping)
            //               Jump();
            if (jumpCount > 0 && !animator.GetBool("isJumping"))
            {
                animator.SetBool("isJumping", false);
                jumpCount = 0;
            }
        }
    }

    void Move()
    {
        float h = Input.GetAxisRaw("Horizontal");
        Vector2 moveVelocity = new Vector2(h * movePower, rigid.velocity.y);
        rigid.velocity = moveVelocity;

        // ĳ������ ������ �������� ��������Ʈ ������
        if (h > 0)
        {
            transform.localScale = new Vector3(1, 1, 1); // ������ �̵�
        }
        else if (h < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1); // ���� �̵�
        }
    }

    void Move(Vector2 direction)
        {
            rigid.velocity = new Vector2(direction.x * movePower, direction.y * movePower);
        }

    void Jump()
    {
        if ( isGrounded && jumpCount < maxJump)
        {
            Debug.Log("Jumping");

            rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            jumpCount++;
            isJumping = true;
            animator.SetBool("isJumping", true);
            // �ڷ�ƾ�� ����Ͽ� ���� ��ٿ��
            StartCoroutine(ResetJump());
        }
    }
    IEnumerator ResetJump()
    {
        yield return new WaitForSeconds(0.1f); // ª�� ������ �Ŀ� �ٽ� ������ ���
        isGrounded = false;
    }
    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.contacts[0].normal.y > 0.7f)
        {
            // isGrounded�� true�� �����ϰ�, ���� ���� Ƚ���� 0���� ����
            isGrounded = true;
            jumpCount = 0; // �ٴڿ� ������ ���� ī��Ʈ ����
            animator.SetBool("Grounded", true);
            animator.SetBool("isJumping", false);
        }
    }
    void UseSkill()
    {
        if (!isUsingSkill)  // ��ų�� �̹� ����ϰ� ���� ���� ��쿡�� ����
        {
            isUsingSkill = true;
            animator.SetBool("isUsingSkill", true);


            // ��ų ���� �� ��ų ���� ����
            StartCoroutine(ResetSkill());
        }
    }
    IEnumerator ResetSkill()
    {
        yield return new WaitForSeconds(1); // ��ų ���� �ð� 1�� ����
        // Instantiate �Ű� ���� ���� ������Ʈ, ������ġ, ȸ��
        // Instantiate(swordTrailEffect, transform.position, Quaternion.identity);  
        // �˱� ����Ʈ ����
        GameObject bullet = Instantiate(swordTrailEffect, transform.position, Quaternion.identity);
        bullet.transform.localScale = transform.localScale; // �Ѿ��� ���� ����
        bullet.GetComponent<Bullet>().InitializeBullet(transform.right * transform.localScale.x);


        isUsingSkill = false;
        animator.SetBool("isUsingSkill", false);
    }
    void Dash()
    {
        if (isDashing)
        {
            Debug.Log("Dashing");
            // �뽬 ������ ���� ĳ���Ͱ� �ٶ󺸴� ������ �Ǿ�� �մϴ�.
            // �뽬 ������ �����ϱ� ���� ĳ������ ���� ����(������)�� ����� �� �ֽ��ϴ�.
            float dashDirection = Mathf.Sign(transform.localScale.x);
            rigid.velocity = new Vector2(dashDirection * Mathf.Abs(dashPower), rigid.velocity.y);

            // ���⿡ �뽬 ���� �ð��� ó���ϴ� ������ �߰��� �� �ֽ��ϴ�.
            // ���� ��� �ڷ�ƾ�� ����Ͽ� �뽬�� ���� �ð� ���ȸ� ���ӵǵ��� �� �� �ֽ��ϴ�.
            StartCoroutine(DashCoroutine());
        }
    }
    IEnumerator DashCoroutine()
    {
        // �뽬 ���� Ȱ��ȭ
        isDashing = true;
        animator.SetBool("isDashing", true);

        // �뽬�� ���� �ð� ���ӵǰ� �����մϴ�. ��: 0.5��
        yield return new WaitForSeconds(0.35f);

        // �뽬 �Ŀ� ĳ������ ���� �ӵ��� ������� �����մϴ�.
        rigid.velocity = new Vector2(0, rigid.velocity.y);

        // �뽬 ���� ��Ȱ��ȭ
        isDashing = false;
        animator.SetBool("isDashing", false);
    }
    private void Die()
    {

    }
}
