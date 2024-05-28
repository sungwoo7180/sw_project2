using UnityEngine;
using UnityEngine.UI;

public class GameManager1 : MonoBehaviour
{
    public float timerDuration = 10f; // Ÿ�̸� ���� �ð� (��)
    public Text timerText; // Ÿ�̸Ӹ� ǥ���� �ؽ�Ʈ
    public Text endText; // Ÿ�̸Ӱ� ������ �� ��Ÿ�� �ؽ�Ʈ

    private float currentTime;
    private bool timerEnded = false;

    void Start()
    {
        currentTime = timerDuration;
        endText.gameObject.SetActive(false); // ������ ���� Ÿ�̸� �� �ؽ�Ʈ�� ����ϴ�.
    }

    void Update()
    {
        if (!timerEnded)
        {
            currentTime -= Time.deltaTime;
            timerText.text = currentTime.ToString("F2"); // �Ҽ��� ��° �ڸ����� ǥ��

            if (currentTime <= 0)
            {
                TimerEnded();
            }
        }
    }

    void TimerEnded()
    {
        timerEnded = true;
        currentTime = 0;
        timerText.text = "0.00";
        endText.gameObject.SetActive(true); // Ÿ�̸Ӱ� ������ �� �ؽ�Ʈ�� ǥ���մϴ�.
    }
}
