using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PlayerMove : MonoBehaviour
{
    public GameManager gameManager;
    public AudioClip audioJump;
    public AudioClip audioDie;
    public float maxSpeed;
    public float jumpPower;

    public int JumpCount;

    public int Count;

    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;
    Animator anim;
    AudioSource audioSource;

    CapsuleCollider2D capsuleCollider;
    
    void Start()
    {
        rigid = GetComponent<Rigidbody2D>(); 
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        audioSource = GetComponent<AudioSource>();
    }

    

    // 키를 떼었을 때 속도 감소 시킴
    void Update()
    {
        //점프
        if (Input.GetButtonDown("Jump") && !anim.GetBool("isJumping")){ //1단 점프까지만 가능케
            rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            anim.SetBool("isJumping", true);
            PlaySound("Jump");
        }
        
        //멈췄을 때    
        if (Input.GetButtonUp("Horizontal")){            
            rigid.velocity = new Vector2(rigid.velocity.normalized.x * 0.000001f, rigid.velocity.y);
        }

        //방향 전환 - getbutton대신 getbuttondown 사용시 양쪽 키보드를 같이 누르면 플레이어 방향이 이상해짐
        if (Input.GetButton("Horizontal")){ 
            spriteRenderer.flipX = Input.GetAxisRaw("Horizontal") == -1;
        }

        //애니메이션 변수 설정
        if(rigid.velocity.normalized.x == 0)
            anim.SetBool("isWalking", false);
        else
            anim.SetBool("isWalking", true);
    }

    void FixedUpdate()
    {
        //키보드로 이동
        float h = Input.GetAxisRaw("Horizontal");
        rigid.AddForce(Vector2.right * h, ForceMode2D.Impulse);

        //위대로만 해두면 너무 가속이 많이 되버림 그래서 밑을 사용
        if(rigid.velocity.x > maxSpeed){//right max speed
            rigid.velocity = new Vector2(maxSpeed, rigid.velocity.y);
        }
        else if (rigid.velocity.x < maxSpeed*(-1)){//left max speed
            rigid.velocity = new Vector2(maxSpeed * (-1), rigid.velocity.y);
        }

        //레이 캐스트 - isGrounded를 쓰면 트리거가 두번씩 발생하는데 대신 이것을 쓰면 깔끔해짐
        if(rigid.velocity.y < 0){//아래로 내려올때만 레이캐스트 활성화 되게
            Debug.DrawRay(rigid.position, Vector3.down, new Color(1,0,0));

            //플랫폼만 레이캐스트에 걸리게 플랫폼들에는 layer을 새로 만들고 밑의 코드로 설정
            RaycastHit2D rayHit = Physics2D.Raycast(rigid.position, Vector3.down, 1, LayerMask.GetMask("Platform"));

            if(rayHit.collider != null) {
                if(rayHit.distance < 0.5f)//레이캐스트 길이 설정->플레이어 크기가 1이므로 절반길이인 0.5
                    anim.SetBool("isJumping", false);//닿으면 점핑 false
            }
        }        
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Enemy"){
            Debug.Log("Ouch!");
            OnDamaged(collision.transform.position);
        }
    }
    void OnTriggerEnter2D(Collider2D collision)//돈먹기
    {
        if (collision.gameObject.tag == "Item"){
            //포인트
            bool isBronze = collision.gameObject.name.Contains("Bronze");
            bool isSilver = collision.gameObject.name.Contains("Silver");
            bool isGold = collision.gameObject.name.Contains("Gold");
            bool isBigGold = collision.gameObject.name.Contains("BigGold");
            if(isBronze)
                gameManager.stagePoint += 50;
            else if(isSilver)
                gameManager.stagePoint += 100;
            else if (isGold)
                gameManager.stagePoint += 500;
            else if(isBigGold)
                gameManager.stagePoint += 1000; 
            //아이템 사라지게 하기
            collision.gameObject.SetActive(false);
        }
        else if (collision.gameObject.tag == "Finish"){//피니시 도착시
            //다음 스테이지
            gameManager.NextStage();
        }
    }
    // void OnTriggerEnter2D(Collider2D collision)//상점 연동
    // {
    //     if (collision.gameObject.tag == "npc"){

    //     }
    // }
    void OnDamaged(Vector2 targetPos)
    {
        //피해시 체력 감소
        gameManager.HealthDown();

        gameObject.layer = 11;//레이어 바꾸기
        spriteRenderer.color = new Color(1,1,1, 0.3f);//무적시 효과

        int dirc = transform.position.x - targetPos.x > 0 ? 1 : -1;
        //왼쪽으로 팅겨나갈지 오른쪽으로 팅겨나갈지
        rigid.AddForce(new Vector2(dirc,1) * 7, ForceMode2D.Impulse);//팅겨 나가게
        //
        anim.SetTrigger("Damaged");
        PlaySound("Die");
        Invoke("OffDamaged", 3);
    }

    void OffDamaged()
    {
        gameObject.layer = 10;
        spriteRenderer.color = new Color(1,1,1,1);
    }

    public void OnDie()
    {
        //연하게 하기
        spriteRenderer.color = new Color(1,1,1,0.4f);
        //스프라이트 y플립
        spriteRenderer.flipY = true;
        //콜라이더 끄기
        capsuleCollider.enabled = false;
        //죽었을시 점프
        rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);
        PlaySound("Die");
    }    
    void PlaySound(string action)
    {
        switch (action){
            case "Jump":
                audioSource.clip = audioJump;
                audioSource.Play();
                break;
            case "Die":
                audioSource.clip = audioDie;
                audioSource.Play();
                break;
        }
    }
}
