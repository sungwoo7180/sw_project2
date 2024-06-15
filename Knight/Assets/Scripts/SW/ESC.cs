using Goldmetal.UndeadSurvivor;
using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class ESC : MonoBehaviour
    {
        RectTransform rect;
        void Awake()
        {
            rect = GetComponent<RectTransform>();  
        }
        void Start()
        {
            // ���� ���� �� �޴� �����
            Hide();
        }
        public void Show()
        {
            rect.localScale = Vector3.one;
            GameManager.instance.Stop();
    }
        public void Hide()
        {
            rect.localScale = Vector3.zero;
            GameManager.instance.Resume();
        }
        // Update is called once per frame
        void Update()
        {
        
        }
    }
