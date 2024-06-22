using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AutoScrollBar : MonoBehaviour
{
    //消息滑动条
    private Scrollbar MessageScrollBar;
    private bool IsDrag = false;
    private void Awake()
    {
        MessageScrollBar = GetComponent<Scrollbar>();
    }
    private void Start()
    {
        //每帧刷新，自动滑动到底部
        StartCoroutine(ScrollCoroutine());
    }

    public void SetDrag(bool isDrag)
    {
        IsDrag = isDrag;
    }

    IEnumerator ScrollCoroutine()
    {
        while (true)
        {
            if (IsDrag == false)
            {
                MessageScrollBar.value = 0;
            }

            yield return null;
        }
    }
    
    
}


