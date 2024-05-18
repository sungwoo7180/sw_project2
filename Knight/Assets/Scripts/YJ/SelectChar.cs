using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SelectChar : MonoBehaviour
{
    public Character character;
    public Image Player1Image;
    public Image Player2Image;
    public Text Player1Text; // Player 1 텍스트
    public Text Player2Text; // Player 2 텍스트
    Animator anim;
    SpriteRenderer sr;
    public SelectChar[] chars;
    private int[] currentIndex = new int[2];
    private bool[] isSelected = new bool[2];
    private int[] selectedIndex = new int[2] { -1, -1 }; // 선택된 캐릭터 인덱스 저장
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

        // 텍스트 초기 위치 설정
        UpdateText(Player1Text, 0);
        UpdateText(Player2Text, 1);

        // 초기 캐릭터 선택 상태 업데이트
        UpdateCharacterSelection();
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
                        UpdateCharacterSelection();
                        UpdateCurrentCharacter(i);
                        UpdateText(Player1Text, i);
                    }
                    else if (Input.GetKeyDown(KeyCode.D))
                    {
                        currentIndex[i]++;
                        if (currentIndex[i] >= chars.Length)
                            currentIndex[i] = 0;
                        UpdateCharacterSelection();
                        UpdateCurrentCharacter(i);
                        UpdateText(Player1Text, i);
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
                        UpdateCharacterSelection();
                        UpdateCurrentCharacter(i);
                        UpdateText(Player2Text, i);
                    }
                    else if (Input.GetKeyDown(KeyCode.RightArrow))
                    {
                        currentIndex[i]++;
                        if (currentIndex[i] >= chars.Length)
                            currentIndex[i] = 0;
                        UpdateCharacterSelection();
                        UpdateCurrentCharacter(i);
                        UpdateText(Player2Text, i);
                    }
                    else if (Input.GetKeyDown(KeyCode.Keypad1))
                    {
                        SelectCurrentCharacter(i);
                    }
                }
            }
        }
    }

    void UpdateCharacterSelection()
    {
        for (int i = 0; i < chars.Length; i++)
        {
            if ((i == currentIndex[0] && !isSelected[0]) || (i == currentIndex[1] && !isSelected[1]))
            {
                chars[i].OnCurrent();
            }
            else if (i != selectedIndex[0] && i != selectedIndex[1])
            {
                chars[i].OnDeCurrent();
            }
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
        selectedIndex[playerIndex] = currentIndex[playerIndex]; // 선택된 캐릭터 인덱스 저장

        if (playerIndex == 0)
        {
            Player1Image.sprite = characterSprites[currentIndex[playerIndex]][playerIndex];
            Player1Image.gameObject.SetActive(true);
        }
        else if (playerIndex == 1)
        {
            Player2Image.sprite = characterSprites[currentIndex[playerIndex]][playerIndex];
            Player2Image.gameObject.SetActive(true);
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
    }

    void OnCurrent()
    {
        sr.color = new Color(1f, 1f, 1f);
    }

    void UpdateText(Text text, int playerIndex)
    {
        text.transform.position = Camera.main.WorldToScreenPoint(chars[currentIndex[playerIndex]].transform.position);
    }
}
