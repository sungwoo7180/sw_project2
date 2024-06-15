using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Goldmetal.UndeadSurvivor
{
    public class HUD : MonoBehaviour
    {
        public enum InfoType { Time, Health_1P, Health_2P, Mana_1P, Mana_2P }
        public InfoType type;

        Text myText;
        Slider mySlider;

        void Awake()
        {
            myText = GetComponent<Text>();
            mySlider = GetComponent<Slider>();
        }

        void LateUpdate()
        {
            switch (type) {
                case InfoType.Time:
                    float remainTime = GameManager.instance.maxGameTime - GameManager.instance.gameTime;
                    int min = Mathf.FloorToInt(remainTime / 60);
                    int sec = Mathf.FloorToInt(remainTime % 60);
                    myText.text = string.Format("{0:D2}:{1:D2}", min, sec);
                    break;
                case InfoType.Health_1P:
                    float curHealth_1P = GameManager.instance.health_P1;
                    float maxHealth_1P = GameManager.instance.maxHealth_P1;
                    mySlider.value = curHealth_1P / maxHealth_1P;
                    break;
                case InfoType.Mana_1P:
                    float curMana_1P = GameManager.instance.mana_P1;
                    float maxMana_1P = GameManager.instance.maxMana_P1;
                    mySlider.value = curMana_1P / maxMana_1P;
                    break;
                case InfoType.Health_2P:
                    float curHealth_2P = GameManager.instance.health_P2;
                    float maxHealth_2P = GameManager.instance.maxHealth_P2;
                    mySlider.value = curHealth_2P / maxHealth_2P;
                    break;
                case InfoType.Mana_2P:
                    float curMana_2P = GameManager.instance.mana_P2;
                    float maxMana_2P = GameManager.instance.maxMana_P2;
                    mySlider.value = curMana_2P / maxMana_2P;
                    break;


            }
        }
    }
}
