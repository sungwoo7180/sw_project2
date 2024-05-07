using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    public GameObject characterSelectionWindow; // ĳ���� ���� â
    public Text timerText; // Ÿ�̸Ӹ� ǥ���� UI(Text) ���
    public Button[] characterButtons; // ĳ���� ���� ��ư �迭
    public float timeLimit = 10f; // ���� �ð�

    public Font playerTextFont; // �÷��̾�1 �ؽ�Ʈ�� ��Ʈ
    public int playerTextSize = 24; // �÷��̾�1 �ؽ�Ʈ�� ũ��


    private float timer = 0f;
    private bool isTimerRunning = false;
    private int selectedButtonIndex = 0;
    private Text currentPlayerText; // ���� ǥ�õǰ� �ִ� Player1 �ؽ�Ʈ

    //ĳ���� ���� �ٽü��� ���?
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
                // �ð��� �� �Ǹ� â �ݱ�
                CloseCharacterUI();
            }
        }

        // ����� �Է� ����
        if (isTimerRunning)
        {
            // A�� D Ű�� ��ư �̵�
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

            // J Ű�� ���õ� ��ư ������
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
        characterSelectionWindow.SetActive(true); // ĳ���� ���� â ����
        isTimerRunning = true; // Ÿ�̸� ����
    }

    void CloseCharacterUI()
    {
        characterSelectionWindow.SetActive(false); // ĳ���� ���� â �ݱ�
        isTimerRunning = false; // Ÿ�̸� ����
        timer = 0f; // Ÿ�̸� �ʱ�ȭ
        selectedButtonIndex = 0; // ���õ� ��ư �ε��� �ʱ�ȭ

        // ���� Player1 �ؽ�Ʈ ����
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
        // ���� Player1 �ؽ�Ʈ�� �����ϸ� ��ġ�� ������Ʈ
        if (currentPlayerText != null)
        {
            RectTransform buttonRect = characterButtons[selectedButtonIndex].GetComponent<RectTransform>();
            Vector3 buttonTopPosition = buttonRect.position + new Vector3(0f, buttonRect.rect.height / 4f, 0f);
            currentPlayerText.transform.position = buttonTopPosition;

            // �ؽ�Ʈ�� ����� ��ư�� ũ�⿡ �°� ����
            currentPlayerText.rectTransform.sizeDelta = new Vector2(buttonRect.rect.width, buttonRect.rect.height);
        }
        else
        {
            // �÷��̾�1 �ؽ�Ʈ ����
            GameObject playerTextObject = new GameObject("Player1Text");
            currentPlayerText = playerTextObject.AddComponent<Text>();
            currentPlayerText.text = "Player1";
            currentPlayerText.font = playerTextFont; // ��Ʈ ����
            currentPlayerText.fontSize = playerTextSize; // ũ�� ����
            currentPlayerText.color = Color.white;
            currentPlayerText.alignment = TextAnchor.UpperCenter;

            // �θ� ����
            playerTextObject.transform.SetParent(characterButtons[selectedButtonIndex].transform, false);

            // ��ġ ����
            RectTransform buttonRect = characterButtons[selectedButtonIndex].GetComponent<RectTransform>();
            Vector3 buttonTopPosition = buttonRect.position + new Vector3(0f, buttonRect.rect.height / 4f, 0f);
            currentPlayerText.transform.position = buttonTopPosition;

            // �ؽ�Ʈ�� ����� ��ư�� ũ�⿡ �°� ����
            currentPlayerText.rectTransform.sizeDelta = new Vector2(buttonRect.rect.width, buttonRect.rect.height);

            // �ؽ�Ʈ�� ��ư�� ũ�⸦ �ʰ����� �ʵ��� ��Ʈ ũ�� ����
            FitTextToButton(currentPlayerText);
        }
    }

    void FitTextToButton(Text text)
    {
        float buttonWidth = text.rectTransform.sizeDelta.x;
        float buttonHeight = text.rectTransform.sizeDelta.y;
        int fontSize = text.fontSize;

        // ��ư�� ũ�⿡ ���� ��Ʈ ũ�⸦ ����
        while (text.preferredWidth > buttonWidth || text.preferredHeight > buttonHeight)
        {
            fontSize--;
            text.fontSize = fontSize;
        }
    }

}
