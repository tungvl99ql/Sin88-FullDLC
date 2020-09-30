using Core.Server.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoneyManager : MonoBehaviour {
    public static MoneyManager instance;
    private int realCoin = 0;
    private int fakeCoin;
    public string FakeCoin { get { return App.FormatMoney(fakeCoin); }}
    Coroutine CorouChangeCoin;
    Coroutine CorouChangePot;
    //public delegate void CoinChanged(string type);
    //public static event CoinChanged onCoinChanged;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }else if(instance != this)
        {
            Destroy(this);
        }
    }
    private void OnEnable()
    {
        CPlayer.changed += OnCoinChange;
    }
    public void OnLoginUpdateCurrCoin()
    {
        fakeCoin = (int)CPlayer.chipBalance;
    }
    public void OnPotChange(Text txt, int lastValue, int currValue, float time = 0.5f)
    {

        StartCoroutine(ThreadChangePot(txt, lastValue, currValue, time));
    }
    private IEnumerator ThreadChangePot(Text txt, int lastValue, int currValue, float time = 0.5f)
    {
        int totalFrame = (int)(time / Time.deltaTime);
        int potChanged = currValue - lastValue;
        for (int i = 1; i <= totalFrame; i++)
        {
            yield return new WaitForEndOfFrame();
            txt.text = i == totalFrame ? string.Format("{0:0,0}", currValue) : string.Format("{0:0,0}", (lastValue + (int)(potChanged * (i / (float)totalFrame))));
        }
    }
    private void OnCoinChange(string type)
    {
        if (type == "chip")
        {
            if (CPlayer.preChipBalance != CPlayer.chipBalance)
            {
                realCoin = (int)CPlayer.chipBalance;
                try
                {
                    StopCoroutine(CorouChangeCoin);
                }catch { }
                CorouChangeCoin = StartCoroutine(ThreadChangeMoney(2f));
            }
        }
    }

    private IEnumerator ThreadChangeMoney(float time = 1f)
    {
        int totalFrame = (int)(time / Time.deltaTime);
        int lastFakeCoin = fakeCoin;
        int coinChanged = realCoin - fakeCoin;
        for (int i = 1; i <= totalFrame; i++)
        {
            yield return new WaitForEndOfFrame();
            fakeCoin = (int)(lastFakeCoin + (coinChanged * (i / (float)totalFrame)));
        }
        fakeCoin = realCoin;
    }
}
