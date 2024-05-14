using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum Character
{
    Black, Blue, Green, Player
}
public class DataMgr : MonoBehaviour
{
    public static DataMgr instance;
    private void Awake()
    {
        if (instance == null) instance = this;
        else if (instance != null) return;
        DontDestroyOnLoad(gameObject);
    }

    public Character player1currentCharacter;
    public Character player2currentCharacter;
}
