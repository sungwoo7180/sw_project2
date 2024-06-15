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
        // �̱��� ������ �����մϴ�.
        if (instance == null)
        {
            instance = this; // �ν��Ͻ��� ���ٸ� ���� ������Ʈ�� �ν��Ͻ��� �����մϴ�.
            DontDestroyOnLoad(gameObject); // ���� ����Ǿ �ı����� �ʵ��� �����մϴ�.
        }
        else if (instance != this)
        {
            Destroy(gameObject); // �̹� �ν��Ͻ��� �����Ѵٸ� �ߺ� ������ ������Ʈ�� �ı��մϴ�.
        }
    }

    public Character player1currentCharacter;
    public Character player2currentCharacter;
}
