using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class P1Spawn : MonoBehaviour
{
    public GameObject[] charPrefabs;
    public GameObject player;
    

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
        player = Instantiate(charPrefabs[(int)DataMgr.instance.player2currentCharacter]);
        if (player == null)
        {
            Debug.LogError("Failed to instantiate player object.");
            return;
        }

        PlayerMove playerMove = player.GetComponent<PlayerMove>();
        if (playerMove == null)
        {
            Debug.LogError("PlayerMove component not found on the instantiated player object.");
            return;
        }

        playerMove.playerIndex = 1; // Assuming player 1 configuration
    }
}
