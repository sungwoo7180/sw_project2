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
    public Text Player1Text; // Player 1 �ؽ�Ʈ
    public Text Player2Text; // Player 2 �ؽ�Ʈ
    Animator anim;
    SpriteRenderer sr;
    public SelectChar[] chars;
    private int[] currentIndex = new int[2];
    private bool[] isSelected = new bool[2];
    private int[] selectedIndex = new int[2] { -1, -1 }; // ���õ� ĳ���� �ε��� ����
    private Sprite[][] characterSprites;

    void Start()
    {
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        // characterSprites �迭 �ʱ�ȭ
        characterSprites = new Sprite[chars.Length][];

        for (int i = 0; i < chars.Length; i++)
        {
            if (chars[i] == null)
            {
                Debug.LogError($"chars[{i}]�� �Ҵ���� �ʾҽ��ϴ�."); // ���� 1
                continue;
            }

            // chars[i]�� null�� �ƴ϶�� sr�� üũ
            chars[i].sr = chars[i].GetComponent<SpriteRenderer>();
            if (chars[i].sr == null)
            {
                Debug.LogError($"chars[{i}]�� SpriteRenderer�� �Ҵ���� �ʾҽ��ϴ�. chars[{i}]�� �̸�: {chars[i].gameObject.name}");  // ���� 2
                continue;
            }

            characterSprites[i] = new Sprite[chars.Length]; // ������ ũ��� �ʱ�ȭ
            for (int j = 0; j < chars.Length; j++)
            {
                characterSprites[i][j] = chars[i].sr.sprite; // ������ ��������Ʈ�� ����Ѵٰ� ����
            }
        }

        for (int i = 0; i < 2; i++)
        {
            currentIndex[i] = 0;
            isSelected[i] = false;
        }

        // �ؽ�Ʈ �ʱ� ��ġ ����
        UpdateText(Player1Text, 0);
        UpdateText(Player2Text, 1);

        // �ʱ� ĳ���� ���� ���� ������Ʈ
        UpdateCharacterSelection();
    }



    void Update()
    {
        //player1 player2 ĳ���� ���� ����
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
                    else if (Input.GetKeyDown(KeyCode.Alpha1))
                    {
                        Debug.Log("선택");
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
        selectedIndex[playerIndex] = currentIndex[playerIndex]; // ���õ� ĳ���� �ε��� ����

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

        // �� ���� ĳ���Ͱ� ��� ���õǾ����� Ȯ��
        if (isSelected[0] && isSelected[1])
        {
            StartCoroutine(LoadPlayScene());
        }
    }

    IEnumerator LoadPlayScene()
    {
        yield return new WaitForSeconds(5f); // 5�� ���
        SceneManager.LoadScene("play"); // "play" ������ �̵�
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
