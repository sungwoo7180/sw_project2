using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    void Update()
    {
            walk();
    }

    public float walk_speed;
    void walk()
    {
        float hor = Input.GetAxis("Horizontal");
        transform.Translate(new Vector3(Mathf.Abs(hor) * walk_speed * Time.deltaTime, 0, 0));
        if (hor > 0)
        {
            transform.eulerAngles = new Vector3(0, 0, 0);
        }
        else if (hor < 0)
        {
            transform.eulerAngles = new Vector3(0, 180, 0);
        }
    }
}
