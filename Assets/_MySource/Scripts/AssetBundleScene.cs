using Core.Server.Api;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public static class ButtonExtension
{
    public static void AddEventListener<T>(this Button btn, Action<T> Onclick, T param)
    {
        btn.onClick.AddListener(delegate
        {
            Onclick(param);
        });
    }
}


public class AssetBundleScene : MonoBehaviour {

    private void Start()
    {

        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        //SceneManager.LoadScene("Loading");
    }

}