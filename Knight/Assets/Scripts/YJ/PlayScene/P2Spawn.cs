using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P2Spawn : MonoBehaviour
{
    public GameObject[] charPrefabs;
    public GameObject player2;

    void Start()
    {
        if (charPrefabs == null || charPrefabs.Length == 0)
        {
            Debug.LogError("charPrefabs array is not initialized or empty.");
            return;
        }

        int index = (int)DataMgr.instance.player2currentCharacter;
        if (index < 0 || index >= charPrefabs.Length)
        {
            Debug.LogError($"Index out of range: {index}");
            return;
        }
        player2 = Instantiate(charPrefabs[(int)DataMgr.instance.player2currentCharacter]);
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
    }
}
