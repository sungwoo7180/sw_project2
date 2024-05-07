using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectSceneMove : MonoBehaviour
{
    public void SelectSceneCtrl()
    {
        SceneManager.LoadScene("Select"); // 이동할 씬 이름
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
