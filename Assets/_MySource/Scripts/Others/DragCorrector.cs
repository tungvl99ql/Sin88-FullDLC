﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class DragCorrector : MonoBehaviour {


    public int baseTH = 6;
    public int basePPI = 210;
    public int dragTH = 0;

    void Start()
    {
        dragTH = baseTH * (int)Screen.dpi / basePPI;

        EventSystem es = GetComponent<EventSystem>();

        if (es) es.pixelDragThreshold = dragTH;
    }
}
