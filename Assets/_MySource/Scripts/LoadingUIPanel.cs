using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingUIPanel : MonoBehaviour {

    private static LoadingUIPanel instance;
    private void Awake()
    {
        getInstance();
    }

    private void Start()
    {
        Hide();
    }

    void getInstance()
    {
        if (instance != null)
            DestroyImmediate(gameObject);
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public static void Show()
    {
        //instance.gameObject.SetActive(true);
    }
    public static void Hide()
    {
        instance.gameObject.SetActive(false);
    }
}
