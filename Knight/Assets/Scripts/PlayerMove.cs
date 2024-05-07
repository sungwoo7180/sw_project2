    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class PlayerMove : MonoBehaviour
    {

        public Transform groundCheck;      // 바닥 감지용 빈 GameObject
        public float groundCheckRadius;    // 바닥을 체크할 반경
        public LayerMask groundLayer;      // 바닥으로 취급할 레이어

        public float dashPower = 15f; // 대쉬 파워 설정
        private bool isDashing = false; // 대쉬 상태인지 확인하는 변수

        private bool isMoving = false; // 움직임 상태를 추적합니다.

        public float movePower = 1f;   //move 파워
        public float jumpPower = 30f;       // 점프 파워 증가

        private int jumpCount = 0; // 누적 점프 횟수
        private bool isGrounded = false; // 바닥에 닿았는지 나타냄
        private bool isDead = false; // 사망 상태
        private int maxJump = 2; // 최대 점프 횟수를 설정 (더블 점프)
        bool isJumping = false;

        private Rigidbody2D rigid;          // 사용할 리지드바디 컴포넌트
        private Animator animator;          // 사용할 애니메이터 컴포넌트
        private AudioSource playerAudio;     // 사용할 오디오 소스 컴포넌트
        // Start is called before the first frame update

        private void Start()
        {
            // 게임 오브젝트로부터 사용할 컴포터들을 가져와 변수에 할당
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
        if (isDead)
            return;

        float h = Input.GetAxisRaw("Horizontal");

        // 이동 상태를 Animator에 반영
        animator.SetFloat("Speed", Mathf.Abs(h));

        // 이동 입력이 있으면 이동
        if (Mathf.Abs(h) > 0.01f)
        {
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }

        // 점프 입력 처리
        if (Input.GetKeyDown(KeyCode.K) && jumpCount < maxJump)
        {
            isJumping = true;
            animator.SetBool("isJumping", true);
        }

        // 대쉬 입력 처리
        if (Input.GetKeyDown(KeyCode.L) && !isDashing)
        {
            isDashing = true;
            animator.SetBool("isDashing", true);
        }
    }
    //Physics engine Updates
    void FixedUpdate()
    {
        // 대쉬 로직
        if (isDashing)
            Dash();
        else
        {
            Move();
            if (isJumping)
                Jump();
        }
    }

    void Move()
    {
        float h = Input.GetAxisRaw("Horizontal");
        Vector2 moveVelocity = new Vector2(h * movePower, rigid.velocity.y);
        rigid.velocity = moveVelocity;

        // 캐릭터의 방향을 바탕으로 스프라이트 뒤집기
        if (h > 0)
        {
            transform.localScale = new Vector3(1, 1, 1); // 오른쪽 이동
        }
        else if (h < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1); // 왼쪽 이동
        }
    }

    void Move(Vector2 direction)
        {
            rigid.velocity = new Vector2(direction.x * movePower, direction.y * movePower);
        }

    void Jump()
    {
        if ( jumpCount < maxJump) // 점프 로직
        {

            rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            jumpCount++;
            isJumping = false;
            animator.SetBool("isJumping", false);
            // 코루틴을 사용하여 점프 디바운싱
            StartCoroutine(ResetJump());
        }
    }
    IEnumerator ResetJump()
    {
        yield return new WaitForSeconds(0.1f); // 짧은 딜레이 후에 다시 점프를 허용
        isGrounded = false;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.contacts[0].normal.y > 0.7f)
        {
            isGrounded = true;
            jumpCount = 0; // 바닥에 닿으면 점프 카운트 리셋
        }
    }

    void Dash()
    {
        if (isDashing)
        {
            Debug.Log("Dashing");
            // 대쉬 방향은 현재 캐릭터가 바라보는 방향이 되어야 합니다.
            // 대쉬 방향을 결정하기 위해 캐릭터의 현재 방향(스케일)을 사용할 수 있습니다.
            float dashDirection = Mathf.Sign(transform.localScale.x);
            rigid.velocity = new Vector2(dashDirection * Mathf.Abs(dashPower), rigid.velocity.y);

            // 여기에 대쉬 지속 시간을 처리하는 로직을 추가할 수 있습니다.
            // 예를 들어 코루틴을 사용하여 대쉬가 일정 시간 동안만 지속되도록 할 수 있습니다.
            StartCoroutine(DashCoroutine());
        }
    }
    IEnumerator DashCoroutine()
    {
        // 대쉬 상태 활성화
        isDashing = true;
        animator.SetBool("isDashing", true);

        // 대쉬가 일정 시간 지속되게 설정합니다. 예: 0.5초
        yield return new WaitForSeconds(0.35f);

        // 대쉬 후에 캐릭터의 수평 속도를 원래대로 복구합니다.
        rigid.velocity = new Vector2(0, rigid.velocity.y);

        // 대쉬 상태 비활성화
        isDashing = false;
        animator.SetBool("isDashing", false);
    }
    private void Die()
    {

    }
}
