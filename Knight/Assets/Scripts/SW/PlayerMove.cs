using Goldmetal.UndeadSurvivor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public int playerIndex = 1;  // 기본값은 1P로 설정

    // 키 설정을 위한 public 변수들
    public KeyCode jumpKey = KeyCode.K;
    public KeyCode attackKey = KeyCode.J;
    public KeyCode dashKey = KeyCode.L;
    public KeyCode defendKey = KeyCode.S;
    public KeyCode skillKey = KeyCode.U;
    public KeyCode ultimateKey = KeyCode.I;

    public Transform groundCheck;      // 바닥 감지용 빈 GameObject
    public float groundCheckRadius;    // 바닥을 체크할 반경
    public LayerMask groundLayer;      // 바닥으로 취급할 레이어

    public float dashPower = 15f; // 대쉬 파워 설정
    private bool isDashing = false; // 대쉬 상태인지 확인하는 변수
    private bool isMoving = false; // 움직임 상태를 추적합니다.
    public float movePower = 1f;   //move 파워
    public float jumpPower = 3000f;       // 점프 파워 증가

    //점프 로직
    private int jumpCount = 0; // 누적 점프 횟수
    private bool isGrounded = false; // 바닥에 닿았는지 나타냄
    private bool isDead = false; // 사망 상태
    private int maxJump = 2; // 최대 점프 횟수를 설정 (더블 점프)
    bool isJumping = false;

    private bool isDefending = false; // 방어 상태를 나타내는 변수 추가


    private Rigidbody2D rigid;          // 사용할 리지드바디 컴포넌트
    private Animator animator;          // 사용할 애니메이터 컴포넌트
    private AudioSource playerAudio;     // 사용할 오디오 소스 컴포넌트
                                         // Start is called before the first frame update

    // 스킬 로직 변수
    public GameObject swordTrailEffect;  // 검귀 이펙트 프리팹
    public Transform pos;
    private bool isUsingSkill = false; // 스킬 사용 상태

    // 일반 공격 로직 변수
    private int attackStep = 0;  // 현재 공격 단계
    private float attackResetTime = 0.5f;  // 연속 공격 입력 대기 시간
    private Coroutine attackRoutine;

    private void Start()
    {
        // 게임 오브젝트로부터 사용할 컴포터들을 가져와 변수에 할당
        rigid = gameObject.GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerAudio = GetComponent<AudioSource>();
        // Horizontal 축을 기반으로 입력을 받습니다.

        // 플레이어 인덱스에 따라 키를 설정합니다.
        if (playerIndex == 1)
        {
            // 1P 키 설정
            jumpKey = KeyCode.K;
            attackKey = KeyCode.J;
            dashKey = KeyCode.L;
            defendKey = KeyCode.S;
            skillKey = KeyCode.U;
            ultimateKey = KeyCode.I;
        }
        else if (playerIndex == 2)
        {
            // 2P 키 설정
            jumpKey = KeyCode.Keypad2;
            attackKey = KeyCode.Keypad1;
            dashKey = KeyCode.Keypad3;
            defendKey = KeyCode.DownArrow; // NUM 패드에는 보통 방어 키가 없기 때문에 예를 들어 방향키 아래로 설정
            skillKey = KeyCode.Keypad4;
            ultimateKey = KeyCode.Keypad5;
        }
    }
    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
        if (!GameManager.instance.isLive)
        {
            return;
        }
        
        // 사망시 처리를 더 이상 진행하지 않고 종료
        if (isDead) return;
        string horizontalAxis = playerIndex == 1 ? "Horizontal1" : "Horizontal2";
        float h = Input.GetAxisRaw(horizontalAxis);

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
        if (Input.GetKeyDown(jumpKey) && jumpCount < maxJump)
        {
            // 점프 횟수 증가
            jumpCount++;
            // 점프 직전에 속도를 순간적으로 제로(0, 0)로 변경
            rigid.velocity = Vector2.zero;
            // 리지드바디에 위쪽으로 힘을 주기
            rigid.AddForce(new Vector2(0, jumpPower));
            isJumping = true;
            animator.SetBool("isJumping", true);

        }
        else if (Input.GetKeyDown(jumpKey) && rigid.velocity.y > 0)
        {
            // 마우스 왼쪽 버튼에서 손을 떼는 순간 && 속도의 y 값이 양수라면 (위로 상승 중)
            // 현재 속도를 절반으로 변경
            rigid.velocity = rigid.velocity * 0.5f;
        }

        // 방어 모션 처리
        if (Input.GetKey(defendKey))
        {
            if (!isDefending)
            {
                isDefending = true; // 방어 상태 활성화
                animator.SetBool("isDefending", true);
            }
        }
        else if (isDefending)
        {
            isDefending = false; // 방어 상태 비활성화
            animator.SetBool("isDefending", false);
        }

        // 대쉬 입력 처리
        if (Input.GetKeyDown(dashKey) && !isDashing)
        {
            isDashing = true;
            animator.SetBool("isDashing", true);
        }
        // 애니메이터의 Grounded 파라미터를 isGrounded 값으로 갱신
        //animator.SetBool("Grounded", isGrounded);

        // 스킬 사용 입력 처리
        if (Input.GetKeyDown(skillKey))
        {
            UseSkill();
        }

        // 기본 공격 입력 처리
        if (Input.GetKeyDown(attackKey))
        {
            HandleAttack();
        }
    }

    private void HandleAttack()
    {
        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }

        attackStep++;
        if (attackStep > 4)  // 가정: 최대 3단계 공격
            attackStep = 1;

        animator.SetTrigger("isAttacking");
        animator.SetInteger("AttackStep", attackStep);

        attackRoutine = StartCoroutine(ResetAttackRoutine());
    }
    private IEnumerator ResetAttackRoutine()
    {
        yield return new WaitForSeconds(attackResetTime);
        attackStep = 0;  // 공격 단계 초기화
        animator.SetInteger("AttackStep", attackStep);
    }
    public void PlayAnimation(int atkNum)
    {
        //animator.SetFloat("Blend", atkNum);
        animator.SetTrigger("Atk");
    }

    IEnumerable ComboAtk()
    {
        yield return null; 
        while(Input.GetKeyDown(attackKey))
        {
            
        }
    }

    //Physics engine Updates
    void FixedUpdate()
    {
        if (!GameManager.instance.isLive)
        {
            return;
        }
        //isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // 대쉬 로직
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
        string horizontalAxis = playerIndex == 1 ? "Horizontal1" : "Horizontal2";
        float h = Input.GetAxisRaw(horizontalAxis);
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


    void Jump()
    {
        if (isGrounded && jumpCount < maxJump)
        {
            Debug.Log("Jumping");

            rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            jumpCount++;
            isJumping = true;
            animator.SetBool("isJumping", true);
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
            // isGrounded를 true로 변경하고, 누적 점프 횟수를 0으로 리셋
            isGrounded = true;
            jumpCount = 0; // 바닥에 닿으면 점프 카운트 리셋
            animator.SetBool("Grounded", true);
            animator.SetBool("isJumping", false);
        }
    }
    void UseSkill()
    {
        if (!isUsingSkill)  // 스킬을 이미 사용하고 있지 않은 경우에만 실행
        {
            isUsingSkill = true;
            animator.SetBool("isUsingSkill", true);


            // 스킬 종료 후 스킬 상태 리셋
            StartCoroutine(ResetSkill());
        }
    }
    IEnumerator ResetSkill()
    {
        yield return new WaitForSeconds(1); // 스킬 지속 시간 1초 가정
        // Instantiate 매개 변수 원본 오브젝트, 생성위치, 회전
        // Instantiate(swordTrailEffect, transform.position, Quaternion.identity);  
        // 검귀 이펙트 생성
        GameObject bullet = Instantiate(swordTrailEffect, transform.position, Quaternion.identity);
        bullet.transform.localScale = transform.localScale; // 총알의 방향 설정
        bullet.GetComponent<Bullet>().InitializeBullet(transform.right * transform.localScale.x);


        isUsingSkill = false;
        animator.SetBool("isUsingSkill", false);
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

    private void OnCollisionStay2D(Collision2D collision)
    {
        if(!GameManager.instance.isLive)
        {
            return;
        }
        // "EnemyAttack" 태그를 가진 객체와만 충돌 시 피해를 입음, 수정 필요.
        if (collision.gameObject.CompareTag("Player")) 
        {
            // 예를 들어, 공격 객체에서 데미지 양을 받아와서 처리
            float damage = collision.gameObject.GetComponent<AttackProperties>().damage;
            TakeDamage(damage);
        }

    }
    private void TakeDamage(float damage)
    {

        if (playerIndex == 1)
        {
            GameManager.instance.health_P1 -= Time.deltaTime * damage;
            if (GameManager.instance.health_P1 <= 0)
            {
                Die();
            }
        }
        else if (playerIndex == 2)
        {
            GameManager.instance.health_P2 -= Time.deltaTime * damage;
            if (GameManager.instance.health_P2 <= 0)
            {
                Die();
            }
        }
    }

    private void Die()
    {
        animator.SetTrigger("Dead");
        
    }
}
