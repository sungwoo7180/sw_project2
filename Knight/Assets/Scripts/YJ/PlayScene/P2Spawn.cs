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
<<<<<<< Updated upstream
        player2.transform.position = transform.position;
=======
        if (player2 == null)
        {
            Debug.LogError("Failed to instantiate player object.");
            return;
        }

        PlayerMove playerMove = player2.GetComponent<PlayerMove>();
        if (playerMove == null)
        {
            Debug.LogError("PlayerMove component not found on the instantiated player object.");
            return;
        }

        playerMove.playerIndex = 2; // Assuming player 2 configuration
        playerMove.playerIndex = 4;
>>>>>>> Stashed changes
    }


}
