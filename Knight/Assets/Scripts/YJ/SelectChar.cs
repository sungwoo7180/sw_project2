using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SelectChar : MonoBehaviour
{
    public Character character;
    public Image characterImage;
    public Image Player2characterImage;
    Animator anim;
    SpriteRenderer sr;
    public SelectChar[] chars;
    private int[] currentIndex = new int[2];
    private bool[] isSelected = new bool[2];
    private Sprite[][] characterSprites;

    void Start()
    {
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        characterSprites = new Sprite[chars.Length][];

        for (int i = 0; i < chars.Length; i++)
        {
            characterSprites[i] = new Sprite[chars.Length];
            for (int j = 0; j < chars.Length; j++)
            {
                characterSprites[i][j] = chars[i].sr.sprite;
            }
        }

        for (int i = 0; i < 2; i++)
        {
            currentIndex[i] = 0;
            isSelected[i] = false;
        }

        characterImage.gameObject.SetActive(false);
        Player2characterImage.gameObject.SetActive(false);
    }

    void Update()
    {
        //player1 player2 캐릭터 선택 로직
        for (int i = 0; i < 2; i++)
        {
            if (!isSelected[i])
            {
                if (i == 0)
                {
                    if (Input.GetKeyDown(KeyCode.A))
                    {
                        currentIndex[i]--;
                        if (currentIndex[i] < 0)
                            currentIndex[i] = chars.Length - 1;
                        UpdateCharacterSelection(i);
                        UpdateCurrentCharacter(i);
                    }
                    else if (Input.GetKeyDown(KeyCode.D))
                    {
                        currentIndex[i]++;
                        if (currentIndex[i] >= chars.Length)
                            currentIndex[i] = 0;
                        UpdateCharacterSelection(i);
                        UpdateCurrentCharacter(i);
                    }
                    else if (Input.GetKeyDown(KeyCode.J))
                    {
                        SelectCurrentCharacter(i);
                    }
                }
                else if (i == 1)
                {
                    if (Input.GetKeyDown(KeyCode.LeftArrow))
                    {
                        currentIndex[i]--;
                        if (currentIndex[i] < 0)
                            currentIndex[i] = chars.Length - 1;
                        UpdateCharacterSelection(i);
                        UpdateCurrentCharacter(i);
                    }
                    else if (Input.GetKeyDown(KeyCode.RightArrow))
                    {
                        currentIndex[i]++;
                        if (currentIndex[i] >= chars.Length)
                            currentIndex[i] = 0;
                        UpdateCharacterSelection(i);
                        UpdateCurrentCharacter(i);
                    }
                    else if (Input.GetKeyDown(KeyCode.Keypad1))
                    {
                        SelectCurrentCharacter(i);
                    }
                }
            }
        }
    }

    void UpdateCharacterSelection(int playerIndex)
    {
        for (int i = 0; i < chars.Length; i++)
        {
            if (i == currentIndex[playerIndex])
                chars[i].OnCurrent();
            else
                chars[i].OnDeCurrent();
        }
    }

    void UpdateCurrentCharacter(int playerIndex)
    {
        if (playerIndex == 0)
        {
            DataMgr.instance.player1currentCharacter = chars[currentIndex[playerIndex]].character;
        }
        else if (playerIndex == 1)
        {
            DataMgr.instance.player2currentCharacter = chars[currentIndex[playerIndex]].character;
        }
    }

    void SelectCurrentCharacter(int playerIndex)
    {
        isSelected[playerIndex] = true;
        // 굳이 없어도 될만한 테스트용1
        /*
        for (int i = 0; i < chars.Length; i++)
        {
            if (i == currentIndex[playerIndex])
                chars[i].anim.SetBool("run", true);
            else
                chars[i].anim.SetBool("run", false);
        }*/
        if (playerIndex == 0)
        {
            characterImage.sprite = characterSprites[currentIndex[playerIndex]][playerIndex];
            characterImage.gameObject.SetActive(true);
        }
        else if (playerIndex == 1)
        {
            Player2characterImage.sprite = characterSprites[currentIndex[playerIndex]][playerIndex];
            Player2characterImage.gameObject.SetActive(true);
        }

        // 두 명의 캐릭터가 모두 선택되었는지 확인
        if (isSelected[0] && isSelected[1])
        {
            StartCoroutine(LoadPlayScene());
        }
    }

    IEnumerator LoadPlayScene()
    {
        yield return new WaitForSeconds(5f); // 5초 대기
        SceneManager.LoadScene("play"); // "play" 씬으로 이동
    }

    void OnDeCurrent()
    {
        sr.color = new Color(0.4f, 0.4f, 0.4f);
        // anim.SetBool("run", false);
    }

    void OnCurrent()
    {
        sr.color = new Color(1f, 1f, 1f);
        //anim.SetBool("run", false);
    }
}
