using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisconnectControl : MonoBehaviour {

    public Transform goj;

    private void OnEnable()
    {
        if (Screen.orientation == ScreenOrientation.Portrait)
            goj.localScale = .75f * Vector2.one;
        else
            goj.localScale = Vector2.one;
    }
}
