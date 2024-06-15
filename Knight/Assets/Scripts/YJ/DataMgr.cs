using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum Character
{
    Anne, Ellen, Player
}
public class DataMgr : MonoBehaviour
{
    public static DataMgr instance;
    private void Awake()
    {
        // 싱글톤 패턴을 구현합니다.
        if (instance == null)
        {
            instance = this; // 인스턴스가 없다면 현재 오브젝트를 인스턴스로 지정합니다.
            DontDestroyOnLoad(gameObject); // 씬이 변경되어도 파괴되지 않도록 설정합니다.
        }
        else if (instance != this)
        {
            Destroy(gameObject); // 이미 인스턴스가 존재한다면 중복 생성된 오브젝트를 파괴합니다.
        }
    }

    public Character player1currentCharacter;
    public Character player2currentCharacter;
}
