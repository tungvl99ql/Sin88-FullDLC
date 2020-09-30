using Core.Server.Api;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleController : MonoBehaviour {
    public Text text;
    public Font[] fonts;
    public Toggle tog;
    public Color[] colors;
    private bool currState = false;
   
    public void ChangeFont()
    {
        if (tog.isOn == currState)
            return;
        App.trace(gameObject.name);
        if (tog.isOn)
        {
            //App.trace("HSJKLHSDKJLHSDLKJHAD " + tog.isOn + "|" + gameObject.name, "red");
            text.font = fonts[1];
            text.color = colors[0];
            text.fontSize = 45;
            currState = true;
        }
        else
        {
            text.font = fonts[0];
            text.color = colors[1];
            text.fontSize = 35;
            currState = false;
        }
    }
}
