using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public int totalPoint;
    public int stagePoint;
    public int stageIndex;
    public int health;
    public PlayerMove player;
    public GameObject[] Stages;
    public Image[] UIhealth;
    public TextMeshProUGUI UIPoint;
    public TextMeshProUGUI UIStage;
    public GameObject UIRestartBtn;
    
    void Update()
    {
        UIPoint.text = (totalPoint + stagePoint).ToString();
    }
    public void NextStage()
    {
        stageIndex++;
        totalPoint += stagePoint;
        stagePoint = 0;
        UIStage.text = "STAGE " +(stageIndex + 1);
    }
    
    public void HealthDown()
    {
        if(health > 1){
            health--;
            UIhealth[health].color = new(1,1,1,0.001f);//알파값 줄여서 안보이게
        }
        else{
            UIhealth[0].color = new(1,1,1,0.001f);//하트 다 안 보이게
            player.OnDie();//죽음시 함수 호출로 효과발생

            Debug.Log("You Die");//디버깅

            UIRestartBtn.SetActive(true);
        }
    }

    // Update is called once per frame
    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Player"){
            //떨어졌을 때 겜 초기화
            if(health > 1) {
                collision.attachedRigidbody.velocity = Vector2.zero;
                collision.transform.position = new Vector3(0,3,0);
            }

            HealthDown();
        }        
    }

    public void Restart()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
    }
}
