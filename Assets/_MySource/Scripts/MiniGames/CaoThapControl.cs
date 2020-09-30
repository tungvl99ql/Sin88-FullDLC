using DG.Tweening;
using Core.Server.Api;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CaoThapControl : MonoBehaviour {

    /// <summary>
    /// 0-51: card|52 :spin|53-54: btnbet|55: AAA
    /// </summary>
    public Sprite[] sprts;

    /// <summary>
    /// 0: spin|1-3: AÂA
    /// </summary>
    public Image[] imgs;

    /// <summary>
    /// 0: Id|1: time|2: pot|3-5: guide-bet|6-8: bet|9: prize|10: win|11: up|12: down|13: noti|14: humanCard
    /// |15: win-OR-break-text
    /// </summary>
    public Text[] txts;

    /// <summary>
    /// 0: hisPanel|1: his-ele|2: guide|3: overlay
    /// </summary>
    public GameObject[] gojs;

    //public Toggle[] togs;

    /// <summary>
    /// 0: win
    /// </summary>
    public RectTransform[] rtfs;


    /// <summary>
    /// 0: Grey|1: White
    /// </summary>
    public Color[] colors;

    /// <summary>
    /// 0: new|1: up|2 : down|3: spin|4-6: btnbet
    /// </summary>
    public Button[] btns;

    /// <summary>
    /// 
    /// </summary>
    public int[] test;
    

    /// <summary>
    /// 0: currbet|1-3: betData|4: currTogId|5: currPot|6: endAllow-if-no-choose|7: allow-enable-new-game
    /// </summary>
    private int[] numList = new int[8];

    /// <summary>
    /// 0: endGame|1: timeCount|2: noti|3: pot-change
    /// </summary>
    private IEnumerator[] tweenNum = new IEnumerator[4];

    /// <summary>
    /// 0: inThread
    /// </summary>
    private bool[] bools = new bool[1];

    //10, J♥, 2
    private string[] humanCardType = { "♥", "♦", "♣", "♠" };

    private void OnEnable()
    {
        transform.SetSiblingIndex(22);
        //SET DEFAULT
        SetDefault();

        GetInfo();

        CPlayer.potchangedCaoThap += CaoThapPotChange;

        SetHandler();

        //Invoke("Test", 3f);
    }

    public void Test()
    {
        //LoadingControl.instance.showNotifyAll("longtnh", "CAO THẤP", 10000, 123456);
    }

    public void GetInfo()
    {
        var req = new OutBounMessage("CAOTHAP.GET_INFO");
        req.addHead();
        req.writeInt(1000);         //BetAmount
        App.ws.send(req.getReq(), delegate (InBoundMessage res)
        {
            App.trace("<color=red>[RECV]</color> CAOTHAP.GET_INFO");
            int count = res.readByte();
            for (int i = 0; i < count; i++)
            {
                int bet = res.readInt();
                numList[1 + i] = bet;
                txts[6 + i].text = bet / 1000 + "K";
            }
            int potAmount = res.readInt();
            txts[2].text = App.FormatMoney(potAmount);

            if(res.readByte() == 1)         //Has playing thread
            {
                //Set bnts active or inactive
                btns[3].interactable = false;
                btns[1].interactable = btns[2].interactable = btns[0].interactable = true;
                btns[1].image.color = btns[2].image.color = btns[0].image.color = colors[1];
                bools[0] = true;

                imgs[0].sprite = sprts[res.readByte()];         //cardId
                txts[9].text = App.FormatMoney(res.readInt());         //prize
                txts[11].text = App.FormatMoney(res.readInt());          //prize up
                txts[12].text = App.FormatMoney(res.readInt());           //prize down
                int aC = res.readByte();          // count a
                numList[0] = res.readInt();           // curr bet
                int time = res.readInt();         //time count
                //App.trace("time = " + time + "currBet = " + numList[0]);
                for (int i = 0; i < 3; i++)
                {
                    if(numList[1 + i] == numList[0])
                    {
                        btns[4 + i].image.sprite = sprts[54];
                        break;
                    }
                }
                //App.trace("<color=red>time = </color>"  +time);

                if (time > 0)
                {
                    if (tweenNum[1] != null)
                        StopCoroutine(tweenNum[1]);
                    tweenNum[1] = TimeCount(time);
                    StartCoroutine(tweenNum[1]);
                }
                    
            }
            else
            {
                numList[0] = numList[1];            //by default, currbet is min
                btns[4].image.sprite = sprts[54];
                SetDefault();
            }

        });
    }

    public void Spin(int upOrDOwn)
    {
        

        
        var req = new OutBounMessage("CAOTHAP.START");
        req.addHead();
        //req.writeInt(numList[0]);           //bet
        req.writeInt(numList[0]);           //bet
        //App.trace("Phắc = " + numList[0]);
        req.writeByte(upOrDOwn);        //up: 1|0: down
        App.ws.send(req.getReq(), delegate (InBoundMessage res)
        {
            btns[1].interactable = btns[2].interactable = false;
            btns[1].image.color = btns[2].image.color = colors[0];
            gojs[3].SetActive(true);
            btns[3].interactable = false;

            
            

            //TEST
            //ACardNum = 1;
            //id = 0;
            ////

            StartCoroutine(SwapCard(res));

            

            

            

        });
        
    }

    private IEnumerator SwapCard( InBoundMessage res)
    {
        List<int> ACardLs = new List<int>();

        int targetId = res.readByte();
        int prize = res.readInt();
        int up = res.readInt();
        int down = res.readInt();
        int ACardNum = res.readByte();
        //App.trace("La bai so "+targetId,"red");
        for (int u = 0; u < ACardNum; u++)
        {
            ACardLs.Add(res.readInt());
            //imgs[ACardNum].sprite = sprts[res.readInt()];
        }
        List<int> ids = res.readBytes();
        bool isStop = res.readByte() == 1;
        int time = res.readInt();
        //App.trace("<color=red>[RECV]</color>cardId = " + id + "|prize = " + prize + "|isStop = " + isStop + "|up = " + up + "|down = " + down + "|Acount = " + ACardNum + "|countTime = " + time + "|cardNUm = " + randomCards.Count);

        int i = 0;
        int k = ids.Count - 1;
        while(i < 100)
        {
            imgs[0].sprite = sprts[ids[k]];
            i++;
            k--;
            if (k < 0)
                k = ids.Count - 1;
            yield return new WaitForSeconds(.01f);
        }
        string t = txts[14].text;
        if ( t.Length > 0)
        {
            t += ", " + getHumanCard(targetId);
        }
        else
            t = getHumanCard(targetId);
        if (t.Length > 16)
            t = t.Substring(1);
        //if (test[0] > 0)
        //imgs[test[0]].sprite = sprts[targetId];
        //if (ACardNum > 0)
        //imgs[ACardNum].sprite = sprts[targetId];
        for (int m = 0; m < ACardNum; m++)
        {
            imgs[1 + m].sprite = sprts[ACardLs[m]];
        }


        if (prize > 0)
            StartCoroutine(TweenNum(txts[9], 0, prize, 3, 1.5f));
        else
        {
            //App.trace("Phắc diu");
            StartCoroutine(TweenNum(txts[9], 0, 0, 3f, 1f));
        }
            


        txts[11].text = App.FormatMoney(up);
        txts[12].text = App.FormatMoney(down);

        txts[14].text = t;
        imgs[0].sprite = sprts[targetId];
        numList[7]++;
        if (numList[7] > 1)
        {
            btns[0].interactable = true;
            btns[0].image.color = colors[1];
        }
        

        if (isStop)
        {
            if (tweenNum[2] != null)
                StopCoroutine(tweenNum[2]);
            tweenNum[2] = ShowNoti("Bạn đã thua. Chúc bạn may mắn lần sau.");
            StartCoroutine(tweenNum[2]);
            yield return new WaitForSeconds(2f);
            
            SetDefault();
        }else 
        if (isStop == false && time > 0)
        {
            if (tweenNum[1] != null)
                StopCoroutine(tweenNum[1]);
            tweenNum[1] = TimeCount(time);
            bools[0] = true;
            StartCoroutine(tweenNum[1]);//Set btn up - down
            btns[1].interactable = btns[2].interactable = true;
            btns[1].image.color = btns[2].image.color = colors[1];
        }
        gojs[3].SetActive(false);


    }

    public void OpenHistory(bool isShow)
    {
        if (!isShow)
        {
            gojs[0].SetActive(false);
            return;
        }
        gojs[0].SetActive(true);
        foreach (Transform rtf in gojs[1].transform.parent)       //Delete exits element before
        {
            if (rtf.gameObject.name != gojs[1].name)
            {
                Destroy(rtf.gameObject);
            }
        }
        OutBounMessage req_HIS = new OutBounMessage("MATCH.HISTORY");
        req_HIS.addHead();
        req_HIS.writeString("caothap");         //game name
        req_HIS.writeByte(1);                       //page index
        req_HIS.writeString("");                    //from date
        req_HIS.writeString("");                    //to date
        App.ws.send(req_HIS.getReq(), delegate (InBoundMessage res_HIS)
        {
            int count = res_HIS.readByte();
            //App.trace("Count = " + count);
            for (int i = 0; i < count; i++)
            {
                long index = res_HIS.readLong();
                string time = res_HIS.readStrings();
                string game = res_HIS.readStrings();
                string bet = res_HIS.readString();
                string change = res_HIS.readString();
                string balance = res_HIS.readString();

                GameObject goj = Instantiate(gojs[1], gojs[1].transform.parent, false);
                Text[] txtArr = goj.GetComponentsInChildren<Text>();
                txtArr[0].text = index.ToString();
                txtArr[1].text = time;
                txtArr[2].text = App.FormatMoney(change);
                txtArr[3].text = App.FormatMoney(balance);
                goj.SetActive(true);
            }

        });

    }

    public void OpenGuide(bool isShow)
    {
        if (!isShow)
        {
            gojs[2].SetActive(false);
            return;
        }

        gojs[2].SetActive(true);
        for (int i = 0; i < 3; i++)
        {
            txts[3 + i].text = numList[1 + i] / 1000 + "K";
        }
    }

    public void ChangeBet(int id)
    {

        if (bools[0] == true)
        {
            if (tweenNum[2] != null)
                StopCoroutine(tweenNum[2]);
            tweenNum[2] = ShowNoti("Không thể đổi mức cược khi phiên chưa kết thúc");
            StartCoroutine(tweenNum[2]);
            return;
        }

        if (numList[4] == id)
            return;


        //App.trace("CLICK ID " + id);
        btns[4 + numList[4]].image.sprite = sprts[53];
        numList[4] = id;
        btns[4 + id].image.sprite = sprts[54];

        numList[0] = numList[1 + id];
        //App.trace(numList[0], "green");

        var req = new OutBounMessage("CAOTHAP.GET_INFO");
        req.addHead();
        req.writeInt(numList[0]);         //BetAmount
        App.ws.send(req.getReq(), delegate (InBoundMessage res) {
            //App.trace("<color=red>[RECV]</color> CAOTHAP.GET_INFO");
            int count = res.readByte();
            for (int i = 0; i < count; i++)
            {
                int bet = res.readInt();
                numList[1 + i] = bet;
                txts[6 + i].text = bet / 1000 + "K";
            }
            int potAmount = res.readInt();

            ///Prevent text jump
            if (tweenNum[3] != null)
                StopCoroutine(tweenNum[3]);

            txts[2].text = App.FormatMoney(potAmount);
        });


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

    public void Close()
    {
        if (bools[0] == true)
        {
            if (tweenNum[2] != null)
                StopCoroutine(tweenNum[2]);
            tweenNum[2] = ShowNoti("Không thể đóng khi chưa kết thúc phiên");
            StartCoroutine(tweenNum[2]);
            return;
        }
        for (int i = 0; i < tweenNum.Length; i++)
        {
            if(tweenNum[i] != null)
                StopCoroutine(tweenNum[i]);
        }

        //set bet btns to interact
        for (int i = 0; i < 3; i++)
        {
            btns[4 + i].image.sprite = sprts[53];
        }
        numList[4] = 0;
        CPlayer.potchangedCaoThap -= CaoThapPotChange;

        var req = new OutBounMessage("CAOTHAP.END_GAME");
        req.addHead();
        App.ws.delHandler(req.getReq());

        gameObject.SetActive(false);
    }

    public void EndGame()
    {
        var req = new OutBounMessage("CAOTHAP.END_GAME");
        req.addHead();
        App.ws.send(req.getReq(), delegate (InBoundMessage res)
        {
            App.trace("[RECV] CAOTHAP.END_GAME","yellow");
            int prize = res.readInt();
            bool isPotBreak = res.readByte() == 1;          //1: true
            long threadId = res.readLong();            // id van choi
            bool isBigWin = res.readByte() == 1;         //1 = thang lon
            //App.trace("ID ván chơi " + threadId);
            txts[0].text = "#" + threadId.ToString();
            //isBigWin = true;
            if (isBigWin)
            {
                txts[15].text = "THẮNG LỚN";
                StartCoroutine(ShowRs(prize));
                SetDefault();
                return;
            }
            //isPotBreak = true;
            if (isPotBreak)
            {
                txts[15].text = "NỔ HŨ";
                StartCoroutine(ShowRs(prize));
                SetDefault();
                return;
            }
            txts[15].text = "";
            if (prize > 0)
            {
                StartCoroutine(ShowRs(prize));
            }
            SetDefault();

        });
    }

    private void SetHandler()
    {
        var req = new OutBounMessage("CAOTHAP.END_GAME");
        req.addHead();
        App.ws.sendHandler(req.getReq(), delegate (InBoundMessage res)
        {
            App.trace("[RECV] CAOTHAP.END_GAME)BY_HAND");
            res.readByte();
            int prize = res.readInt();
            bool isPotBreak = res.readByte() == 1;          //1: true
            long threadId = res.readLong();            // id van choi
            bool isBigWin = res.readByte() == 1;         //1 = thang lon
            //App.trace("prize = " + prize, "red");
            //App.trace("ID ván chơi handler "+threadId + " || prize "+prize ,"yellow");
            txts[0].text = "#" + threadId.ToString();
            //isBigWin = true;
            if (isBigWin)
            {
                txts[15].text = "THẮNG LỚN";
                StartCoroutine(ShowRs(prize));
                //SetDefault();
                return;
            }
            //isPotBreak = true;
            if (isPotBreak)
            {
                txts[15].text = "NỔ HŨ";
                StartCoroutine(ShowRs(prize));
                //SetDefault();
                return;
            }
            txts[15].text = "";
            if (prize > 0)
            {
                StartCoroutine(ShowRs(prize));
            }
            //SetDefault(true);

        });
    }

    private IEnumerator ShowRs(int prize)
    {
        txts[10].text = "0";
        rtfs[0].transform.localScale = 5 * Vector2.one;
        
        if (tweenNum[0] != null)
            StopCoroutine(tweenNum[0]);
        tweenNum[0] = TweenNum(txts[10], 0, (int)prize);
        StartCoroutine(tweenNum[0]);
        rtfs[0].parent.gameObject.SetActive(true);
        rtfs[0].DOScale(1f, .5f).SetEase(Ease.OutElastic).OnComplete(() => {
            
            txts[10].text = string.Format("{0:0,0}", prize);
        });
        yield return new WaitForSeconds(3f);
        rtfs[0].parent.gameObject.SetActive(false);
        bools[0] = false;

        txts[9].text = 0.ToString();
        txts[15].text = "";
    }

    private IEnumerator TimeCount(int time)
    {
        while (time > 0)
        {
            txts[1].text = (time / 60).ToString("00") + ":" + (time % 60).ToString("00");
            time--;
            yield return new WaitForSeconds(1f);
        }
        if(numList[6] > 0)
        {
            EndGame();
        }
        else
        {
            numList[6]++;
            Spin(UnityEngine.Random.Range(0, 2));
        }

        
    }

    private void SetDefault(bool byHand =false)
    {
        //App.trace("PHắc", "red");
        //set default value
        bools[0] = false;
        if(byHand== false)
            txts[9].text = txts[11].text = txts[12].text = "0";
        txts[14].text = "";
        if (tweenNum[1] != null)
            StopCoroutine(tweenNum[1]);
        txts[1].text = "0:00";
        numList[6] = 0;
        numList[7] = 0;

        //set for new game btn
        btns[0].interactable = false;
        btns[0].image.color = colors[0];

        //set for up and down btn
        btns[1].interactable = btns[2].interactable = false;
        btns[1].image.color = btns[2].image.color = colors[0];

        //Set for spin btn
        btns[3].interactable = true;
        btns[3].image.sprite = sprts[52];
        gojs[3].SetActive(false);

        //set 3 AAA
        for (int i = 0; i < 3; i++)
        {
            imgs[1 + i].sprite = sprts[55];
        }
    }

    private IEnumerator ShowNoti(string t, float delay = 0)
    {
        if(delay > 0)
            yield return new WaitForSeconds(delay);
        if (tweenNum[1] != null)
            StopCoroutine(tweenNum[1]);
        txts[13].text = t;
        txts[13].transform.parent.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        txts[13].transform.parent.gameObject.SetActive(false);
    }

    private string getHumanCard(int id)
    {
        int i = (id % 13 + 1);
        string high = i.ToString();
        switch (i)
        {
            case 1:
                high = "A";
                break;
            case 11:
                high = "J";
                break;
            case 12:
                high = "Q";
                break;
            case 13:
                high = "K";
                break;
        }
        return high + humanCardType[Mathf.FloorToInt(id / 13)];
    }

    private void CaoThapPotChange()
    {
        InBoundMessage res = CPlayer.res_potMiniGameCaoThap;
        int count = res.readByte();
        for (int i = 0; i < count; i++)
        {
            string gameId = res.readString();
            if (gameId == "caothap")
            {
                int count0 = res.readByte();
                for (int j = 0; j < count0; j++)
                {
                    int bet = res.readInt();
                    int value = res.readInt();
                    if (bet == numList[0])
                    {
                        if (tweenNum[3] != null)
                            StopCoroutine(tweenNum[3]);
                        tweenNum[3] = TweenNum(txts[2], (int)App.formatMoneyBack(txts[2].text), value, 1f, 1f);
                        StartCoroutine(tweenNum[3]);
                        return;
                    }
                }
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
}
