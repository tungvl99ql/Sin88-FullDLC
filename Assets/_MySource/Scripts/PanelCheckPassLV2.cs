using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelCheckPassLV2 : MonoBehaviour
{
    public void OnOpen()
    {
        this.gameObject.SetActive(true);
    }
    public void OnClose()
    {
        this.gameObject.SetActive(false);
    }
}
