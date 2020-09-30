using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrawLineEffect : MonoBehaviour {


    public DrawLine Line;
    public GameObject Panel;
    public void DrawLine(RectTransform Pos1, RectTransform Pos2)
    {


        var l = Instantiate(Line, Panel.transform) as DrawLine;
        l.name = Pos1.name + "" + Pos2.name;
        l.Draw(Pos1.transform.position, Pos2.transform.position);
        l.gameObject.SetActive(true);
    }
    public void DrawLine(RectTransform Pos1, RectTransform Pos2, GameObject parent, Color32 color)
    {


        var l = Instantiate(Line, parent.transform) as DrawLine;
        l.name = Pos1.name + "" + Pos2.name;
        l.transform.GetComponent<Image>().color = color;
        l.Draw(Pos1.transform.position, Pos2.transform.position);
        l.gameObject.SetActive(true);
    }
}
