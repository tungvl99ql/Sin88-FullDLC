using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Core.Server.Api;
public class MiniPockerrControl : MonoBehaviour {

    /// <summary>
    /// 0-51: card |52-53: chosebet | 54-55: autospin
    /// </summary>
    public Sprite[] sprts;

    /// <summary>
    /// 0: history | 1: elementHistory | 2: help
    /// </summary>
    public GameObject[] gojs;

    /// <summary>
    /// 0-4: card
    /// </summary>
    public Image[] imgs;

    /// <summary>
    /// 0-2: bet | 3: pot | 4: bigwin | 5: potbreak
    /// </summary>
    public Text[] txts;

    /// <summary>
    /// 0-2: bet|3: auto
    /// </summary>
    public Button[] btns;

    /// <summary>
    /// 0: bigwin | 1: potbreak
    /// </summary>
    public RectTransform[] rtfs;

    /// <summary>
    /// 0: spin
    /// </summary>
    public Animator[] animtors;


    /// <summary>
    /// 0: sieutoc
    /// </summary>
    public Toggle[] togs;

    public SpinMiniPocker spinMiniPocker;
    /// <summary>
    /// 0: noti|1: big-win&pot-break|2: pot-change|3: balanceChange
    /// </summary>
    private IEnumerator[] threads = new IEnumerator[5];

    private List<GameObject> gojsToDel = new List<GameObject>();            //save cards to del except 5 last cards
    private List<GameObject> gojsToDel2 = new List<GameObject>();           //save 5 last card to del

    private static string[] cardTypes = { "bích", "tép", "rô", "cơ" };

    private Dictionary<int, int> betData = new Dictionary<int, int>();

    /// <summary>
    /// 0 allowSpin
    /// </summary>
    private bool[] boolLs = new bool[1];

    /// <summary>
    /// 0-5
    /// </summary>
    private Vector3[] cardEndPos = new Vector3[5];

    /// <summary>
    /// 0: currBet|1: autoSpin|2: isSpinning|3: his-page|4: win-balance|5: pot|6-8: betData
    /// </summary>
    private int[] numList = new int[14];

    public Button buttonSpin;
    private void Start()
    {
        boolLs[0] = true;
        
        CPlayer.forceStopGameEvent += EnoughMoney;
        cardEndPos[0] = new Vector3(0, -2, 0);
        cardEndPos[1] = new Vector3(0, -2, 0);

        numList[2] = -1;
        numList[0] = -1;
        btns[0].image.sprite = sprts[53];
        btns[0].gameObject.GetComponentInChildren<Text>().color = new Color32(255, 251, 98, 255);
        btns[1].image.sprite = sprts[53];
        btns[1].gameObject.GetComponentInChildren<Text>().color = new Color32(255, 251, 98, 255);
        btns[2].image.sprite = sprts[53];
        btns[2].gameObject.GetComponentInChildren<Text>().color = new Color32(255, 251, 98, 255);
        changeBet(0);

        int ranNum = UnityEngine.Random.Range(0, 3);
        List<int> ids = new List<int>() { 13 * ranNum + 9, 13 * ranNum + 10, 13 * ranNum + 11, 13 * ranNum + 12, 13 * ranNum };
        for (int i = 0; i < 5; i++)
        {
            Image img = Instantiate(imgs[i], imgs[i].transform.parent, false);
            img.sprite = sprts[ids[i]];
            img.transform.DOLocalMove(cardEndPos[0], 0f);
            img.transform.SetAsFirstSibling();
        }
        LoadData();
        txts[6].text = App.listKeyText["SPIN_REMIND"]; //"Ấn quay để bắt đầu game";
        ShowNotiTotu();
    }
    private void OnEnable()
    {
        CPlayer.potchangedMiniPocker += MiniPockerPotChanged;
        buttonSpin.interactable = true;
        spinMiniPocker.startSpin.AddListener(StarSpin);
        spinMiniPocker.openShowValue += ShowValueEvent;
    }

    private void ShowNotiTotu()
    {
        rtfs[2].transform.localScale = 5 * Vector2.one;
        rtfs[2].gameObject.SetActive(true);
        rtfs[2].DOScale(1f, .5f).SetEase(Ease.OutElastic);
    }
    private void LoadData()
    {
        var req_INFO = new OutBounMessage("MINIPOKER.GET_INFO");
        req_INFO.addHead();
        req_INFO.writeInt(100);
        App.ws.send(req_INFO.getReq(), delegate (InBoundMessage res_INFO)
        {
            int count = res_INFO.readByte();
            for (int i = 0; i < count; i++)
            {
                int bet = res_INFO.readInt();
                if (i < 8)
                {
                    numList[6 + i] = bet;
                    txts[0 + i].text = bet % 1000 == 0 ? (bet / 1000).ToString() + "K" : App.formatMoneyK(bet);
                }
            }

            int potAmount = res_INFO.readInt();
            count = res_INFO.readInt();
            for (int i = 0; i < count; i++)
            {
                int rsId = res_INFO.readInt();
                string rsName = res_INFO.readString();
                int rsBet = res_INFO.readInt();
                if (i > 0 && i < 9)
                {
                    //txts[6 + i].text = rsName.ToUpper();
                    //txts[14 + i].text = "x" + rsBet;
                }

            }
        });

    }
    public void ChangeAutoSpin()
    {
        /*
        if (numList[2] > 0)
        {
            ShowNoti("Không thể thực hiện khi đang quay");
            return;

        }
        */
        SoundManager.instance.PlayUISound(SoundFX.MNP_SPIN_AUTO);
        if (numList[1] == 0)
        {
            numList[1] = 1;
            btns[3].image.sprite = sprts[55];
            btns[3].image.color = new Color32(209, 0, 165, 255);
            //btns[3].GetComponentInChildren<Text>().color = new Color32(255,255,255, 255);
            if (numList[2] < 0)
                spinMiniPocker.Spin();
            return;
        }
        if (numList[1] == 1)
        {
            numList[1] = 0;
            btns[3].image.sprite = sprts[54];
             btns[3].image.color = new Color32(255,255,255,255);
            //btns[3].GetComponentInChildren<Text>().color = new Color32(148, 0, 0, 255);
        }
    }
    public void changeBet(int id)
    {
        if (numList[2] > -1)
        {
            return;
        }
        
        if (id == numList[0])
            return;
        if (numList[0] != -1)
        {
            btns[numList[0]].image.sprite = sprts[53];
            //btns[numList[0]].interactable = true;
            btns[numList[0]].gameObject.GetComponentInChildren<Text>().color = new Color32(255, 251, 98, 255);
        }
        numList[0] = id;
        btns[numList[0]].image.sprite = sprts[52];
        btns[numList[0]].gameObject.GetComponentInChildren<Text>().color = new Color32(102,51,20,255);
        //btns[numList[0]].interactable = false;

        //if (betData.ContainsKey(numList[6 + id]))
        //    txts[3].text = App.FormatMoney(betData[numList[6 + id]]);         
    }

    private IEnumerator ChangeToDefault()
    {
        animtors[0].SetBool("spin", false);
        animtors[0].SetBool("respin", true);
        yield return new WaitForSeconds(.5f);
        animtors[0].SetBool("respin", false);
        boolLs[0] = true;
        if (numList[1] == 1)
            ChangeAutoSpin();
    }
    public void StarSpin()
    {
        rtfs[1].gameObject.SetActive(false);
        buttonSpin.interactable = false;
        GetDataServer();
    }
    private void GetDataServer()
    {
        if (rtfs[2].gameObject.activeSelf)
            StartCoroutine(HideSmt(rtfs[2].gameObject, 0f));
        //set data for new spin
        numList[4] = 0;


        var req_SPIN = new OutBounMessage("MINIPOKER.START");
        req_SPIN.addHead();
        req_SPIN.writeInt(numList[6 + numList[0]]);         //bet id
        App.ws.send(req_SPIN.getReq(), delegate (InBoundMessage res_SPIN)
        {
            boolLs[0] = false;
            List<int> ids = res_SPIN.readBytes();

            foreach (int item in ids)
            {
                
                detecHumanCard(item);
            }
            StartCoroutine(StopSpin(ids, res_SPIN));
        });
    }
    private IEnumerator StopSpin(List<int> ids,InBoundMessage res)
    {
        if (togs[0].isOn)
        {
            spinMiniPocker.StopSpin(ids, res);
        }
        else
        {
            yield return new WaitForSeconds(1.5f);
            spinMiniPocker.StopSpin(ids, res);
        }
    }
    public void ShowValueEvent(InBoundMessage res)
    {
        StartCoroutine(ShowValue(res));
    }
    public IEnumerator ShowValue(InBoundMessage res_SPIN = null)
    {
        if (res_SPIN != null)
        {
            int rsId = res_SPIN.readInt();
            string rsName = res_SPIN.readString();
            bool isPotBreak = res_SPIN.readByte() == 1;
            bool isBigWin = res_SPIN.readByte() == 1;
            numList[4] = res_SPIN.readInt();            //won balance
            long spinId = res_SPIN.readLong();

           // Debug.LogError("rsId " + rsId.ToString() + " || rsName " + rsName + " || won balance " + numList[4]);

            if (isPotBreak)
            {
                if (numList[1] == 1)
                {
                    ChangeAutoSpin();
                }
                ShowPotBreak();
                yield return new WaitForSeconds(3f);
            }

            if (isBigWin)
            {
                ShowBigWin();
                yield return new WaitForSeconds(3f);
            }
            if (!isBigWin && !isPotBreak && numList[4] > 0)
            {
                txts[6].text = rsName;
                ShowWin();
                yield return new WaitForSeconds(2f);
                //Invoke("ChangeBalanceIfWin", 1.5f);
            }



            //Invoke("Spin", numList[4] == 0 ? 1f : 2f);
        }

        if (numList[1] != 1)
        {
            animtors[0].SetBool("spin", false);
            animtors[0].SetBool("respin", true);
            yield return new WaitForSeconds(.5f);
            animtors[0].SetBool("respin", false);
            spinMiniPocker.isPlaying = false;
            boolLs[0] = true;
        }
        else
        {
            boolLs[0] = true;
            spinMiniPocker.isPlaying = false;
            spinMiniPocker.Spin();
        }
        buttonSpin.interactable = true;
    }

    /*
    public void Spin()
    {
        if(CPlayer.chipBalance < numList[6 + numList[0]] && numList[1] == 1)
        {
            StartCoroutine(ChangeToDefault());
            return;
        }
        if (!boolLs[0])
        {
            return;
        }

        if (numList[2] > -1)
        {
            return;

        }

      

        if (numList[1] == 1 && numList[2] > -1)
        {
            return;
        }
        
        if (rtfs[2].gameObject.activeSelf)
            StartCoroutine(HideSmt(rtfs[2].gameObject, 0f));
        //set data for new spin
        numList[4] = 0;


        var req_SPIN = new OutBounMessage("MINIPOKER.START");
        req_SPIN.addHead();
        req_SPIN.writeInt(numList[6 + numList[0]]);         //bet id
        App.ws.send(req_SPIN.getReq(), delegate (InBoundMessage res_SPIN)
        {
            boolLs[0] = false;
            List<int> ids = res_SPIN.readBytes();
            foreach (int item in ids)
            {
                detecHumanCard(item);
            }
            animtors[0].SetBool("spin",true);
            Spin(ids, res_SPIN);
        });

    }

    private void Spin(List<int> ids, InBoundMessage res = null)
    {
        numList[2] = 4;
        for (int i = 0; i < 5; i++)
        {
            int tmp = i;
            StartCoroutine(_Spin(ids[tmp], i, i == 0 ? 0 : i * .1f, res));
        }
    }

    private IEnumerator _Spin(int targetId, int j, float waitFor, InBoundMessage res_SPIN = null)
    {
        yield return new WaitForSeconds(waitFor);
        float time = 0;
        if (togs[0].isOn)
        {
            Image img = Instantiate(imgs[j], imgs[j].transform.parent, false);
            img.sprite = sprts[targetId];
            gojsToDel2.Add(img.gameObject);
            time = .06f;
            img.transform.DOLocalMove(cardEndPos[0], time);
            yield return new WaitForSeconds(time);
        }
        else
        {
            for (int i = 0; i < 20; i++)
            {
                Image img = Instantiate(imgs[j], imgs[j].transform.parent, false);
                if (i == 19)
                {
                    img.sprite = sprts[targetId];
                    gojsToDel2.Add(img.gameObject);
                }
                else
                {
                    img.sprite = sprts[UnityEngine.Random.Range(0, 51)];
                    gojsToDel.Add(img.gameObject);
                }

                caculate time per spin - slow when start or end of a spin
                if (numList[1] == 1)
                {
                    time = (i - 15) * (i - 15) * .0006f;
                    if (time < .08f)
                        time = .08f;
                }
                else
                {
                    time = (i - 15) * (i - 15) * .0006f;
                    if (time < .08f)
                        time = .08f;
                }
                img.transform.DOLocalMove(cardEndPos[0], time);
                yield return new WaitForSeconds(time);
            }
        }
        If it's the last spin - end of a spin
        numList[2]--;
        if (numList[2] < 0)
        {
            foreach (GameObject item in gojsToDel)
            {
                Destroy(item);
            }

            gojsToDel.Clear();

            foreach (GameObject item in gojsToDel2)
            {
                gojsToDel.Add(item);
            }

            if (res_SPIN != null)
            {
                gojsToDel2.Clear();

                int rsId = res_SPIN.readInt();
                string rsName = res_SPIN.readString();
                bool isPotBreak = res_SPIN.readByte() == 1;
                bool isBigWin = res_SPIN.readByte() == 1;
                numList[4] = res_SPIN.readInt();            //won balance
                long spinId = res_SPIN.readLong();

                App.trace("rsId " + rsId.ToString() + " || rsName " + rsName+ " || won balance "+ numList[4], "yellow");

                if (isPotBreak)
                {
                    ShowPotBreak();
                    yield return new WaitForSeconds(3f);
                }

                if (isBigWin)
                {
                    ShowBigWin();
                    yield return new WaitForSeconds(3f);
                }
                if (!isBigWin && !isPotBreak && numList[4] > 0)
                {
                    txts[6].text = rsName;
                    ShowWin();
                    yield return new WaitForSeconds(2f);
                    Invoke("ChangeBalanceIfWin", 1.5f);
                }



                    Invoke("Spin", numList[4] == 0 ? 1f : 2f);
            }
            
            if (numList[1] != 1)
            {
                animtors[0].SetBool("spin", false);
                animtors[0].SetBool("respin", true);
                yield return new WaitForSeconds(.5f);               
                animtors[0].SetBool("respin", false);
                boolLs[0] = true;
            }
            else
            {
                boolLs[0] = true;
                Spin();
            }
          
          
        }
    }
    */
    private void ShowBigWin()
    {
        SoundManager.instance.PlayUISound(SoundFX.BIG_WIN + "_" + UnityEngine.Random.Range(1,4));
        rtfs[0].transform.localScale = 5 * Vector2.one;
        rtfs[0].gameObject.SetActive(true);
        rtfs[0].DOScale(1f, .5f).SetEase(Ease.OutElastic).OnComplete(() => {
            if (threads[1] != null)
                StopCoroutine(threads[1]);
            threads[1] = TweenNum(txts[4], 0, numList[4]);
            StartCoroutine(threads[1]);
        });
        StartCoroutine(HideSmt(rtfs[0].gameObject, 3f));
    }

    private void ShowWin()
    {
        txts[7].text = "";
        txts[7].text = App.formatMoney(numList[4].ToString());
        rtfs[2].transform.localScale = 5 * Vector2.one;
        rtfs[2].gameObject.SetActive(true);
        rtfs[2].DOScale(1f, .5f).SetEase(Ease.OutElastic).OnComplete(() =>
        {
            /*
            if (threads[1] != null)
                StopCoroutine(threads[1]);
            threads[1] = TweenNum(txts[7], 0, numList[4], 2f);
            StartCoroutine(threads[1]);
            */
        });
        StartCoroutine(HideSmt(rtfs[2].gameObject, 2f));
    }

    private void ChangeBalanceIfWin()
    {
        if (threads[3] != null)
            StopCoroutine(threads[3]);
        threads[3] = TweenNum(txts[1], (int)CPlayer.preChipBalance, (int)CPlayer.chipBalance, 2f, 1f);
        StartCoroutine(threads[3]);
    }

    private void ShowPotBreak()
    {
        rtfs[1].transform.localScale = 5 * Vector2.one;
        rtfs[1].gameObject.SetActive(true);
        rtfs[1].DOScale(1f, .5f).SetEase(Ease.OutElastic).OnComplete(() => {
            if (threads[1] != null)
                StopCoroutine(threads[1]);
            threads[1] = TweenNum(txts[5], 0, numList[4]);
            StartCoroutine(threads[1]);
        });
        //StartCoroutine(HideSmt(rtfs[1].gameObject, 3f));
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

    private IEnumerator HideSmt(GameObject obj, float timeToWait)
    {
        yield return new WaitForSeconds(timeToWait);
        if (obj != null)
        {
            obj.SetActive(false);
        }
    }
    private void detecHumanCard(int id)
    {
        string rs = "";
        int cardHigh = 0; // A 2 .. 10 J Q K
        int cardType = 0; // Chất của quân bài
        cardType = (int)Mathf.Floor(id / 13);
        cardHigh = (int)Mathf.Floor(id % 13) + 1;

        switch (cardHigh)
        {
            case 1:
                rs = "A" + cardTypes[cardType] + "|" + rs;
                break;
            case 11:
                rs = "J" + cardTypes[cardType] + "|" + rs;
                break;
            case 12:
                rs = "Q" + cardTypes[cardType] + "|" + rs;
                break;
            case 13:
                rs = "K" + cardTypes[cardType] + "|" + rs;
                break;
            default:
                rs = cardHigh + cardTypes[cardType] + "|" + rs;
                break;

        }
       
    }

    public void ShowHis(bool isShow)
    {
        if (!isShow)
        {
            gojs[0].SetActive(false);
            return;

        }
        gojs[0].SetActive(true);
        numList[3] = 0;
        LoadHis();
    }
    public void ShowHelp(bool isShow)
    {
        if (!isShow)
        {
            gojs[2].SetActive(false);
            return;

        }
        gojs[2].SetActive(true);
    }

    private void LoadHis()
    {
        foreach (Transform rtf in gojs[1].transform.parent)       //Delete exits element before
        {
            if (rtf.gameObject.name != gojs[1].name)
            {
                Destroy(rtf.gameObject);
            }
        }


        OutBounMessage req_HIS = new OutBounMessage("MATCH.HISTORY");
        req_HIS.addHead();
        req_HIS.writeString("minipoker");         //game name
        req_HIS.writeByte(1);                       //page index
        req_HIS.writeString("");                    //from date
        req_HIS.writeString("");                    //to date
        App.ws.send(req_HIS.getReq(), delegate (InBoundMessage res_HIS)
        {
            int count = res_HIS.readByte();
            for (int i = 0; i < count; i++)
            {
                long index = res_HIS.readLong();
                string time = res_HIS.readString();
                string game = res_HIS.readString();
                string bet = res_HIS.readString();
                string change = res_HIS.readString();
                string balance = res_HIS.readString();
                GameObject goj = Instantiate(gojs[1], gojs[1].transform.parent, false);
                Text[] txtArr = goj.GetComponentsInChildren<Text>();
                txtArr[0].text = index.ToString();
                txtArr[1].text = time;
                txtArr[2].text = App.FormatMoney(int.Parse(bet));
                txtArr[3].text = /*App.FormatMoney(int.Parse(*/change/*))*/;
                if (balance == "")
                    balance = "0";
                txtArr[4].text = /*App.FormatMoney(int.Parse(*/balance/*))*/;
        goj.SetActive(true);
                /*
                 
                */
            }
        });
    }
    public void MiniPockerPotChanged()
    {/*
        InBoundMessage res = CPlayer.res_pot;
        int count = res.readByte();
        int bet = res.readInt();
        int value = res.readInt();

        if (!betData.ContainsKey(bet))
            betData.Add(bet, value);
        else
            betData[bet] = value;

        //If pot is broken
        if (value < numList[5])
        {
            numList[5] = value;
            txts[6].text = string.Format("{0:0,0}", value);
        }
        if (true)
        {
            if (threads[2] != null)
                StopCoroutine(threads[2]);
            threads[2] = TweenNum(txts[6], numList[5], value, 1f, 1f);
            numList[5] = value;
            StartCoroutine(threads[2]);
        }
        */
        InBoundMessage res = CPlayer.res_potMiniGameMiniPocker;
        int count = res.readByte();
        App.trace("[RECV] POT_CHANGED FROM MINIPOKER");

        for (int i = 0; i < count; i++)
        {
            string gameId = res.readString();
            Debug.Log(gameId + "GAME ID");
            if (gameId == "minipoker")
            {
                int count0 = res.readByte();
                for (int j = 0; j < count0; j++)
                {
                    int bet = res.readInt();
                    int value = res.readInt();

                    if (!betData.ContainsKey(bet))
                        betData.Add(bet, value);
                    else
                        betData[bet] = value;
                    //App.trace("gameID = " + gameId + "|bet = " + bet + "|val = " + value+ "|numList[6 + numList[0]] "+ numList[6 + numList[0]], "yellow");
                    //If pot is broken
                    if (bet == numList[6 + numList[0]])
                    {
                        if (value < betData[numList[6 + numList[0]]])
                        {
                            txts[3].text = string.Format("{0:0,0}", value);
                        }
                        else
                        {
                            if (threads[2] != null)
                                StopCoroutine(threads[2]);
                            threads[2] = TweenNum(txts[3], numList[5], value, 1f, 1f);
                            StartCoroutine(threads[2]);
                        }
                        numList[5] = value;
                    }
                }
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
    public void Close()
    {
        if (isEnoughMoney)
        {
            StartCoroutine(closeGameWhenEnoughMoney());
            return;
        }
        if (spinMiniPocker.isPlaying || numList[1] ==1 || !boolLs[0])
        {
            return;
        }
        if (threads[2] != null)
            StopCoroutine(threads[2]);
        rtfs[1].gameObject.SetActive(false);
        CPlayer.potchangedMiniPocker -= MiniPockerPotChanged;
        spinMiniPocker.startSpin.RemoveListener(StarSpin);
        spinMiniPocker.openShowValue -= ShowValueEvent;
        gameObject.SetActive(false);
    }
    private bool isEnoughMoney;
    public void EnoughMoney(string gamecode, int gameState)
    {
        if (gamecode.Contains("minipoker")) {
            isEnoughMoney = true;
           
        }
    }

    private IEnumerator closeGameWhenEnoughMoney()
    {
        spinMiniPocker.StopSpinEnoughMoney();
        animtors[0].SetBool("spin", false);
        animtors[0].SetBool("respin", true);
        animtors[0].SetBool("respin", false);
        if (numList[1] == 1){
            ChangeAutoSpin();
        }
        yield return new WaitForSeconds(.2f);
        spinMiniPocker.isPlaying = false;
        boolLs[0] = true;
        CPlayer.potchangedMiniPocker -= MiniPockerPotChanged;
        spinMiniPocker.startSpin.RemoveListener(StarSpin);
        spinMiniPocker.openShowValue -= ShowValueEvent;
        gameObject.SetActive(false);
    }
}
