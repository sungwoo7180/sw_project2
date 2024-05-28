using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiSpriteCharacter : MonoBehaviour
{
    public Sprite[] sprites; // ���� ��������Ʈ �迭

    // �⺻ ��������Ʈ�� ��ȯ�ϴ� �޼���
    public Sprite GetDefaultSprite()
    {
        if (sprites != null && sprites.Length > 0)
        {
            return sprites[0]; // ù ��° ��������Ʈ�� �⺻ ��������Ʈ�� ��ȯ
        }
        return null;
    }
}