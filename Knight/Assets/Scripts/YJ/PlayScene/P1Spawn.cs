using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P1Spawn : MonoBehaviour
{
    public GameObject[] charPrefabs;
    public GameObject player;
    void Start()
    {
        player = Instantiate(charPrefabs[(int)DataMgr.instance.player1currentCharacter]);
        player.transform.position = transform.position;
    }


}
