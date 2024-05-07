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
        // Ű���� �Է��� ����Ͽ� ĳ���͸� �����մϴ�.
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
        // ���õ� ĳ���͸� ������Ʈ�մϴ�.
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
        // ���� ĳ���͸� ������Ʈ�մϴ�.
        DataMgr.instance.currentCharacter = chars[currentIndex].character;
    }

    void SelectCurrentCharacter()
    {
        // ���� ĳ���͸� �����մϴ�.
        isSelected = true;
        // ���õ� ĳ������ �ִϸ��̼� ����
        for (int i = 0; i < chars.Length; i++)
        {
            if (i == currentIndex)
                chars[i].anim.SetBool("run", true);
            else
                chars[i].anim.SetBool("run", false); // �ٸ� ĳ������ �ִϸ��̼� ����
        }
    }

    void OnDeCurrent()
    {
        // ���õ��� ���� ĳ������ ���� ����
        sr.color = new Color(0.5f, 0.5f, 0.5f);
        // ���õ��� ���� ĳ������ �ִϸ��̼� ����
        anim.SetBool("run", false);
    }

    void OnCurrent()
    {
        // ���õ� ĳ������ ���� ����
        sr.color = new Color(1f, 1f, 1f);
        // ���õ� ĳ������ �ִϸ��̼� ����
        anim.SetBool("run", false);
    }
}