using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using Core.Server.Api;

public class TXControl : MonoBehaviour
{
    [SerializeField]
    AudioSource soundEndTurn;
    /// <summary>
    /// 0: help|1: his|2: hisEle|3; kb1|4: kb2|5: tai|6: xiu|7: dices|8: smallTime
    /// |9: QuickChat|10: chat|11: tracPanel
    /// </summary>
    public GameObject[] gojs;
    public GameObject d_goPanelChat;
    public GameObject gojBowl;
    public RectTransform postionDefaultBowl;
    /// <summary>
    /// 0: taiBkbet|1: xiukbBet|2: myTaiBit|3: myXiuBet| 4: mainTimeCount|5: betOnTaiPlayerNum|6: betOnXiuPlayerNum
    /// |7: taiCurrAmount|8: xiuCurrAmount|9: currTurn|10: currState|11: myBalance|12: winBalance|13: smallTime
    /// |14: chatELe|15: lastThread|16: lastThreadRs
    /// </summary>
    public Text[] txts;

    /// <summary>
    /// 0: his-circle-ele|1: his-circle-curr|2-4: 3dices|5: trackSubCirlceEle|6: trackSubLine|7: trackEachCircle
    /// |8: trackEachLine
    /// </summary>
    public Image[] imgs;

    /// <summary>
    /// 0: his-tai-red|1: his-cirlce-xiu|2-7: dice|8-9: trackSubEleTai-Xiu
    /// |10-12: trackEachLineBlue-Pur-Red|13-15: trackCircleRed-Green-Pur
    /// </summary>
    public Sprite[] sprts;

    /// <summary>
    /// 0-2: dices
    /// </summary>
    public Transform[] tfs;


    public Animator[] anims;

    public ScrollRect scr;


    public InputField ipf;

    /// <summary>
    /// 0: currTaiXiuBetKb|1: currBetSide|3: myBalance|4: taiBetAmount|5: xiuBetAmount
    /// </summary>
    private int[] nums = new int[10];


    /// <summary>
    /// 0: Count down|1: mybalance|2: dices
    /// </summary>
    private IEnumerator[] threads;
    private List<string> tweens;
    private List<string> handlers;

    /// <summary>
    /// 0: currState
    /// </summary>
    private string[] strs;

    private string STRING_NGOAC_1 = "(";
    private string STRING_NGOAC_2 = ")";

    private void OnEnable()
    {
        MiniGameControl.instance.OnEnableTX();
        transform.SetSiblingIndex(LoadingControl.MAX_SIBLING_INDEX);
        strs = new string[5];
        threads = new IEnumerator[5];
        tweens = new List<string>();
        handlers = new List<string>();
        LoadData();
        //OpenChat(true);

        imageMolding.overrideSprite = spriterHand[isMolding ? 1 : 0];
        StartCoroutine(EleZoomInOut());
    }
    private void OnDisable()
    {
        StopCoroutine(EleZoomInOut());
    }
    private IEnumerator EleZoomInOut()
    {
        while (true)
        {
            imgs[1].transform.DOScale(1.4f, 0.2f).OnComplete(() => {
                imgs[1].transform.DOScale(0.8f, 0.2f).OnComplete(() => {

                });
            });
            yield return new WaitForSeconds(0.4f);
        }
    }
    #region //OTHER FEATURES

    public void OpenHelp(bool toShow)
    {
        if (toShow)
            gojs[0].SetActive(true);
        else
            gojs[0].SetActive(false);
    }

    public void OpenHis(bool toShow)
    {

        if (!toShow)
        {
            gojs[1].SetActive(false);
            return;
        }
        gojs[1].SetActive(true);

        foreach (Transform rtf in gojs[2].transform.parent)       //Delete exits element before
        {
            if (rtf.gameObject.name != gojs[2].name)
            {
                Destroy(rtf.gameObject);
            }
        }



        var req_HIS = new OutBounMessage("TAIXIU.GET_INFO");
        req_HIS.addHead();
        App.ws.send(req_HIS.getReq(), delegate (InBoundMessage res_HIS)
        {
            // App.trace("[RECV] TAIXIU.GET_INFO");
            //res_HIS.readByte();
            int count = res_HIS.readByte();
            for (int i = 0; i < count; i++)
            {

                //App.trace(res_HIS.readStrings() + "|" + res_HIS.readStrings() + "|" + res_HIS.readStrings() + "|" +
                //    res_HIS.readStrings() + "|" + res_HIS.readStrings() + "|" + res_HIS.readStrings() + "|" + res_HIS.readStrings() + "|" + res_HIS.readStrings());


                //#359792|2017-11-28 09:43:39|TÀI|TÀI|13|4,4,5|95|0

                GameObject goj = Instantiate(gojs[2], gojs[2].transform.parent, false);
                Text[] txtArr = goj.GetComponentsInChildren<Text>();
                txtArr[0].text = res_HIS.readStrings(); //curr thread - ok
                txtArr[1].text = res_HIS.readStrings(); //time - ok
                txtArr[4].text = res_HIS.readStrings();  //bet amount
                txtArr[2].text = res_HIS.readStrings(); //bet side
                txtArr[3].text = res_HIS.readStrings() + "=" + res_HIS.readStrings();
                txtArr[6].text = res_HIS.readStrings(); //recv amount
                txtArr[5].text = res_HIS.readStrings(); //return amount
                goj.SetActive(true);

            }
        });
    }


    public void OpenTrack(bool toShow)
    {
        if (!toShow)
        {
            foreach (Transform rtf in imgs[7].transform.parent)       //Delete exits element before
            {
                if (rtf.gameObject.name.Contains("Clone"))
                {
                    Destroy(rtf.gameObject);
                }
            }

            foreach (Transform rtf in imgs[5].transform.parent)       //Delete exits element before
            {
                if (rtf.gameObject.name.Contains("Clone"))
                {
                    Destroy(rtf.gameObject);
                }
            }
            gojs[11].SetActive(false);
            return;
        }

        //int[] ids = new int[] { 7, 4, 10, 18, 8, 12, 3, 6, 9, 15, 16, 11, 12,  14, 16 };



        gojs[11].SetActive(true);


        OutBounMessage req_track = new OutBounMessage("TAIXIU.REPORT");
        req_track.addHead();
        App.ws.send(req_track.getReq(), delegate (InBoundMessage res_track)
        {
            int count = res_track.readInt();
            // App.trace("[RECV] TAIXIU.REPORT " + count);
            List<int> subLs = new List<int>();
            Dictionary<int, List<int>> eachLs = new Dictionary<int, List<int>>();
            int lastThreadId = 0;
            for (int i = count - 1; i >= 0; i--)
            {
                int threadId = res_track.readInt();
                //App.trace(threadId, "red");
                if (i == count - 1)
                {

                    eachLs.Add(0, new List<int>() { res_track.readInt() });
                    eachLs.Add(1, new List<int>() { res_track.readInt() });
                    eachLs.Add(2, new List<int>() { res_track.readInt() });
                }
                else
                {
                    eachLs[0].Add(res_track.readInt());
                    eachLs[1].Add(res_track.readInt());
                    eachLs[2].Add(res_track.readInt());
                }
                subLs.Add(res_track.readInt());
                //if(i == count - 10)
                lastThreadId = threadId;
            }

            int lastId = subLs.Count - 1;

            txts[15].text = "Phiên gần nhất (#" + lastThreadId.ToString() + ")";

            txts[16].text = (subLs[lastId] > 10 ? "Tài" : "Xỉu") + subLs[lastId] + " (" + eachLs[0][lastId] + ", " + eachLs[1][lastId] + ", " + eachLs[2][lastId] + ")";

            subLs.Reverse();
            List<int> l1 = null;
            eachLs.TryGetValue(0, out l1);
            l1.Reverse();

            List<int> l2 = null;
            eachLs.TryGetValue(1, out l2);
            l2.Reverse();

            List<int> l3 = null;
            eachLs.TryGetValue(2, out l3);
            l3.Reverse();

            eachLs.Clear();
            eachLs.Add(0, l1);
            eachLs.Add(1, l2);
            eachLs.Add(2, l3);




            //Dictionary<int, List<int>> eachLsReversed = new Dictionary<int, List<int>>();
            //for (int i = eachLs.Count- 1; i >=0; i--)
            //{
            //    List<int> l = null;
            //    var value = eachLs.TryGetValue(0, out l);
            //    eachLsReversed.Add(value);

            //}



            DrawTrackLines(subLs, eachLs);
        });
    }
    public GameObject contentTrack1;
    public GameObject contentTrack2;
    public GameObject contentTrack0;
    public ScrollRect scrollRectTrack;
    private void DrawTrackLines(List<int> subIds, Dictionary<int, List<int>> eachIds)
    {
        {
            //SUBLINE
            //foreach (Transform rtf in imgs[5].transform.parent)       //Delete exits element before
            //{
            //    if (rtf.gameObject.name.Contains("Clone"))
            //    {
            //        Destroy(rtf.gameObject);
            //    }
            //}

            Vector2 firstPos = Vector2.zero;
            for (int i = 0; i < subIds.Count; i++)
            {
                Image img = Instantiate(imgs[5], imgs[5].transform.parent, false);
                img.sprite = sprts[subIds[i] > 10 ? 8 : 9];

                Vector2 secondPos = new Vector2(79 + i * 86.6f, 14f + (1f * (subIds[i] - 3) / 3f) * 39.6f);
                img.GetComponent<RectTransform>().anchoredPosition = secondPos;

                img.gameObject.SetActive(true);

                if (i > 0)
                {
                    //Draw line
                    Image line = Instantiate(imgs[6], imgs[6].transform.parent, false);
                    //line.sprite
                    RectTransform rtf = line.GetComponent<RectTransform>();
                    rtf.SetAsFirstSibling();
                    rtf.anchoredPosition = new Vector2(firstPos.x + 16, firstPos.y + 8);
                    rtf.localScale = new Vector2(Vector2.Distance(firstPos, secondPos) / 4, 1f);
                    rtf.right = secondPos - firstPos;
                    line.gameObject.SetActive(true);
                }
                firstPos = secondPos;
            }

        }


        {
            //EACHLINE
            //foreach (Transform rtf in imgs[7].transform.parent)       //Delete exits element before
            //{
            //    if (rtf.gameObject.name.Contains("Clone"))
            //    {
            //        Destroy(rtf.gameObject);
            //    }
            //}



            for (int ji = 0; ji < 3; ji++)
            {
                List<int> ids = eachIds[ji];
                Vector2 firstPos = Vector2.zero;
                for (int i = 0; i < ids.Count; i++)
                {
                    Image img = Instantiate(imgs[7], imgs[7].transform.parent, false);
                    img.sprite = sprts[13 + ji];

                    Vector2 secondPos = new Vector2(68.7f + i * 86.6f, 14f + (ids[i] - 1) * 39.6f);
                    img.GetComponent<RectTransform>().anchoredPosition = secondPos;

                    img.gameObject.SetActive(true);

                    if (i > 0)
                    {
                        //Draw line
                        Image line = Instantiate(imgs[8], imgs[8].transform.parent, false);
                        line.sprite = sprts[10 + ji];
                        RectTransform rtf = line.GetComponent<RectTransform>();
                        rtf.SetAsFirstSibling();
                        rtf.anchoredPosition = new Vector2(firstPos.x + 16, firstPos.y + 12);
                        rtf.localScale = new Vector2(Vector2.Distance(firstPos, secondPos) / 4, .5f);
                        rtf.right = secondPos - firstPos;
                        line.gameObject.SetActive(true);
                    }
                    firstPos = secondPos;
                }
            }
        }
        float x = contentTrack1.transform.GetChild(contentTrack1.transform.childCount - 1).transform.localPosition.x;
        if (x < contentTrack2.transform.GetChild(contentTrack2.transform.childCount - 1).transform.localPosition.x)
            x = contentTrack2.transform.GetChild(contentTrack2.transform.childCount - 1).transform.localPosition.x;

        contentTrack1.GetComponent<RectTransform>().sizeDelta = new Vector2(x + 50, contentTrack1.GetComponent<RectTransform>().sizeDelta.y);
        contentTrack2.GetComponent<RectTransform>().sizeDelta = new Vector2(x + 50, contentTrack2.GetComponent<RectTransform>().sizeDelta.y);
        //contentTrack1.GetComponent<RectTransform>().offsetMin = new Vector2(0, contentTrack1.GetComponent<RectTransform>().rect.height);
        //contentTrack2.GetComponent<RectTransform>().offsetMin = new Vector2(0, contentTrack2.GetComponent<RectTransform>().rect.height);
        /*contentTrack1.GetComponent<RectTransform>().localPosition = new Vector2(x + 45, contentTrack1.GetComponent<RectTransform>().localPosition.y);
        contentTrack2.GetComponent<RectTransform>().localPosition = new Vector2(x + 45, contentTrack2.GetComponent<RectTransform>().localPosition.y);
        */
        contentTrack0.GetComponent<RectTransform>().sizeDelta = new Vector2(x+50,contentTrack0.GetComponent<RectTransform>().rect.size.y);
        scrollRectTrack.horizontalNormalizedPosition = 0;
    }

    public void OpenChat(bool toShow)
    {
        /*
        if (!toShow)
        {
            gojs[10].SetActive(false);
            return;
        }



        foreach (Transform rtf in txts[14].transform.parent)       //Delete exits element before
        {
            if (rtf.gameObject.name.Contains("Clone"))
            {
                Destroy(rtf.gameObject);
            }
        }

        gojs[10].SetActive(true);

        for (int i = 0; i < 30; i++)
        {
            Text txt = Instantiate(txts[14], txts[14].transform.parent, false);
            txt.text = "<color=orange>Người chơi " + (i + 1) + "</color>: " + "CÁI NÀY SƠ VƠ CHƯA CÓ";
            txt.gameObject.SetActive(true);
        }

        if (threads[3] != null)
        {
            StopCoroutine(threads[3]);
        }
        threads[3] = TweenScroll();
        StartCoroutine(threads[3]);
        */
        //if (!toShow)
        //{
        //    LoadingControl.instance.CloseChat();
        //    return;
        //}
        //LoadingControl.instance.OpenChat();


    }

    //public void D_SetActivePanelChat()
    //{
    //    d_goPanelChat.SetActive(d_goPanelChat.activeInHierarchy ? false : true);
    //}
    public void SendChat()
    {
        if (ipf.text == "")
            return;

        Text txt = Instantiate(txts[14], txts[14].transform.parent, false);
        txt.text = "<color=green>" + CPlayer.nickName + ": </color>" + ipf.text;
        txt.gameObject.SetActive(true);
        ipf.text = "";

        if (threads[3] != null)
        {
            StopCoroutine(threads[3]);
        }
        threads[3] = TweenScroll();
        StartCoroutine(threads[3]);
    }

    public void OpenQuickChat(bool toShow)
    {
        if (!toShow)
        {
            gojs[9].SetActive(false);
            return;
        }

        gojs[9].SetActive(true);

    }

    public void SendQuickChat(string t)
    {
        OpenQuickChat(false);
        ipf.text = t;
        SendChat();
    }
    #endregion

    #region //VIRTUAL KEYBOARD
    public void OpenVirtualKeyBoard(int id)
    {
        if (strs[0] != "bet")
            return;
        switch (id)
        {
            case 30:
                nums[1] = 0;
                OpenVirtualKeyBoard(11);
                break;
            case 31:
                nums[1] = 1;
                OpenVirtualKeyBoard(11);
                break;
            case -2:    //close all kb
                txts[0].text = txts[1].text = App.listKeyText["GAME_BET"].ToUpper(); //"ĐẶT CƯỢC";
                gojs[3].SetActive(false);
                gojs[4].SetActive(false);
                break;
            case 11:    //open virtual kb 1
                gojs[4].SetActive(false);
                gojs[3].SetActive(true);
                nums[0] = 0;
                txts[nums[1]].text = "0";
                break;
            case 21:    //open virtual kb 2
                gojs[3].SetActive(false);
                gojs[4].SetActive(true);
                nums[0] = 0;
                txts[nums[1]].text = "0";
                break;

            case -3:    //accept
                OpenVirtualKeyBoard(-2);
                Bet(nums[0]);
                break;

            #region //num from 0 to 9
            case 0:
                if (txts[nums[1]].text.Length == 11)
                    return;
                nums[0] = nums[0] * 10;
                txts[nums[1]].text = App.FormatMoney(nums[0], true);
                break;
            case 1:
                if (txts[nums[1]].text.Length == 11)
                    return;
                nums[0] = nums[0] * 10 + 1;
                txts[nums[1]].text = App.FormatMoney(nums[0], true);
                break;
            case 2:
                if (txts[nums[1]].text.Length == 11)
                    return;
                nums[0] = nums[0] * 10 + 2;
                txts[nums[1]].text = App.FormatMoney(nums[0], true);
                break;
            case 3:
                if (txts[nums[1]].text.Length == 11)
                    return;
                nums[0] = nums[0] * 10 + 3;
                txts[nums[1]].text = App.FormatMoney(nums[0], true);
                break;
            case 4:
                if (txts[nums[1]].text.Length == 11)
                    return;
                nums[0] = nums[0] * 10 + 4;
                txts[nums[1]].text = App.FormatMoney(nums[0], true);
                break;
            case 5:
                if (txts[nums[1]].text.Length == 11)
                    return;
                nums[0] = nums[0] * 10 + 5;
                txts[nums[1]].text = App.FormatMoney(nums[0], true);
                break;
            case 6:
                if (txts[nums[1]].text.Length == 11)
                    return;
                nums[0] = nums[0] * 10 + 6;
                txts[nums[1]].text = App.FormatMoney(nums[0], true);
                break;
            case 7:
                if (txts[nums[1]].text.Length == 11)
                    return;
                nums[0] = nums[0] * 10 + 7;
                txts[nums[1]].text = App.FormatMoney(nums[0], true);
                break;
            case 8:
                if (txts[nums[1]].text.Length == 11)
                    return;
                nums[0] = nums[0] * 10 + 8;
                txts[nums[1]].text = App.FormatMoney(nums[0], true);
                break;
            case 9:
                if (txts[nums[1]].text.Length == 11)
                    return;
                nums[0] = nums[0] * 10 + 9;
                txts[nums[1]].text = App.FormatMoney(nums[0], true);
                break;
            #endregion

            case 999:  //key * 1k
                if (txts[nums[1]].text.Length > 7)
                    return;
                nums[0] *= 1000;
                txts[nums[1]].text = App.FormatMoney(nums[0], true);
                break;

            case -1:    //key del
                nums[0] = Mathf.FloorToInt(nums[0] / 10);
                txts[nums[1]].text = App.FormatMoney(nums[0], true);
                break;

            #region //key > 999
            default:
                nums[0] = id;
                txts[nums[1]].text = App.FormatMoney(nums[0], true);
                break;
                #endregion
        }
    }
    #endregion

    private void CountDown(int time)
    {
        //Debug.Log("time   => " + time);
        if (threads[0] != null)
            StopCoroutine(threads[0]);
        threads[0] = CountDown(time, txts[4]);
        StartCoroutine(threads[0]);
    }

    private IEnumerator CountDown(int time, Text txt)
    {
        int count = time;
        while (count > -1)
        {
            txt.text = "00:" + string.Format("{0:0,0}", count);
            time_CountDown = count;
            yield return new WaitForSeconds(1f);
            count--;
        }
    }

    public void Close()
    {
        if (gojs[0].activeSelf || gojs[1].activeSelf || gojs[11].activeSelf)
        {
            return;
        }

        for (int i = 0; i < threads.Length; i++)
        {
            if (threads[i] != null)
                StopCoroutine(threads[i]);
        }
        for (int i = 0; i < tweens.Count; i++)
        {
            DOTween.Kill(tweens[i]);
        }

        //gojs[10].SetActive(false);

        this.gameObject.transform.DOScale(0, 0.3f).OnComplete(() => {
            gameObject.SetActive(false);
        });

        DelHandlers();
        var req_TX_EXIT = new OutBounMessage("TAIXIU.EXIT");
        req_TX_EXIT.addHead();
        App.ws.send(req_TX_EXIT.getReq(), delegate (InBoundMessage res_TX_EXIT)
        {
            // App.trace("[RCV] TAIXIU.EXIT");
        });
        MiniGameControl.instance.OnDisableTX();
    }

    private void LoadData()
    {
        nums[3] = (int)CPlayer.chipBalance;
        txts[11].text = App.FormatMoney(CPlayer.chipBalance);
        SetNewGameDF();


        var req_TX_ENTER = new OutBounMessage("TAIXIU.ENTER");
        req_TX_ENTER.addHead();
        App.ws.send(req_TX_ENTER.getReq(), delegate (InBoundMessage res_TX_ENTER)
        {
            string currState = res_TX_ENTER.readString();  //currTimeCountDown;
            int seconds = res_TX_ENTER.readByte();
            txts[5].text = App.formatMoney(string.Concat(STRING_NGOAC_1, res_TX_ENTER.readInt().ToString(), STRING_NGOAC_2)); //Số ng đặt tài
            txts[6].text = App.formatMoney(string.Concat(STRING_NGOAC_1, res_TX_ENTER.readInt().ToString(), STRING_NGOAC_2)); //Số ng đặt xỉu
            txts[7].text = App.formatMoney(res_TX_ENTER.readLong().ToString());    //Tiền cửa tài
            txts[8].text = App.formatMoney(res_TX_ENTER.readLong().ToString());    //Tiền cửa xỉu
            txts[2].text = App.formatMoney(res_TX_ENTER.readLong().ToString());  //Tiền mình đặt tài
            txts[3].text = App.formatMoney(res_TX_ENTER.readLong().ToString());  //Tiền mình đặt xỉu

            foreach (Transform rtf in imgs[0].transform.parent)       //Delete exits element before
            {
                if (rtf.gameObject.name.Contains("Clone"))
                {
                    Destroy(rtf.gameObject);
                }
            }

            int count = res_TX_ENTER.readByte();
            for (int i = 0; i < count; i++)
            {

                int gateId = res_TX_ENTER.readByte();   //0: tai|1: xiu
                Image img = Instantiate(imgs[0], imgs[0].transform.parent, false);
                img.sprite = gateId == 0 ? sprts[0] : sprts[1];
                img.GetComponent<Button>().onClick.AddListener(() => ClickPostion(img.name));
                img.name = i + " Clone";
                img.gameObject.SetActive(true);
            }
            imgs[1].transform.SetAsLastSibling();
            //App.trace("Count " + count + " || " + res_TX_ENTER.readInt(), "yellow");
            txts[9].text = "#" + res_TX_ENTER.readInt().ToString();

            CountDown(seconds);
            EnterState(currState);
        });
        RegHandler();
    }
    public GameObject childrenContenHis;
    public void ClickPostion(string pos)
    {
        // Debug.Log(pos);
        List<int> lst = new List<int>();
        OutBounMessage req_track = new OutBounMessage("TAIXIU.REPORT");
        req_track.addHead();
        App.ws.send(req_track.getReq(), delegate (InBoundMessage res_track)
        {
            int count = res_track.readInt();
            for (int i = count - 1; i >= 0; i--)
            {
                int threadId = res_track.readInt();
                // Debug.Log("threadId = " + threadId);
                if (i == count - 1)
                {
                    int x1 = res_track.readInt();
                    int x2 = res_track.readInt();
                    int x3 = res_track.readInt();
                }
                else
                {
                    int x1 = res_track.readInt();
                    int x2 = res_track.readInt();
                    int x3 = res_track.readInt();
                }
                int sub = res_track.readInt();
                lst.Add(threadId);
            }

            int vt = int.Parse(pos.Split(' ')[0]);
            //Debug.Log(" => " + childrenContenHis.transform.childCount+" "+ lst.Count+" "+vt);
            //Debug.Log("Poss " + (lst.Count - childrenContenHis.transform.childCount - 5 + vt));
            int posMax = childrenContenHis.transform.childCount - 3;
            MathInfoTX(lst[lst.Count - 1 - posMax + vt]);
            //Debug.Log("Session = " +lst[lst.Count-12-1+vt]);
        });


    }
    private int _gateId;
    private void RegHandler()
    {
        var req_UPDATE_POT = new OutBounMessage("TAIXIU.UPDATE_POT");
        req_UPDATE_POT.addHead();
        handlers.Add("TAIXIU.UPDATE_POT");
        App.ws.sendHandler(req_UPDATE_POT.getReq(), delegate (InBoundMessage res_UPDATE_POT)
        {
            // Debug.Log("[RECV] TAIXIU.UPDATE_POT");
            // App.trace("RECV [TAIXIU.UPDATE_POT]");
            res_UPDATE_POT.readByte();
            txts[5].text = App.formatMoney(res_UPDATE_POT.readInt().ToString());   //Số ng đặt tài
            txts[6].text = App.formatMoney(res_UPDATE_POT.readInt().ToString());  //Số ng đặt xỉu
            txts[7].text = App.formatMoney(res_UPDATE_POT.readLong().ToString());    //Tiền cửa tài
            txts[8].text = App.formatMoney(res_UPDATE_POT.readLong().ToString());    //Tiền cửa xỉu
        });

        var req_PREPARE = new OutBounMessage("TAIXIU.PREPARE");
        req_PREPARE.addHead();
        handlers.Add("TAIXIU.PREPARE");
        App.ws.sendHandler(req_PREPARE.getReq(), delegate (InBoundMessage res_PREPARE)
        {
            //Debug.Log("[RECV] TAIXIU.PREPARE");
            // App.trace("[RECV] TAIXIU.PREPARE");
            res_PREPARE.readByte();
            EnterState("prepare");
            CountDown(res_PREPARE.readByte());
        });

        var req_START = new OutBounMessage("TAIXIU.START");
        req_START.addHead();
        handlers.Add("TAIXIU.START");
        App.ws.sendHandler(req_START.getReq(), delegate (InBoundMessage res_START)
        {
            nums[3] = (int)CPlayer.chipBalance;
            // Debug.Log("[RECV] TAIXIU.START");
            res_START.readByte();
            // App.trace("RECV [TAIXIU.START]");
            CountDown(res_START.readByte());
            txts[9].text = "#" + App.formatMoney(res_START.readInt().ToString()); //Update curr thread
            EnterState("bet");
        });

        var req_SELL_GATE = new OutBounMessage("TAIXIU.SELL_GATE");
        req_SELL_GATE.addHead();
        handlers.Add("TAIXIU.SELL_GATE");
        App.ws.sendHandler(req_SELL_GATE.getReq(), delegate (InBoundMessage res_SELL_GATE)
        {
            // Debug.Log("[RECV] TAIXIU.SELL_GATE");
            // App.trace("[RCV] TAIXIU.SELL_GATE");
            OpenVirtualKeyBoard(-2);
            res_SELL_GATE.readByte();
            CountDown(res_SELL_GATE.readByte());
            EnterState("sellGate");
            //soundEndTurn.Stop();
            //soundEndTurn.Play();
            SoundManager.instance.PlayEffectSound(SoundFX.TAIXIU_COUNTDOWN);
            //LoadingControl.instance.PlaySound("sound-tx-cool-down");
        });

        var req_REFUND = new OutBounMessage("TAIXIU.REFUND");
        req_REFUND.addHead();
        handlers.Add("TAIXIU.REFUND");
        App.ws.sendHandler(req_REFUND.getReq(), delegate (InBoundMessage res_REFUND)
        {
            // Debug.Log("[RECV] TAIXIU.REFUND");
            // App.trace("[RCV] TAIXIU.REFUND");
            res_REFUND.readByte();
            long amount = res_REFUND.readLong();
            int id = res_REFUND.readByte();

            int refunAmount = nums[4 + id] - (int)amount;

            string new1 = App.listKeyText["TX_REFUND"];

            string new2 = new1.Replace("#1", App.FormatMoney(refunAmount));
            string new3 = new2.Replace("#2", App.listKeyText["CURRENCY"] + "\n");
            string new4 = new3.Replace("#3", (id == 0 ? "TÀI" : "XỈU"));

            ShowNote(new4); //"Trả lại " + App.FormatMoney(refunAmount) + " Gold\ncửa " + (id == 0 ? "TÀI" : "XỈU")

            txts[7 + id].text = App.FormatMoney(amount);
            nums[3] = nums[3] + refunAmount;
            txts[11].text = App.FormatMoney(nums[3]);
        });


        var req_SHOW_RS = new OutBounMessage("TAIXIU.SHOW_RESULT");
        req_SHOW_RS.addHead();
        handlers.Add("TAIXIU.SHOW_RESULT");
        App.ws.sendHandler(req_SHOW_RS.getReq(), delegate (InBoundMessage res_SHOW_REULT)
        {

            txts[4].text = txts[10].text = "";
            //Debug.Log("[RECV] TAIXIU.SHOW_RESULT");
            // App.trace("[RECV] TAIXIU.SHOW_RESULT");

            res_SHOW_REULT.readByte();

            int[] rs = new int[3];
            rs[0] = res_SHOW_REULT.readByte();
            rs[1] = res_SHOW_REULT.readByte();
            rs[2] = res_SHOW_REULT.readByte();
            int gateId = res_SHOW_REULT.readByte();
            int time = res_SHOW_REULT.readInt();
            timecountdown = time;
            timecount = time;
            // Debug.Log("time = " + time);
            string content = gateId == 0 ? "Tài: " : "Xỉu: ";
            content = content + App.formatMoney((rs[0] + rs[1] + rs[2]).ToString()) + " điểm";
            //App.trace(content, "red");

            StartCoroutine(CountDown());
            if (threads[0] != null)
                StopCoroutine(threads[0]);

            //txts[4].text = rs[0] + "|" + rs[1] + "|" + rs[2];

            gojs[7].SetActive(true);

            if (threads[2] != null)
                StopCoroutine(threads[2]);

            threads[2] = SpinDices(rs, gateId);
            StartCoroutine(threads[2]);

            EnterState("showResult", content);
        });

        var req_DIVIDE_CHIP = new OutBounMessage("TAIXIU.DIVIDE_CHIP");
        req_DIVIDE_CHIP.addHead();
        handlers.Add("TAIXIU.DIVIDE_CHIP");
        App.ws.sendHandler(req_DIVIDE_CHIP.getReq(), delegate (InBoundMessage res_DIVIDE_CHIP)
        {
            // Debug.Log("[RECV] TAIXIU.DIVIDE_CHIP");
            // App.trace("[RECV] TAIXIU.DIVIDE_CHIP");
            res_DIVIDE_CHIP.readByte();
            int amount = res_DIVIDE_CHIP.readInt();
            ShowNote(amount > 0 ? ("+ " + App.FormatMoney(amount)) : ("- " + App.FormatMoney(Math.Abs(amount))), 3);
        });

        var req_GAME_OVER = new OutBounMessage("TAIXIU.GAMEOVER");
        req_GAME_OVER.addHead();
        handlers.Add("TAIXIU.GAMEOVER");
        App.ws.sendHandler(req_GAME_OVER.getReq(), delegate (InBoundMessage res_GAME_OVER)
        {
            if (isMolding && isBat)
            {
                gojBowl.SetActive(false);
                gojs[5 + _gateId].transform.DOScale(1.2f, .125f).SetLoops(1500, LoopType.Yoyo).OnComplete(() =>
                {
                    gojs[5 + _gateId].transform.localScale = new Vector3(1.7f, 1.7f, 0);
                }).SetId(idTweenSlace);
            }
            isBat = true;
            gojs[8].SetActive(true);
            res_GAME_OVER.readByte();
            // App.trace("RECV [TAIXIU.GAMEOVER]");
            // Debug.Log("[RECV] [TAIXIU.GAMEOVER]");
            oldBet = 0;
            foreach (Transform rtf in imgs[0].transform.parent)       //Delete exits element before
            {
                if (rtf.gameObject.name.Contains("Clone"))
                {
                    Destroy(rtf.gameObject);
                }
            }

            int count = res_GAME_OVER.readByte();
            for (int i = 0; i < count; i++)
            {
                //if (i < 4)
                //    continue;
                int gateId = res_GAME_OVER.readByte();   //0: tai|1: xiu
                //App.trace("Gate Id "+gateId,"yellow");
                Image img = Instantiate(imgs[0], imgs[0].transform.parent, false);
                img.sprite = gateId == 0 ? sprts[0] : sprts[1];
                img.name = i + " Clone";
                img.GetComponent<Button>().onClick.AddListener(() => ClickPostion(img.name));
                img.gameObject.SetActive(true);
            }
            imgs[1].transform.SetAsLastSibling();

            if (nums[3] < CPlayer.chipBalance)
            {
                if (threads[1] != null)
                    StopCoroutine(threads[1]);
                threads[1] = TweenNum(txts[11], nums[3], (int)CPlayer.chipBalance);
                StartCoroutine(threads[1]);
            }

        });

    }
    private bool isBat = true;

    public void OpenBat()
    {
        isBat = false;
        gojBowl.SetActive(false);
        gojs[8].SetActive(true);
        Tween test= gojs[5 + _gateId].transform.DOScale(1.2f, .125f).SetLoops(1500, LoopType.Yoyo).OnComplete(() =>
        {
            gojs[5 + _gateId].transform.localScale = new Vector3(1.7f, 1.7f, 0);
        }).SetId(idTweenSlace);
        //  Debug.Log(doScale);
        /* gojs[5 + _gateId].transform.DOScale(1.2f, .125f).SetLoops(40, LoopType.Yoyo).OnComplete(() =>
         {
             gojs[5 + _gateId].transform.localScale = new Vector3(1.7f, 1.7f, 0);
         });*/

    }

    private void DelHandlers()
    {
        for (int i = 0; i < handlers.Count; i++)
        {
            var req = new OutBounMessage(handlers[i]);
            req.addHead();
            App.ws.delHandler(req.getReq());
        }
    }

    private void EnterState(string state, string content = "")
    {
        strs[0] = state;
        txts[10].fontSize = 40;
        txts[10].color = colorTextTime[0];
        txts[4].color = colorTextTime[0];
        isBet = false;
        switch (state)
        {
            case "bet":     //Đặt cửa

                DOTween.Kill(idTweenSlace);
                buttonNan.SetActive(true);
                panelTime.SetActive(false);
                timeCountdown.text = "";
                timecountdown = 0;
                gojs[8].SetActive(false);
                txts[10].text = App.listKeyText["TX_BET_GATE"].ToUpper(); //"ĐẶT CỬA";
                txts[10].color = colorTextTime[0];
                isBet = true;
                if (spinFX != null)
                    StopCoroutine(spinFX);
                timeSpinFX.transform.rotation = new Quaternion(0, 0, 0, 0);
                timeSpinFX.SetActive(true);
                spinFX = Spin_Time_FX();
                StartCoroutine(spinFX);

                break;
            case "prepare":     //Chuẩn bị
                DOTween.Kill(idTweenSlace);
                gojs[5].transform.localScale = new Vector3(1.7f, 1.7f, 0);
                gojs[6].transform.localScale = new Vector3(1.7f, 1.7f, 0);
                buttonNan.SetActive(true);
                txts[10].text = App.listKeyText["GAME_PREPARE"].ToUpper();// "CHUẨN BỊ";
                timeSpinFX.SetActive(false);
                txts[10].color = colorTextTime[0];
                SetNewGameDF();
                break;
            case "sellGate":     //Hệ thống cân cửa
                buttonNan.SetActive(false);
                panelTime.SetActive(false);
                timeCountdown.text = "";
                timecountdown = 0;
                gojs[8].SetActive(false);
                timeSpinFX.SetActive(false);
                txts[10].transform.localScale = new Vector3(1f, 1f, 0);
                txts[10].text = App.listKeyText["TX_BET_BALANCE"].ToUpper();//"CÂN CỬA";
                txts[10].color = colorTextTime[0];
                break;
            case "showResult":     //Kết quả
                buttonNan.SetActive(false);
                timeSpinFX.SetActive(false);
                txts[10].text = "";
                txts[10].color = colorTextTime[0];
                break;
            default:        //Vui lòng chờ
                DOTween.Kill(idTweenSlace);
                buttonNan.SetActive(true);
                timeSpinFX.SetActive(false);
                txts[10].fontSize = 27;
                txts[10].text = App.listKeyText["TX_WAITING"].ToUpper();//"VUI LÒNG CHỜ";
                txts[10].color = colorTextTime[0];
                break;
        }
    }
    /// <summary>
    /// True: Nặn   | False: Không Nặn
    /// </summary>
    private bool isMolding = false;
    public GameObject buttonNan;
    public Image imageMolding;
    public Sprite[] spriterHand;
    private static string idTweenSlace = "idTweenSlace";
    /// <summary>
    /// Nặn
    /// </summary>
    public void Molding()
    {
        isMolding = !isMolding;
        imageMolding.overrideSprite = spriterHand[isMolding ? 1 : 0];

    }
    private int speedMove = 700;
    private float waittingTime = 0.1f;
    private int time_CountDown = 0;
    private bool isBet = false;
    public GameObject timeSpinFX;
    private IEnumerator spinFX = null;
    private IEnumerator Spin_Time_FX()
    {

        while (timeSpinFX.activeInHierarchy)
        {
            yield return new WaitForSeconds(waittingTime);

            if (time_CountDown > 20)
            {
                speedMove = 700;
            }
            else if (time_CountDown > 10)
            {
                txts[10].color = colorTextTime[1];
                txts[4].color = colorTextTime[1];
                speedMove = 1500;
            }
            else
            {

                if (isBet)
                {
                    txts[10].color = colorTextTime[2];
                    txts[4].color = colorTextTime[2];
                    speedMove = 2500;
                }
            }

          /*  if (time_CountDown < 10)
            {
                txts[10].color = colorTextTime[2];
                txts[4].color = colorTextTime[2];
                speedMove = 2500;
            }
            else if (time_CountDown < 20)
            {
                txts[10].color = colorTextTime[1];
                txts[4].color = colorTextTime[1];
                speedMove = 1500;
            }
            else
                speedMove = 700;*/

           // Debug.Log(time_CountDown + " => " +speedMove);
            if (timeSpinFX.activeInHierarchy)
            {
                timeSpinFX.transform.Rotate(Vector3.back *Time.deltaTime* speedMove);
            }
        }
       // StartCoroutine(Spin_Time_FX());
    }
    private IEnumerator HideSmt(int time, GameObject goj)
    {
        yield return new WaitForSeconds(time);
        goj.SetActive(false);
    }

    private int oldBet = 0;
    private void Bet(int numToBet)
    {
        var req_BET = new OutBounMessage("TAIXIU.BET");
        req_BET.addHead();
        req_BET.writeLong((long)numToBet);
        req_BET.writeByte(nums[1]);
        App.ws.send(req_BET.getReq(), delegate (InBoundMessage res_BET)
        {
            oldBet += nums[0];
            // App.trace(numToBet + " || " + nums[0], "yellow");
            txts[2 + nums[1]].text = App.FormatMoney(oldBet);

            //nums[3] -= numToBet;
            nums[4 + nums[1]] += numToBet;
            //txts[11].text = App.FormatMoney(nums[3]);
        });
    }
    public GameObject panelTime;
    private int timecountdown = 0;
    private static int timecount = 0;
    public Text timeCountdown;
    /// <summary>
    /// 0: Mau trang mac dinh | 1: Vang | 2: Do
    /// </summary>
    [Header("0: Trang | 1: Vang | 2: Do")]
    public static Color32[] colorTextTime={ new Color32(255, 233, 255, 255), new Color32(255,233,0,255), new Color32(255, 0, 18, 255) };
    private IEnumerator CountDown()
    {
        timeCountdown.text = timecountdown.ToString();
        //Debug.Log("Time "+timecountdown);
        yield return new WaitForSeconds(1);
        timecountdown--;
        if (timecountdown >= 0)
        {
            if (!panelTime.activeSelf)
                panelTime.SetActive(true);
            timeCountdown.text = timecountdown.ToString();
            if (isMolding)//check xem có nặn k
            {
                if ((timecount / 3) + 1 > timecountdown)// Đc 2/3 thời gian sẽ tự mở bát
                {
                    if (gojBowl.activeInHierarchy)
                        OpenBat();
                }
            }
            StartCoroutine(CountDown());
        }
        else
        {
            // Debug.Log("End Time");
        }
    }
    private void SetNewGameDF()
    {
        txts[2].text = txts[3].text = txts[5].text = txts[6].text
            = txts[7].text = txts[8].text = "0";
        nums[4] = nums[5] = 0;
        txts[12].text = "";
        gojs[8].SetActive(false);
        gojs[7].SetActive(false);
        imgs[2].gameObject.SetActive(false);
        imgs[3].gameObject.SetActive(false);
        imgs[4].gameObject.SetActive(false);
        panelTime.SetActive(false);
        timeCountdown.text = "";
        timecountdown = 0;
        gojBowl.transform.position = postionDefaultBowl.position;
        gojBowl.SetActive(false);
        timeSpinFX.SetActive(false);
    }

    private void ShowNote(string t, int time = 4)
    {
        // Debug.Log("SHOW");
        txts[12].text = t;
        txts[12].gameObject.SetActive(true);
        StartCoroutine(HideSmt(time, txts[12].gameObject));
    }

    private IEnumerator SpinDices(int[] ids, int gateId)
    {
        //App.trace(ids[0] + "|" + ids[1] + "|" + ids[2], "red");
        yield return new WaitForSeconds(1.333333f);
        imgs[2].sprite = sprts[1 + ids[0]];
        imgs[3].sprite = sprts[1 + ids[1]];
        imgs[4].sprite = sprts[1 + ids[2]];

        imgs[2].gameObject.SetActive(true);
        imgs[3].gameObject.SetActive(true);
        imgs[4].gameObject.SetActive(true);


        _gateId = gateId;
        txts[13].text = (ids[0] + ids[1] + ids[2]).ToString() + "\n"+ App.listKeyText["TX_POINT"];

        if (isMolding)
        {

            gojBowl.SetActive(true);
        }
        else
        {
            gojBowl.SetActive(false);
            gojs[5 + gateId].transform.DOScale(1.2f, .125f).SetLoops(1500, LoopType.Yoyo).OnComplete(() =>
            {
                gojs[5 + gateId].transform.localScale = new Vector3(1.7f, 1.7f, 0);
            }).SetId(idTweenSlace);
            gojs[8].SetActive(true);
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
        //App.trace(i.ToString());
        txt.transform.localScale = Vector2.one;
        yield return new WaitForSeconds(.05f);
    }

    private IEnumerator TweenScroll()
    {
        yield return new WaitForSeconds(.25f);
        if (DOTween.IsTweening(scr))
        {
            DOTween.Complete("scroll");
        }
        scr.DOVerticalNormalizedPos(0, .25f).SetId("scroll");
        //isLoading = firsLoad = false;

    }
    public Text txtSession;
    public Text txtSumSession;
    public List<Image> xucXac;
    public GameObject panelMatInfor;
    public GameObject[] lightTX;
    public void Close_MathInfoTX()
    {
        panelMatInfor.SetActive(false);
    }
    public void MathInfoTX(int idCodeSession)
    {
        // Debug.Log("Vao " + idCodeSession);
        var req_matchInfo = new OutBounMessage("TAIXIU.MATCH_INFO");
        req_matchInfo.addHead();
        req_matchInfo.writeInt(idCodeSession);
        App.ws.send(req_matchInfo.getReq(), delegate (InBoundMessage res_matchInfo)
        {
            // Debug.Log("Call Back");
            int x1 = (int)res_matchInfo.readByte();
            int x2 = (int)res_matchInfo.readByte();
            int x3 = (int)res_matchInfo.readByte();
            int result = (int)res_matchInfo.readByte();//Cửa thắng 0:Tài|1:Xỉu
                                                       //Debug.Log(" x1 = "+ x1+" x2 = "+x2+" x3 = "+x3+" rs= "+result);
            lightTX[0].transform.DOKill();
            lightTX[1].transform.DOKill();
            lightTX[0].transform.localScale = new Vector3(2, 2, 0);
            lightTX[1].transform.localScale = new Vector3(2, 2, 0);
            if (result == 0)
            {
               // lightTX[1].transform.DOKill();
                lightTX[0].transform.DOScale(1.2f, .125f).SetLoops(1000, LoopType.Yoyo).OnComplete(() =>
                {
                    lightTX[0].transform.localScale = new Vector3(2, 2, 0);
                });
            }
            else
            {
               // lightTX[0].transform.DOKill();
                lightTX[1].transform.DOScale(1.2f, .125f).SetLoops(1000, LoopType.Yoyo).OnComplete(() =>
                {
                    lightTX[1].transform.localScale = new Vector3(2, 2, 0);
                });
            }
            txtSumSession.text = (x1 + x2 + x3).ToString();
            txtSession.text = "#" + idCodeSession;
            xucXac[0].overrideSprite = sprts[x1 + 1];
            xucXac[1].overrideSprite = sprts[x2 + 1];
            xucXac[2].overrideSprite = sprts[x3 + 1];
            panelMatInfor.SetActive(true);
        });
    }
}
