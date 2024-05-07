using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class SelectChar : MonoBehaviour
{
    public Character character;
    Animator anim;
    SpriteRenderer sr;
    public SelectChar[] chars;
    private int currentIndex = 0;
    private bool isCurrent = false;
    private bool isSelected = false;

    void Start()
    {
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        if (DataMgr.instance.currentCharacter == character)
        {
            OnCurrent();
            isCurrent = true;
        }
        else
        {
            OnDeCurrent();
            isCurrent = false;
        }
    }

    void Update()
    {
        // 키보드 입력을 사용하여 캐릭터를 선택합니다.
        if (Input.GetKeyDown(KeyCode.A))
        {
            currentIndex--;
            if (currentIndex < 0)
                currentIndex = chars.Length - 1;
            UpdateCharacterSelection();
            UpdateCurrentCharacter();
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            currentIndex++;
            if (currentIndex >= chars.Length)
                currentIndex = 0;
            UpdateCharacterSelection();
            UpdateCurrentCharacter();
        }
        else if (Input.GetKeyDown(KeyCode.J) && !isSelected)
        {
            SelectCurrentCharacter();
        }
    }

    void UpdateCharacterSelection()
    {
        // 선택된 캐릭터를 업데이트합니다.
        for (int i = 0; i < chars.Length; i++)
        {
            if (i == currentIndex)
                chars[i].OnCurrent();
            else
                chars[i].OnDeCurrent();
        }
    }

    void UpdateCurrentCharacter()
    {
        // 현재 캐릭터를 업데이트합니다.
        DataMgr.instance.currentCharacter = chars[currentIndex].character;
    }

    void SelectCurrentCharacter()
    {
        // 현재 캐릭터를 선택합니다.
        isSelected = true;
        // 선택된 캐릭터의 애니메이션 실행
        for (int i = 0; i < chars.Length; i++)
        {
            if (i == currentIndex)
                chars[i].anim.SetBool("run", true);
            else
                chars[i].anim.SetBool("run", false); // 다른 캐릭터의 애니메이션 정지
        }
    }

    void OnDeCurrent()
    {
        // 선택되지 않은 캐릭터의 색상 변경
        sr.color = new Color(0.5f, 0.5f, 0.5f);
        // 선택되지 않은 캐릭터의 애니메이션 정지
        anim.SetBool("run", false);
    }

    void OnCurrent()
    {
        // 선택된 캐릭터의 색상 변경
        sr.color = new Color(1f, 1f, 1f);
        // 선택된 캐릭터의 애니메이션 정지
        anim.SetBool("run", false);
    }
}