using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    public GameObject characterSelectionWindow; // 캐릭터 선택 창
    public Text timerText; // 타이머를 표시할 UI(Text) 요소
    public Button[] characterButtons; // 캐릭터 선택 버튼 배열
    public float timeLimit = 10f; // 제한 시간

    public Font playerTextFont; // 플레이어1 텍스트의 폰트
    public int playerTextSize = 24; // 플레이어1 텍스트의 크기


    private float timer = 0f;
    private bool isTimerRunning = false;
    private int selectedButtonIndex = 0;
    private Text currentPlayerText; // 현재 표시되고 있는 Player1 텍스트

    //캐릭터 선택 다시선택 기능?
    private bool isButtonPressed = false;
    private bool isMovementEnabled = true;
    void Update()
    {
        if (isTimerRunning)
        {
            timer += Time.deltaTime;
            float timeRemaining = Mathf.Max(0, timeLimit - timer);
            UpdateTimerDisplay(timeRemaining);

            if (timer >= timeLimit)
            {
                // 시간이 다 되면 창 닫기
                CloseCharacterUI();
            }
        }

        // 사용자 입력 감지
        if (isTimerRunning)
        {
            // A와 D 키로 버튼 이동
            if (isMovementEnabled && !isButtonPressed)
            {
                if (Input.GetKeyDown(KeyCode.A))
                {
                    SelectPreviousButton();
                    UpdatePlayerTextPosition();
                }
                else if (Input.GetKeyDown(KeyCode.D))
                {
                    SelectNextButton();
                    UpdatePlayerTextPosition();
                }
            }

            // J 키로 선택된 버튼 누르기
            if (Input.GetKeyDown(KeyCode.J))
            {
                if (!isButtonPressed)
                {
                    isButtonPressed = true;
                    isMovementEnabled = false;
                    PressSelectedButton();
                    //timeLimit = 10f;
                    currentPlayerText.color = Color.green;
                }
                else
                {
                    isButtonPressed = false;
                    isMovementEnabled = true;
                    currentPlayerText.color = Color.white;
                }
            }
        }
    }

    public void OpenCharacterUI()
    {
        characterSelectionWindow.SetActive(true); // 캐릭터 선택 창 열기
        isTimerRunning = true; // 타이머 시작
    }

    void CloseCharacterUI()
    {
        characterSelectionWindow.SetActive(false); // 캐릭터 선택 창 닫기
        isTimerRunning = false; // 타이머 중지
        timer = 0f; // 타이머 초기화
        selectedButtonIndex = 0; // 선택된 버튼 인덱스 초기화

        // 현재 Player1 텍스트 제거
        if (currentPlayerText != null)
        {
            Destroy(currentPlayerText.gameObject);
            currentPlayerText = null;
        }
    }

    void UpdateTimerDisplay(float timeRemaining)
    {
        int seconds = Mathf.FloorToInt(timeRemaining);
        timerText.text = seconds.ToString();
    }

    void SelectNextButton()
    {
        selectedButtonIndex = (selectedButtonIndex + 1) % characterButtons.Length;
    }

    void SelectPreviousButton()
    {
        selectedButtonIndex = (selectedButtonIndex - 1 + characterButtons.Length) % characterButtons.Length;
    }

    void PressSelectedButton()
    {
        characterButtons[selectedButtonIndex].onClick.Invoke();
    }

    void UpdatePlayerTextPosition()
    {
        // 현재 Player1 텍스트가 존재하면 위치를 업데이트
        if (currentPlayerText != null)
        {
            RectTransform buttonRect = characterButtons[selectedButtonIndex].GetComponent<RectTransform>();
            Vector3 buttonTopPosition = buttonRect.position + new Vector3(0f, buttonRect.rect.height / 4f, 0f);
            currentPlayerText.transform.position = buttonTopPosition;

            // 텍스트의 사이즈를 버튼의 크기에 맞게 조절
            currentPlayerText.rectTransform.sizeDelta = new Vector2(buttonRect.rect.width, buttonRect.rect.height);
        }
        else
        {
            // 플레이어1 텍스트 생성
            GameObject playerTextObject = new GameObject("Player1Text");
            currentPlayerText = playerTextObject.AddComponent<Text>();
            currentPlayerText.text = "Player1";
            currentPlayerText.font = playerTextFont; // 폰트 설정
            currentPlayerText.fontSize = playerTextSize; // 크기 설정
            currentPlayerText.color = Color.white;
            currentPlayerText.alignment = TextAnchor.UpperCenter;

            // 부모 설정
            playerTextObject.transform.SetParent(characterButtons[selectedButtonIndex].transform, false);

            // 위치 설정
            RectTransform buttonRect = characterButtons[selectedButtonIndex].GetComponent<RectTransform>();
            Vector3 buttonTopPosition = buttonRect.position + new Vector3(0f, buttonRect.rect.height / 4f, 0f);
            currentPlayerText.transform.position = buttonTopPosition;

            // 텍스트의 사이즈를 버튼의 크기에 맞게 조절
            currentPlayerText.rectTransform.sizeDelta = new Vector2(buttonRect.rect.width, buttonRect.rect.height);

            // 텍스트가 버튼의 크기를 초과하지 않도록 폰트 크기 조절
            FitTextToButton(currentPlayerText);
        }
    }

    void FitTextToButton(Text text)
    {
        float buttonWidth = text.rectTransform.sizeDelta.x;
        float buttonHeight = text.rectTransform.sizeDelta.y;
        int fontSize = text.fontSize;

        // 버튼의 크기에 따라 폰트 크기를 조절
        while (text.preferredWidth > buttonWidth || text.preferredHeight > buttonHeight)
        {
            fontSize--;
            text.fontSize = fontSize;
        }
    }

}
