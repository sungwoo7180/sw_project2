using Goldmetal.UndeadSurvivor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follow : MonoBehaviour
{
    RectTransform rect;
    // Start is called before the first frame update
    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    private void FixedUpdate()
    {
        //rect.position = Camera.main.WorldToScreenPoint(GameManager.instance.player1P.transform.position);
        //GameManager.instance.player1PId.
    }
}
