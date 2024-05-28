using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider slider; // 체력바 조절
    public Gradient gradient; // 색상 변화
    public Image fill; // 체력바 채우기 이미지

    public void SetMaxHealth(int health)
    {
        slider.maxValue= health;
        slider.value= health;

        fill.color = gradient.Evaluate(1f);
    }

    public void SetHealth(int health)
    {
        slider.value= health;
        fill.color = gradient.Evaluate(slider.normalizedValue);
    }
}
