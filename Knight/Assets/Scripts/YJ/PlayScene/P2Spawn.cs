using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P2Spawn : MonoBehaviour
{
    public GameObject[] charPrefabs;
    public GameObject player2;
    void Start()
    {
        player2 = Instantiate(charPrefabs[(int)DataMgr.instance.player2currentCharacter]);
        player2.transform.position = transform.position;
    }


}
