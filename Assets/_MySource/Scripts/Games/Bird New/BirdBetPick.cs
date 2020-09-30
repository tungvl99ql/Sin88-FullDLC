using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Core.Server.Api;
using UnityEngine;
using UnityEngine.UI;

public class BirdBetPick : MonoBehaviour
{


    void Update()
    {
        txtMoney.text = MoneyManager.instance.FakeCoin;
    }
    private void OnEnable()
    {
       /* txtGiaTriHu100.text = "0";
        txtGiaTriHu500.text = "0";
        txtGiaTriHu1000.text = "0";
        txtGiaTriHu10000.text = "0";
        lastPotValue100 = 0;
        lastPotValue500 = 0;
        lastPotValue1000 = 0;
        lastPotValue10000 = 0;
        txtMoney.text = "0";*/
        CPlayer.potchanged1 += OnPotChanged;
    }
    private void OnDisable()
    {
      CPlayer.potchanged1 -= OnPotChanged;
    }
    private int lastPotValue100 = 0;
    private int lastPotValue500 = 0;
    private int lastPotValue1000 = 0;
    private int lastPotValue10000 = 0;
    public Text txtGiaTriHu100;
    public Text txtGiaTriHu500;
    public Text txtGiaTriHu1000;
    public Text txtGiaTriHu10000;
    public Text txtMoney;
    private const string gameCode= GameCodeApp.gameCode2;
    public void OnPotChanged()
    {
        InBoundMessage res = CPlayer.res_pot1;
        // number of games.
        int count = res.readByte();
       // Debug.Log("count "+count);
        for (int i = 0; i < count; i++)
        {
            // game name
            string gameId = res.readString();
                //  Debug.Log("gameId "+gameId);
            if (gameId == gameCode)
            {
                int count0 = res.readByte();
                 //  Debug.Log("count0 "+count0);
                for (int j = 0; j < count0; j++)
                {
                    int bet = res.readInt();
                    int value = res.readInt();
 //Debug.Log("bet "+bet);
 //Debug.Log("value "+value);
                    if (bet == 100)
                    {
                        txtGiaTriHu100.text = string.Format("{0:0,0}", value);
                        StartCoroutine(TweenNum(txtGiaTriHu100, lastPotValue100, value, 1f, 1f));
                        lastPotValue100 = value;
                        //return;
                    }
                    if (bet == 500)
                    {
                        txtGiaTriHu500.text = string.Format("{0:0,0}", value);
                        StartCoroutine(TweenNum(txtGiaTriHu500, lastPotValue500, value, 1f, 1f));
                        lastPotValue500 = value;
                        //return;
                    }
                    if (bet == 1000)
                    {
                        txtGiaTriHu1000.text = string.Format("{0:0,0}", value);
                        StartCoroutine(TweenNum(txtGiaTriHu1000, lastPotValue1000, value, 1f, 1f));
                        lastPotValue1000 = value;
                        //return;
                    }
                    if (bet == 10000)
                    {
                        txtGiaTriHu10000.text = string.Format("{0:0,0}", value);
                        StartCoroutine(TweenNum(txtGiaTriHu10000, lastPotValue10000, value, 1f, 1f));
                        lastPotValue10000 = value;
                        //return;
                    }
                   
                }
                return;
            }
            else
            {
                int count1 = res.readByte();
                for (int j = 0; j < count1; j++)
                {
                    int bet = res.readInt();
                    int value = res.readInt();
                }
            }
        }
    }
    private IEnumerator TweenNum(Text txt, int fromNum, int toNum, float tweenTime = 3, float scaleNum = 1.5f, float delay = 0)
    {
        if (delay > 0)
            yield return new WaitForSeconds(delay);
        float i = 0.0f;
        float rate = 2.0f / tweenTime;
        txt.transform.DOScale(scaleNum, tweenTime / 2);
        while (i < tweenTime)
        {
            i += Time.deltaTime * rate;
            float a = Mathf.Lerp(fromNum, toNum, i);

            txt.text = a > 0 ? string.Format("{0:0,0}", a) : "0";
            if (a == toNum)
            {
                i = tweenTime;
            }
            yield return null;
        }
        txt.transform.localScale = Vector2.one;
        yield return new WaitForSeconds(.05f);
    }


}
