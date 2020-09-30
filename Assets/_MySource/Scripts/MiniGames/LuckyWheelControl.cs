using DG.Tweening;
using Core.Server.Api;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LuckyWheelControl : MonoBehaviour {

    /// <summary>
    /// 0: nick|1: balance|2: remainSpin|3: potAmount|4-6: spin buy num|7-9: spin buy amount|10-12: spin buy amount - guide|13: winText|14: idText
    /// |15: potbreak|16:potbreak-header
    /// </summary>
    public Text[] txts;

    /// <summary>
    /// 0: ava|arrow
    /// </summary>
    public Image[] imgs;

    /// <summary>
    /// 0: bgwheel|1: wheel-overlay|2: potbreak
    /// </summary>
    public RectTransform[] rtfs;

    /// <summary>
    /// 0: spin|1: his
    /// </summary>
    public Button[] btns;
    /// <summary>
    /// 0: his-ele|1: his|2: guide
    /// </summary>
    public GameObject[] gojs;

    /// <summary>
    /// 0-1: arrow
    /// </summary>
    public Sprite[] sprts;

    /// <summary>
    /// 0: remain spin
    /// </summary>
    private int[] spinData = new int[1];
    //0: balance|1: pot|2: pot break
    private IEnumerator[] tweens = new IEnumerator[3];

    private void OnEnable()
    {
        CPlayer.changed += ChangeLuckyWheelBalance;
        CPlayer.potchangedVQMM += LuckyWheelPotChange;
        //SET DATA EXITS
        txts[0].text = App.formatNickName(CPlayer.fullName.Length > 0 ? CPlayer.fullName : CPlayer.nickName, 20);
        txts[1].text = App.FormatMoney(CPlayer.chipBalance);
        imgs[0].sprite = CPlayer.avatarSpriteToSave;


        //LOAD DATA
        var req = new OutBounMessage("GET_REMAIN_SPIN");
        req.addHead();
        App.ws.send(req.getReq(), delegate (InBoundMessage res) {
            spinData[0] = res.readInt();           //remain spin
            txts[2].text = "LƯỢT QUAY: " + spinData[0];
            txts[3].text = App.FormatMoney(res.readInt());         //pot amount
        });

        var req_BUY_INFO = new OutBounMessage("ROULETTE.GET_DATA");
        req_BUY_INFO.addHead();
        App.ws.send(req_BUY_INFO.getReq(), delegate (InBoundMessage res_BUY_INFO)
        {
            //App.trace("<color=red>[RECV]</color> ROULETTE.GET_DATA");
            int count = res_BUY_INFO.readByte();
            for (int i = 0; i < count; i++)
            {
                int quan = res_BUY_INFO.readByte();
                int amount = res_BUY_INFO.readInt();
                App.trace("quantity = " + quan + "|amount = " + amount);
                txts[4 + i].text = quan + " LẦN ";
                txts[7 + i].text = txts[10 + i].text = App.FormatMoney(amount);
            }
        });
    }

    public void Spin()
    {
        
        var req = new OutBounMessage("SPIN_LUCKY_WHEEL");
        req.addHead();
        App.ws.send(req.getReq(), delegate (InBoundMessage res)
        {
            btns[0].interactable = false;
            btns[2].interactable = false;
            spinData[0] -= 1;
            txts[2].text = "LƯỢT QUAY: " + spinData[0];
            int prize = res.readByte() + 1;
            string prizeDesc = res.readStrings();
            int prizeValue = res.readInt();
            int potBreak = res.readByte();
            //potBreak = 1;
            long id = res.readLong();

            txts[14].text = "#" + id;

            App.trace("<color=red>[RECV]</color> SPIN_LUCKY_WHEEL | PRIZE = " + prize + " PRIZEDESC = " + prizeDesc + " PRIZEVALUE = " + prizeValue + "|idSpin = " + id);

            float pos = Mathf.Floor(30 * (prize) + UnityEngine.Random.Range(3, 27));
            int spinTo = -360 * UnityEngine.Random.Range(4, 7) + (int)pos;
            Vector3 toPos = new Vector3(0, 0, spinTo + rtfs[0].rotation.z + 60);
            //rtfs[1].DORotate(toPos, 3f, RotateMode.FastBeyond360);

            imgs[1].sprite = sprts[1];          //Change arrow image
            rtfs[0].DORotate(toPos, 3f, RotateMode.FastBeyond360).SetEase(Ease.OutCubic).OnComplete(() =>
            {
                

                imgs[1].sprite = sprts[0];          //Change arrow image

                if (potBreak == 1)
                {
                    StartCoroutine(ShowPotBreak(prizeValue,"NỔ HŨ"));
                }
                else
                {
                    StartCoroutine(ShowRs(prizeValue));
                }
            });
        });
    }

    private IEnumerator ShowPotBreak(int prize, string type)
    {
        txts[15].text = "0";
        txts[16].text = type;
        rtfs[2].transform.localScale = 5 * Vector2.one;

        if (tweens[2] != null)
            StopCoroutine(tweens[2]);
        tweens[2] = TweenNum(txts[15], 0, (int)prize);
        StartCoroutine(tweens[2]);
        rtfs[2].gameObject.SetActive(true);
        rtfs[2].DOScale(1f, .5f).SetEase(Ease.OutElastic).OnComplete(() => {

            txts[15].text = string.Format("{0:0,0}", prize);
        });
        yield return new WaitForSeconds(3f);
        rtfs[2].gameObject.SetActive(false);

        txts[16].text = "";
        btns[0].interactable = true;
        btns[2].interactable = true;
    }

    public void BuySpin(int numberOfSpin)
    {
        LoadingControl.instance.ldTextList[11].text = "Bạn muốn mua thêm <color=red>" + numberOfSpin + "</color> lượt quay?";
        LoadingControl.instance.ldBtns[1].onClick.RemoveAllListeners();
        LoadingControl.instance.ldBtns[1].onClick.AddListener(() =>
        {
            var reqBuy = new OutBounMessage("BUY_ITEM");
            reqBuy.addHead();
            reqBuy.writeAcii("WOODEN_WHEEL");
            reqBuy.writeByte(numberOfSpin);
            App.ws.send(reqBuy.getReq(), delegate (InBoundMessage resBuy)
            {
                //LoadingControl.instance.CloseConfirm();
                App.trace("<color=red>[RECV]</color> BUY_ITEM //SPIN");
                spinData[0] += numberOfSpin;
                txts[2].text = "LƯỢT QUAY: " + spinData[0];
                //App.showErr("Giao dịch thành công, bạn có thêm " + numberOfSpin + " lượt quay may mắn");
                string appShowErr = PlayerPrefs.GetString("canhbao26");
                appShowErr.Replace("#1", numberOfSpin.ToString());
                App.showErr(appShowErr);
            });
        });

        LoadingControl.instance.ldBtns[1].transform.parent.parent.gameObject.SetActive(true);
    }

    public void Close()
    {
        if (tweens[1] != null)
            StopCoroutine(tweens[1]);
        CPlayer.changed -= ChangeLuckyWheelBalance;
        CPlayer.potchangedVQMM -= LuckyWheelPotChange;
        gameObject.SetActive(false);
    }

    public void OpenRechargePanel()
    {
        LoadingControl.instance.showRecharge(true);
    }

    #region //HIS

    
    public void OpenHistory()
    {
        foreach (Transform rtf in gojs[0].transform.parent)       //Delete exits element before
        {
            if (rtf.gameObject.name != gojs[0].name)
            {
                Destroy(rtf.gameObject);
            }
        }
        gojs[1].SetActive(true);

        OutBounMessage req_HIS = new OutBounMessage("MATCH.HISTORY");
        req_HIS.addHead();
        req_HIS.writeString("vongquay");         //game name
        req_HIS.writeByte(1);                       //page index
        req_HIS.writeString("");                    //from date
        req_HIS.writeString("");                    //to date
        App.ws.send(req_HIS.getReq(), delegate (InBoundMessage res_HIS)
        {
            int count = res_HIS.readByte();
            App.trace("Count = " + count);
            for (int i = 0; i < count; i++)
            {
                long index = res_HIS.readLong();
                string time = res_HIS.readStrings();
                string game = res_HIS.readStrings();
                string bet = res_HIS.readString();
                string change = res_HIS.readString();
                string balance = res_HIS.readString();

                GameObject goj = Instantiate(gojs[0], gojs[0].transform.parent, false);
                Text[] txtArr = goj.GetComponentsInChildren<Text>();
                txtArr[0].text = index.ToString();
                txtArr[1].text = time;
                txtArr[2].text = App.FormatMoney(change);
                txtArr[3].text = App.FormatMoney(balance);
                goj.SetActive(true);
            }
                
        });
    }

    public void CloseHis()
    {
        gojs[1].SetActive(false);
    }
    #endregion


    #region //GUID
    public void OpenGuidePanel(bool isOpen)
    {
        gojs[2].SetActive(isOpen);
    }
    #endregion

    private void ChangeLuckyWheelBalance(string type)
    {
        if (type == "chip")
        {
             if(CPlayer.preChipBalance > CPlayer.chipBalance)
                txts[1].text = string.Format("{0:0,0}", CPlayer.chipBalance);
            else
            {
                if (tweens[0] != null)
                    StopCoroutine(tweens[0]);
                tweens[0] = TweenNum(txts[1], CPlayer.preChipBalance, CPlayer.chipBalance,3,1.1f,5f);
                StartCoroutine(tweens[0]);
            }
        }
    }

    private IEnumerator TweenNum(Text txt, float fromNum, float toNum, float tweenTime = 3, float scaleNum = 1.5f, float delay = 0)
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
        //App.trace(i.ToString());
        txt.transform.localScale = Vector2.one;
        yield return new WaitForSeconds(.05f);
    }

    public void Test()
    {
        //StartCoroutine(_Test());
    }

    private IEnumerator ShowRs(int toNum)
    {
        int fromNum = 0;
        float i = 0.0f;
        float rate = 2.0f / 1f;
        Text txt = Instantiate(txts[13], txts[13].transform.parent, false);
        txt.transform.SetSiblingIndex(txts[13].transform.GetSiblingIndex() + 1);
        txt.gameObject.SetActive(true);
        txt.transform.DOScale(2, 1f);
        Transform savedTf = txts[1].transform.parent;
        while (i < 2)
        {
            i += Time.deltaTime * rate;
            float a = Mathf.Lerp(fromNum, toNum, i);
            txt.text = a > 0 ? string.Format("{0:0,0}", a) : "0";
            yield return null;
        }

        yield return new WaitForSeconds(.05f);
        txts[1].transform.parent  = txts[13].transform.parent;
        txts[1].transform.SetSiblingIndex(txts[13].transform.GetSiblingIndex() + 2);
        txt.transform.DOScale(1f, .5f);
        txt.transform.DOLocalMove(txts[1].transform.localPosition, .5f).OnComplete(() => {
            //txts[1].text = string.Format("{0:0,0}", toNum);
            Destroy(txt.gameObject);
            txts[1].transform.parent = savedTf;
            btns[0].interactable = true;
            btns[2].interactable = true;
        });
    }

    private void LuckyWheelPotChange()
    {
        InBoundMessage res = CPlayer.res_potMiniGameVQMM;
        int count = res.readByte();
        for (int i = 0; i < count; i++)
        {
            string gameId = res.readString();
            if (gameId == "vongquay")
            {
                int count0 = res.readByte();
                int bet = res.readInt();
                int value = res.readInt();
                if (tweens[1] != null)
                    StopCoroutine(tweens[1]);
                tweens[1] = TweenNum(txts[3], (int)App.formatMoneyBack(txts[3].text), value, 1f, 1f);
                StartCoroutine(tweens[1]);
                break;
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
