using UnityEngine;
using UnityEngine.UI;

public class GameManager1 : MonoBehaviour
{
    public float timerDuration = 10f; // 타이머 지속 시간 (초)
    public Text timerText; // 타이머를 표시할 텍스트
    public Text endText; // 타이머가 끝났을 때 나타날 텍스트

    private float currentTime;
    private bool timerEnded = false;

    void Start()
    {
        currentTime = timerDuration;
        endText.gameObject.SetActive(false); // 시작할 때는 타이머 끝 텍스트를 숨깁니다.
    }

    void Update()
    {
        if (!timerEnded)
        {
            currentTime -= Time.deltaTime;
            timerText.text = currentTime.ToString("F2"); // 소수점 둘째 자리까지 표시

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
        endText.gameObject.SetActive(true); // 타이머가 끝났을 때 텍스트를 표시합니다.
    }
}
