using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiSpriteCharacter : MonoBehaviour
{
    public Sprite[] sprites; // 다중 스프라이트 배열

    // 기본 스프라이트를 반환하는 메서드
    public Sprite GetDefaultSprite()
    {
        if (sprites != null && sprites.Length > 0)
        {
            return sprites[0]; // 첫 번째 스프라이트를 기본 스프라이트로 반환
        }
        return null;
    }
}