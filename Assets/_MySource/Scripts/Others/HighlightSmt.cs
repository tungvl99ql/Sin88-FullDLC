using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightSmt : MonoBehaviour {

    public GameObject[] gojs;

    private void OnEnable()
    {
        Invoke("HideSmt", 3f);
    }

    void HideSmt()
    {
        gojs[0].SetActive(false);
    }
}
