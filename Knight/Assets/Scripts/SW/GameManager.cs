using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Goldmetal.UndeadSurvivor
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance;
        [Header("# Game Control")]
        public bool isLive = true;
        public float gameTime;
        public float maxGameTime = 2 * 10f;

        [Header("# Player1 Info")]
        public int player1PId;
        public float health_P1;
        public float maxHealth_P1 = 100;
        public float mana_P1;
        public float maxMana_P1 = 100;
        // public PlayerMove player1P;

        [Header("# Player2 Info")]
        public int player2PId;
        public float health_P2;
        public float maxHealth_P2 = 100;
        public float mana_P2;
        public float maxMana_P2 = 100;

        [Header("# Game Object")]
        public ESC uiEsc;
        public GameObject uiResult_1PWIN;
        public GameObject uiResult_2PWIN;
        public GameObject uiResult_Draw;  // 무승부 UI 객체
        //public PlayerMove player_1P;


        //public void GameStart(int ID_1P , int ID_2P )
        public void GameStart()
        {
            Debug.Log("Game has started");
            isLive = true;
            //player1PId = ID_1P;
            health_P1 = maxHealth_P1;
            mana_P1 = maxMana_P1 / 2;

            //player2PId = ID_2P;
            health_P2 = maxHealth_P2;
            mana_P2 = maxMana_P2 / 2;

            //player2PId.gameObject.SetActive(true);


        }

        void Update()
        {
            // ESC 키 입력 처리, *** !isLIve 보다 더 빨리 수행 되어야함.
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ToggleESCMenu();
            }

            if (!isLive)
                return;

            gameTime += Time.deltaTime;
            Debug.Log("Game Time: " + gameTime); // 이 로그를 통해 gameTime의 증가를 확인

            if (gameTime > maxGameTime || health_P1 <= 0 || health_P2 <= 0)
            {
                gameTime = maxGameTime;
                GameVictroy();
            }


        }
        void Awake()
        {
            instance = this;
            Application.targetFrameRate = 60;
            GameStart();
        }
        public void GameVictroy()
        {
            StartCoroutine(GameVictroyRoutine());
        }
        IEnumerator GameVictroyRoutine()
        {
            isLive = false;

            yield return new WaitForSeconds(0.5f);      // 5초 정도 딜레이
            if (health_P1 > health_P2) { 
                uiResult_1PWIN.SetActive(true); 
            } else if (health_P1 < health_P2) {
                uiResult_2PWIN.SetActive(true);
            }
            else
            {
                uiResult_Draw.SetActive(true);
            }
            Stop();
        }
        // ESC 메뉴 토글 기능
        void ToggleESCMenu()
        {
            if (uiEsc != null)
            {

                if (uiEsc.transform.localScale == Vector3.zero)
                {
                    uiEsc.Show();
                    //Stop();
                }
                else
                {
                    uiEsc.Hide();
                    //Resume();
                }
            }
        }
        public void Stop()
        {
            isLive = false;
            Time.timeScale = 0;     // 시간이 멈춤
            //uiJoy.localScale = Vector3.zero;
        }

        public void Resume()
        {
            isLive = true;
            Time.timeScale = 1;
            //uiJoy.localScale = Vector3.one;
        }
        public void GameRetry()
        {
            SceneManager.LoadScene(4);
        }

        /*
        [Header("# Player Info")]
        public int playerId;
        public float health;
        public float maxHealth = 100;
        public int level;
        public int kill;
        public int exp;
        public int[] nextExp = { 3, 5, 10, 100, 150, 210, 280, 360, 450, 600 };
        [Header("# Game Object")]
        public PoolManager pool;
        public Player player;
        public LevelUp uiLevelUp;
        public Result uiResult;
        public Transform uiJoy;
        public GameObject enemyCleaner;

        void Awake()
        {
            instance = this;
            Application.targetFrameRate = 60;
        }

        public void GameStart(int id)
        {
            playerId = id;
            health = maxHealth;

            player.gameObject.SetActive(true);
            uiLevelUp.Select(playerId % 2);
            Resume();

            AudioManager.instance.PlayBgm(true);
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
        }

        public void GameOver()
        {
            StartCoroutine(GameOverRoutine());
        }

        IEnumerator GameOverRoutine()
        {
            isLive = false;

            yield return new WaitForSeconds(0.5f);

            uiResult.gameObject.SetActive(true);
            uiResult.Lose();
            Stop();

            AudioManager.instance.PlayBgm(false);
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Lose);
        }



        IEnumerator GameVictroyRoutine()
        {
            isLive = false;
            enemyCleaner.SetActive(true);

            yield return new WaitForSeconds(0.5f);

            uiResult.gameObject.SetActive(true);
            uiResult.Win();
            Stop();

            AudioManager.instance.PlayBgm(false);
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Win);
        }

        public void GameRetry()
        {
            SceneManager.LoadScene(0);
        }

        public void GameQuit()
        {
            Application.Quit();
        }



        public void GetExp()
        {
            if (!isLive)
                return;

            exp++;

            if (exp == nextExp[Mathf.Min(level, nextExp.Length - 1)]) {
                level++;
                exp = 0;
                uiLevelUp.Show();
            }
        }

        public void Stop()
        {
            isLive = false;
            Time.timeScale = 0;
            uiJoy.localScale = Vector3.zero;
        }

        public void Resume()
        {
            isLive = true;
            Time.timeScale = 1;
            uiJoy.localScale = Vector3.one;
        }*/
    }
    }
