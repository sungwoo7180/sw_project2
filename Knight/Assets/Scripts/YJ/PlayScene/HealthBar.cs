using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider slider; // ü�¹� ����
    public Gradient gradient; // ���� ��ȭ
    public Image fill; // ü�¹� ä��� �̹���

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
