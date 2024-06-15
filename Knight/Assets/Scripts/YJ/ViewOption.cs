using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ViewOption : MonoBehaviour
{
    FullScreenMode screenMode;
    public Dropdown resolutionDropdown;
    public Toggle fullscreenBtn;
    List<Resolution> resolutions = new List<Resolution>();
    public int resolutionNum;

    void Start()
    {
        InitUI();
    }

    void InitUI()
    {
        // �ػ� ��� �ʱ�ȭ �� �� 120Hz �ػ󵵸� �߰�
        for (int i = 0; i < Screen.resolutions.Length; i++)
        {
            var refreshRateRatio = Screen.resolutions[i].refreshRateRatio;
            float refreshRate = (float)refreshRateRatio.numerator / refreshRateRatio.denominator;

            // �ٻ簪�� ����Ͽ� 120Hz���� Ȯ��
            if (Mathf.Abs(refreshRate - 120f) < 0.1f)
            {
                resolutions.Add(Screen.resolutions[i]);
            }
        }

        // ��Ӵٿ� �ɼ� �ʱ�ȭ
        resolutionDropdown.options.Clear();

        // �ɼ� �߰� �� ���� �ػ� ����
        int optionNum = 0;
        foreach (Resolution item in resolutions)
        {
            Dropdown.OptionData option = new Dropdown.OptionData();
            var refreshRateRatio = item.refreshRateRatio;
            float refreshRate = (float)refreshRateRatio.numerator / refreshRateRatio.denominator;
            option.text = item.width + "x" + item.height + " " + refreshRate + "hz";
            resolutionDropdown.options.Add(option);

            if (item.width == Screen.width && item.height == Screen.height)
                resolutionDropdown.value = optionNum;

            optionNum++;
        }
        resolutionDropdown.RefreshShownValue();

        // Ǯ��ũ�� ����
        fullscreenBtn.isOn = Screen.fullScreenMode.Equals(FullScreenMode.FullScreenWindow) ? true : false;

        // ��Ӵٿ�� ����� ���� �̺�Ʈ�� ����
        resolutionDropdown.onValueChanged.AddListener(DropboxOptionChange);
        fullscreenBtn.onValueChanged.AddListener(FullScreenBtn);
    }

    public void DropboxOptionChange(int index)
    {
        resolutionNum = index;
    }

    public void FullScreenBtn(bool isFull)
    {
        screenMode = isFull ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
    }

    public void OkBtnClick()
    {
        Resolution selectedResolution = resolutions[resolutionNum];
        Screen.SetResolution(selectedResolution.width, selectedResolution.height, screenMode);
    }
}
