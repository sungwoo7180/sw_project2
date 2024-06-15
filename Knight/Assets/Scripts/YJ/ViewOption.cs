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
        // 해상도 목록 초기화 및 약 120Hz 해상도만 추가
        for (int i = 0; i < Screen.resolutions.Length; i++)
        {
            var refreshRateRatio = Screen.resolutions[i].refreshRateRatio;
            float refreshRate = (float)refreshRateRatio.numerator / refreshRateRatio.denominator;

            // 근사값을 사용하여 120Hz인지 확인
            if (Mathf.Abs(refreshRate - 120f) < 0.1f)
            {
                resolutions.Add(Screen.resolutions[i]);
            }
        }

        // 드롭다운 옵션 초기화
        resolutionDropdown.options.Clear();

        // 옵션 추가 및 현재 해상도 설정
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

        // 풀스크린 설정
        fullscreenBtn.isOn = Screen.fullScreenMode.Equals(FullScreenMode.FullScreenWindow) ? true : false;

        // 드롭다운과 토글의 변경 이벤트를 연결
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
