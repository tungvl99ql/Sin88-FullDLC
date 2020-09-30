﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Bird
{
    public class DrawLineEffect : MonoBehaviour
    {
        public SparkLine Line;
        public GameObject Panel;
        public void DrawLine(RectTransform Pos1, RectTransform Pos2)
        {
         

            var l = Instantiate(Line, Panel.transform) as SparkLine;
            l.name = Pos1.name + "" + Pos2.name;
            l.Draw(Pos1.transform.position, Pos2.transform.position) ;
            l.gameObject.SetActive(true);
        }
        public void DrawLine(RectTransform Pos1, RectTransform Pos2 ,GameObject parent,Color32 color)
        {


            var l = Instantiate(Line, parent.transform) as SparkLine;
            l.name = Pos1.name + "" + Pos2.name;
            l.transform.GetChild(0).GetComponent<Image>().color = color;
            l.Draw(Pos1.transform.position, Pos2.transform.position);
            l.gameObject.SetActive(true);
        }
        public void Disable()
        {
            
        }
        public void Enable()
        {
            
        }
        public void Delete()
        {
            for (int i = 0; i < Panel.transform.childCount; i++)
            {
                Destroy(Panel.transform.GetChild(i).gameObject);
            }
        }

    }
}