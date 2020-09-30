using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text.RegularExpressions;
using DG.Tweening;
using UnityEngine.Events;
using Core.Server.Api;
public class FeastControl : MonoBehaviour {
    private int myMoney;
    private List<string> handlers;
    private List<int> betOld = new List<int>();
    private List<int> betPerMatch = new List<int>();
    private int[] betCur = new int[6];
    private IEnumerator[] threads;
    /// <summary>
    /// 0-11: betValue
    /// </summary>
    public Text[] txts;
    /// <summary>
    /// 0: anotherValueBet | 1: resultPer100times | 2: result20times | 3: historyPanel | 4: historyElement | 5: historyPanel | 6: history20timesBg | 7: textValuePrize | 8: textContent | 9: diaContent | 10:bg | 11: icoBat | 12: textNoti
    /// </summary>
    public GameObject[] feastGojs;
    /// <summary>
    /// 0: anotherValueBet
    /// </summary>
    public InputField[] ipfs;
    /// <summary>
    /// 0: currBet
    /// </summary>
    private int[] nums = new int[1];
    /// <summary>
    /// 0: isBet | 1: isShownoti | 2: canClose | 3: canBet | 4: canDọubleBet | 5: holdBtnBet
    /// </summary>
    private bool[] boolLs = new bool[6];
    /// <summary>
    /// 0-5: btnBetIco | 6-9: btnOption | 10-16: btnBetValue
    /// </summary>
    public Button[] btnsFeast;
    /// <summary>
    /// 0-13: btnBet | 14-19: icoHistory | 20-25: iconxx
    /// </summary>
    public Sprite[] sprts;
    /// <summary>
    /// 0-3: iconxx
    /// </summary>
    public Image[] imgs;
    public Text txtSession;
    public static FeastControl instance;
    private void Awake()
    {
        if (instance != null)
            Destroy(instance);
        else
            instance = this;
    }
    private void OnEnable()
    {
        myMoney = (int)CPlayer.chipBalance;
        transform.SetSiblingIndex(LoadingControl.MAX_SIBLING_INDEX);
        boolLs[2] = true;
        boolLs[3] = true;
        boolLs[4] = true;
        betCur[0] = betCur[1] = betCur[2] = betCur[3] = betCur[4] = betCur[5] = 0;
        betPerMatch.Add(0);
        betPerMatch.Add(0);
        betPerMatch.Add(0);
        betPerMatch.Add(0);
        betPerMatch.Add(0);
        betPerMatch.Add(0);
        SetBet(1000);
        feastGojs[4].transform.parent.GetComponent<ContentSizeFitter>().enabled = false;
        threads = new IEnumerator[3];
        handlers = new List<string>();
        LoadData();
        CPlayer.changed += CPlayer_changed;
    }

    public void ControllScrollRect()
    {
        feastGojs[2].GetComponent<ScrollRect>().verticalNormalizedPosition = 1;
    }
    private void CPlayer_changed(string type)
    {
        if (type == "chip")
        {
            if (CPlayer.preChipBalance != CPlayer.chipBalance)
            {
                myMoney = (int)CPlayer.chipBalance;
            }
        }
    }

    //public void SetChatActive()
    //{
    //    feastGojs[13].SetActive(!feastGojs[13].activeInHierarchy);
    //}
    public void OpenHistory(bool open)
    {
        if (!open)
        {
            feastGojs[5].SetActive(false);
            feastGojs[4].transform.parent.GetComponent<ContentSizeFitter>().enabled = false;
            return;
        }
        foreach (Transform rtf in feastGojs[4].transform.parent)       //Delete exits element before
        {
            if (rtf.gameObject.name != feastGojs[4].name)
            {
                Destroy(rtf.gameObject);
            }
        }
        feastGojs[5].SetActive(true);
        App.trace("[SEND]BAUCUA.GET_INFO");
        var req_History = new OutBounMessage("BAUCUA.GET_INFO");        
        req_History.addHead();
        App.ws.send(req_History.getReq(),delegate (InBoundMessage res) {
            App.trace("[RECV]BAUCUA.GET_INFO");
            int countHis = res.readByte();
            App.trace("Count his "+ countHis,"red");
            for(int i = 0; i < countHis; i++)
            {
                GameObject hisClone = Instantiate(feastGojs[4], feastGojs[4].transform.parent, false);
                Text[] txtshis = hisClone.GetComponentsInChildren<Text>();
                txtshis[0].text = res.readString();
                txtshis[1].text = res.readString();
                txtshis[2].text = res.readString();
                txtshis[3].text = res.readString();
                txtshis[4].text = App.formatMoney(res.readString());
                hisClone.SetActive(true);
            }
            feastGojs[4].transform.parent.GetComponent<ContentSizeFitter>().enabled = true;
        });
    }
    private void LoadData()
    {
        foreach (Transform rtf in feastGojs[6].transform.parent)       //Delete exits element before
        {
            if (rtf.gameObject.name != feastGojs[6].name)
            {
                Destroy(rtf.gameObject);
            }
        }
        App.trace("[SEND]BAUCUA.ENTER");
        var req_BauCuaEnter = new OutBounMessage("BAUCUA.ENTER");
        req_BauCuaEnter.addHead();
        App.ws.send(req_BauCuaEnter.getReq(), delegate (InBoundMessage res) {
            App.trace("[RECV]BAUCUA.ENTER");
            string currState = res.readString();  //currTimeCountDown;
            if (currState.Contains("bet"))
            {
                btnsFeast[0].interactable = true;
                btnsFeast[1].interactable = true;
                btnsFeast[2].interactable = true;
                btnsFeast[3].interactable = true;
                btnsFeast[4].interactable = true;
                btnsFeast[5].interactable = true;
                btnsFeast[6].interactable = true;
                btnsFeast[7].interactable = true;
                btnsFeast[8].interactable = true;
                btnsFeast[9].interactable = true;
            }
            else
            {
                boolLs[3] = false;
                btnsFeast[0].interactable = false;
                btnsFeast[1].interactable = false;
                btnsFeast[2].interactable = false;
                btnsFeast[3].interactable = false;
                btnsFeast[4].interactable = false;
                btnsFeast[5].interactable = false;
                btnsFeast[6].interactable = false;
                btnsFeast[7].interactable = false;
                btnsFeast[8].interactable = false;
                btnsFeast[9].interactable = false;
            }
            int seconds = res.readByte();
            App.trace("curStage: " + currState + " || seconds : " + seconds, "red");
            int numberBetBau = res.readInt();
            int numberBetTom = res.readInt();
            int numberBetCua = res.readInt();
            int numberBetCa = res.readInt();
            int numberBetGa = res.readInt();
            int numberBetHuou = res.readInt();
            int totalMoneyBetBau = (int)res.readLong();
            int totalMoneyBetTom = (int)res.readLong();
            int totalMoneyBetCua = (int)res.readLong();
            int totalMoneyBetCa = (int)res.readLong();
            int totalMoneyBetGa = (int)res.readLong();
            int totalMoneyBetHuou = (int)res.readLong();
            int mineMoneyBetBau = (int)res.readLong();
            int mineMoneyBetTom = (int)res.readLong();
            int mineMoneyBetCua = (int)res.readLong();
            int mineMoneyBetCa = (int)res.readLong();
            int mineMoneyBetGa = (int)res.readLong();
            int mineMoneyBetHuou = (int)res.readLong();
            //App.trace("number bet bau " + numberBetBau + " || number bet cua " + numberBetCua + " || number bet tom " + numberBetTom + " || number bet ca " + numberBetCa + " || number bet ga " + numberBetGa + " || number bet huou " + numberBetHuou, "blue");
            //App.trace("total money bau " + totalMoneyBetBau + " || total money cua " + totalMoneyBetCua + " || total money tom " + totalMoneyBetTom + " || total money ca " + totalMoneyBetCa + " || total money ga " + totalMoneyBetGa + " || total money huou " + totalMoneyBetHuou, "red");
            //App.trace("mine money bet bau " + mineMoneyBetBau + " || mine money bet cua " + mineMoneyBetCua + " || mine money bet tom " + mineMoneyBetTom + " || mine money bet ca " + mineMoneyBetCa + " || mine money bet ga " + mineMoneyBetGa + " || mine money bet huou " + mineMoneyBetHuou, "yellow");
            txts[0].text = App.formatMoneyD((float)totalMoneyBetBau);
            txts[1].text = App.formatMoneyD((float)mineMoneyBetBau);
            txts[4].text = App.formatMoneyD((float)totalMoneyBetCua);
            txts[3].text = App.formatMoneyD((float)mineMoneyBetCua);
            txts[2].text = App.formatMoneyD((float)totalMoneyBetTom);
            txts[5].text = App.formatMoneyD((float)mineMoneyBetTom);
            txts[6].text = App.formatMoneyD((float)totalMoneyBetCa);
            txts[7].text = App.formatMoneyD((float)mineMoneyBetCa);
            txts[8].text = App.formatMoneyD((float)totalMoneyBetGa);
            txts[9].text = App.formatMoneyD((float)mineMoneyBetGa);
            txts[10].text = App.formatMoneyD((float)totalMoneyBetHuou);
            txts[11].text = App.formatMoneyD((float)mineMoneyBetHuou);
            int historyBet = res.readByte();
            //App.trace("total history " + historyBet, "green");
            for (int i = 0; i < historyBet; i++)
            {
                string historyElement = res.readString();
                //App.trace("history " + i + " " + historyElement, "orange");
                GameObject hisBgClone = Instantiate(feastGojs[6],feastGojs[6].transform.parent,false);
                Image[] imageHisClone = hisBgClone.GetComponentsInChildren<Image>();                
                string[] perElemnt = historyElement.Split('-');
                for(int j = 0; j < perElemnt.Length; j++)
                {
                    //App.trace("per element " + j + " : " + perElemnt[j], "yellow");
                    SetIconHistory(perElemnt[j],imageHisClone[j+j+1]);
                }
                hisBgClone.SetActive(true);
            }
            int sessionValue = res.readInt();
            txtSession.text ="#"+ sessionValue.ToString();
            CountDown(seconds);
        });
        RegHandler();
    }

    private void SetIconHistory(string type,Image spriteIcon)
    {
        //App.trace("type " + type + " || name " + spriteIcon.name, "red");
        switch (type)
        {
            case "Bầu":
                spriteIcon.sprite = sprts[14];
                break;
            case "Cá":
                spriteIcon.sprite = sprts[15];
                break;
            case "Cua":
                spriteIcon.sprite = sprts[16];
                break;
            case "Tôm":
                spriteIcon.sprite = sprts[19];
                break;
            case "Gà":
                spriteIcon.sprite = sprts[17];
                break;
            case "Hươu":
                spriteIcon.sprite = sprts[18];
                break;
        }
    }

    private void Bet()
    {
        if (betCur[0] <= 0 && betCur[1] <= 0 && betCur[2] <= 0 && betCur[3] <= 0 && betCur[4] <= 0 && betCur[5] <= 0 || !boolLs[3] || boolLs[5] || (betCur[0] - betPerMatch[0]) <= 0 && (betCur[1] - betPerMatch[1]) <= 0 && (betCur[2] - betPerMatch[2]) <= 0 && (betCur[3] - betPerMatch[3]) <= 0 && (betCur[4] - betPerMatch[4]) <= 0 && (betCur[5] - betPerMatch[5]) <= 0)
            return;
        string bet = (betCur[0]-betPerMatch[0]).ToString() + "-" + (betCur[1]- betPerMatch[1]).ToString() + "-" + (betCur[2]- betPerMatch[2]).ToString() + "-" + (betCur[3]- betPerMatch[3]).ToString() + "-" + (betCur[4]- betPerMatch[4]).ToString() + "-" + (betCur[5]- betPerMatch[5]).ToString();
        int total = (betCur[0]- betPerMatch[0]) + (betCur[1]- betPerMatch[1]) + (betCur[2]- betPerMatch[2]) + (betCur[3]- betPerMatch[3]) + (betCur[4]- betPerMatch[4]) + (betCur[5]- betPerMatch[5]);
        //App.trace(total, "yellow");
        //App.trace(bet, "yellow");
        if (myMoney < total)
        {
            //App.showErr("Không đủ tiền cược.");
            App.showErr(App.listKeyText["WARN_NOT_ENOUGH_GOLD"]);
            return;
        }
        boolLs[5] = true;
        App.trace("[SEND]BAUCUA.BET");
        var req_Bet = new OutBounMessage("BAUCUA.BET");
        req_Bet.addHead();
        req_Bet.writeInt(total);
        req_Bet.writeString(bet);
        App.ws.send(req_Bet.getReq(), delegate (InBoundMessage res) {
            App.trace("[RECV]BAUCUA.BET");
            boolLs[4] = false;
            boolLs[0] = true;
            betOld.Clear();
            betOld.Add(betCur[0]);
            betOld.Add(betCur[1]);
            betOld.Add(betCur[2]);
            betOld.Add(betCur[3]);
            betOld.Add(betCur[4]);
            betOld.Add(betCur[5]);
            betPerMatch.Clear();
            betPerMatch.Add(betCur[0]);
            betPerMatch.Add(betCur[1]);
            betPerMatch.Add(betCur[2]);
            betPerMatch.Add(betCur[3]);
            betPerMatch.Add(betCur[4]);
            betPerMatch.Add(betCur[5]);
            if (threads[1] != null)
                StopCoroutine(threads[1]);
            threads[1] = ShowNoti("Đặt cửa thành công",2f);
            StartCoroutine(threads[1]);
            boolLs[5] = false;
        });
    }

    public void SetBet(int value)
    {
        btnsFeast[10].GetComponent<Image>().sprite = sprts[1];
        btnsFeast[11].GetComponent<Image>().sprite = sprts[5];
        btnsFeast[12].GetComponent<Image>().sprite = sprts[7];
        btnsFeast[13].GetComponent<Image>().sprite = sprts[9];
        btnsFeast[14].GetComponent<Image>().sprite = sprts[11];
        btnsFeast[15].GetComponent<Image>().sprite = sprts[3];
        btnsFeast[16].GetComponent<Image>().sprite = sprts[13];
        btnsFeast[17].GetComponent<Image>().sprite = sprts[27];

        switch (value)
        {
            case 1000:
                btnsFeast[10].GetComponent<Image>().sprite = sprts[0];
                break;
            case 5000:
                btnsFeast[11].GetComponent<Image>().sprite = sprts[4];
                break;
            case 10000:
                btnsFeast[12].GetComponent<Image>().sprite = sprts[6];
                break;
            case 50000:
                btnsFeast[13].GetComponent<Image>().sprite = sprts[8];
                break;
            case 100000:
                btnsFeast[14].GetComponent<Image>().sprite = sprts[10];
                break;
            case 1000000:
                btnsFeast[15].GetComponent<Image>().sprite = sprts[2];
                break;
            case 10000000:
                btnsFeast[17].GetComponent<Image>().sprite = sprts[26];
                break;

        }
        if (feastGojs[0].activeSelf)
        {
            ipfs[0].text = "";
            nums[0] = 0;
            feastGojs[0].SetActive(false);
        }
        nums[0] = value;
        //App.trace("Value Bet "+value,"yellow");
    }

    public void OpenAnotherBet()
    {
        btnsFeast[10].GetComponent<Image>().sprite = sprts[1];
        btnsFeast[11].GetComponent<Image>().sprite = sprts[5];
        btnsFeast[12].GetComponent<Image>().sprite = sprts[7];
        btnsFeast[13].GetComponent<Image>().sprite = sprts[9];
        btnsFeast[14].GetComponent<Image>().sprite = sprts[11];
        btnsFeast[15].GetComponent<Image>().sprite = sprts[3];
        btnsFeast[16].GetComponent<Image>().sprite = sprts[12];

        feastGojs[0].SetActive(true);
    }
    public void SetAnotherBet(bool delete)
    {
        if (delete) {
            ipfs[0].text = "";
            nums[0] = 0;
            return;
        }
        if(Regex.IsMatch(ipfs[0].text, @"\d+"))
            nums[0] = Int32.Parse(ipfs[0].text);
        feastGojs[0].SetActive(false);
    }

    public void BetPerElement(string type)
    {
        //App.trace("Type " + type + " || value " + nums[0], "red");
        if (!boolLs[1])
        {           
            if (threads[1] != null)
                StopCoroutine(threads[1]);
            threads[1] = ShowNoti(App.listKeyText["BET_CONFIRM"]/*"Bấm Xác nhận để hoàn tất đặt cược"*/,2f);
            StartCoroutine(threads[1]);
            boolLs[1] = true;
        }

        switch (type)
        {
            case "B":
                txts[1].text = App.formatMoneyD((float)(betCur[0] + nums[0]));
                betCur[0] += nums[0];
                break;
            case "C":
                txts[3].text = App.formatMoneyD((float)(betCur[2] + nums[0]));
                betCur[2] += nums[0];
                break;
            case "T":
                txts[5].text = App.formatMoneyD((float)(betCur[1] + nums[0]));
                betCur[1] += nums[0];
                break;
            case "CA":
                txts[7].text = App.formatMoneyD((float)(betCur[3] + nums[0]));
                betCur[3] += nums[0];
                break;
            case "G":
                txts[9].text = App.formatMoneyD((float)(betCur[4] + nums[0]));
                betCur[4] += nums[0];
                break;
            case "H":
                txts[11].text = App.formatMoneyD((float)(betCur[5] + nums[0]));
                betCur[5] += nums[0];
                break;
        }
    }

    private IEnumerator ShowNoti(string content,float time = 1f)
    {
        feastGojs[12].GetComponentInChildren<Text>().text = content;
        feastGojs[12].SetActive(true);
        yield return new WaitForSeconds(time);
        feastGojs[12].SetActive(false);
    }

    public void OpenGuide(bool open)
    {
        if (open)
        {
            feastGojs[3].SetActive(true);
            return;
        }
        feastGojs[3].SetActive(false);
    }

    public void DoAction(string type)
    {
        switch (type)
        {
            case "rebet":
                txts[1].text = App.formatMoneyD(betOld[0]);
                txts[3].text = App.formatMoneyD(betOld[2]);
                txts[5].text = App.formatMoneyD(betOld[1]);
                txts[7].text = App.formatMoneyD(betOld[3]);
                txts[9].text = App.formatMoneyD(betOld[4]);
                txts[11].text = App.formatMoneyD(betOld[5]);
                betCur[0] = betOld[0];
                betCur[1] = betOld[1];
                betCur[2] = betOld[2];
                betCur[3] = betOld[3];
                betCur[4] = betOld[4];
                betCur[5] = betOld[5];
                break;
            case "doublerebet":
                if (!boolLs[4])
                    return;
                txts[1].text = App.formatMoneyD(betOld[0]*2);
                txts[3].text = App.formatMoneyD(betOld[2]*2);
                txts[5].text = App.formatMoneyD(betOld[1]*2);
                txts[7].text = App.formatMoneyD(betOld[3]*2);
                txts[9].text = App.formatMoneyD(betOld[4]*2);
                txts[11].text = App.formatMoneyD(betOld[5]*2);
                betCur[0] = betOld[0]*2;
                betCur[1] = betOld[1]*2;
                betCur[2] = betOld[2]*2;
                betCur[3] = betOld[3]*2;
                betCur[4] = betOld[4]*2;
                betCur[5] = betOld[5]*2;
                break;
            case "delete":
                if (boolLs[0])
                {
                    txts[1].text = App.formatMoneyD(betOld[0]);
                    txts[3].text = App.formatMoneyD(betOld[2]);
                    txts[5].text = App.formatMoneyD(betOld[1]);
                    txts[7].text = App.formatMoneyD(betOld[3]);
                    txts[9].text = App.formatMoneyD(betOld[4]);
                    txts[11].text = App.formatMoneyD(betOld[5]);
                    betCur[0] = betOld[0];
                    betCur[1] = betOld[1];
                    betCur[2] = betOld[2];
                    betCur[3] = betOld[3];
                    betCur[4] = betOld[4];
                    betCur[5] = betOld[5];
                }
                else
                {
                    txts[1].text = "0";
                    txts[3].text = "0";
                    txts[5].text = "0";
                    txts[7].text = "0";
                    txts[9].text = "0";
                    txts[11].text = "0";
                    betCur[0] = 0;
                    betCur[1] = 0;
                    betCur[2] = 0;
                    betCur[3] = 0;
                    betCur[4] = 0;
                    betCur[5] = 0;
                }
                break;
            case "accept":
                Bet();
                break;
            case "newgame":
                txts[0].text = "0";
                txts[1].text = "0";
                txts[2].text = "0";
                txts[3].text = "0";
                txts[4].text = "0";
                txts[5].text = "0";
                txts[6].text = "0";
                txts[7].text = "0";
                txts[8].text = "0";
                txts[9].text = "0";
                txts[10].text = "0";
                txts[11].text = "0";
                betCur[0] = betCur[1] = betCur[2] = betCur[3] = betCur[4] = betCur[5] = 0;
                boolLs[0] = false;
                break;
        }
    }

    public void OpenPer100Times()
    {
        if (feastGojs[1].activeSelf)
        {
            feastGojs[2].SetActive(true);
            feastGojs[1].SetActive(false);
            return;
        }
        App.trace("[SEND]BAUCUA.REPORT");
        var req_Report = new OutBounMessage("BAUCUA.REPORT");
        req_Report.addHead();
        App.ws.send(req_Report.getReq(), delegate (InBoundMessage res) {
            App.trace("[RECV]BAUCUA.REPORT");
            int count = res.readInt();
            for(int i = 0; i < count; i++)
            {
                txts[i + 13].text = res.readInt().ToString();
            }
        });
        feastGojs[1].SetActive(true);
        feastGojs[2].SetActive(false);
    }
    private void CountDown(int time)
    {
        if (threads[0] != null)
            StopCoroutine(threads[0]);
        threads[0] = CountDown(time, txts[12]);
        StartCoroutine(threads[0]);
    }

    private IEnumerator CountDown(int time, Text txt)
    {
        int count = time;
        while (count > -1)
        {
            txt.text = "00:" + string.Format("{0:0,0}", count);
            yield return new WaitForSeconds(1f);
            count--;
        }
    }

    private void TextNumWin(string gate,string value)
    {
        int id = Int32.Parse(gate) - 1;
        GameObject txtWinClone = Instantiate(feastGojs[7], feastGojs[8].transform, false);
        txtWinClone.GetComponent<RectTransform>().anchoredPosition = btnsFeast[id].GetComponent<RectTransform>().anchoredPosition;
        txtWinClone.GetComponentInChildren<Text>().text = "+"+App.formatMoneyD(float.Parse(value));
        txtWinClone.SetActive(true);
        txtWinClone.GetComponent<RectTransform>().DOLocalMoveY(txtWinClone.GetComponent<RectTransform>().anchoredPosition.y + 20, 4f).SetEase(Ease.InOutElastic).OnComplete(() => {
        });
    }

    private IEnumerator PrePareAction()
    {
        yield return new WaitForSeconds(.2f);
        txts[1].text = App.formatMoneyD((float)betPerMatch[0]);
        txts[3].text = App.formatMoneyD((float)betPerMatch[2]);
        txts[5].text = App.formatMoneyD((float)betPerMatch[1]);
        txts[7].text = App.formatMoneyD((float)betPerMatch[3]);
        txts[9].text = App.formatMoneyD((float)betPerMatch[4]);
        txts[11].text = App.formatMoneyD((float)betPerMatch[5]);
    }
    private void RegHandler()
    {

        var req_PREPARE = new OutBounMessage("BAUCUA.PREPARE");
        req_PREPARE.addHead();
        handlers.Add("BAUCUA.PREPARE");
        App.ws.sendHandler(req_PREPARE.getReq(), delegate (InBoundMessage res_PREPARE)
        {
            boolLs[3] = false;
            btnsFeast[0].interactable = false;
            btnsFeast[1].interactable = false;
            btnsFeast[2].interactable = false;
            btnsFeast[3].interactable = false;
            btnsFeast[4].interactable = false;
            btnsFeast[5].interactable = false;
            btnsFeast[6].interactable = false;
            btnsFeast[7].interactable = false;
            btnsFeast[8].interactable = false;
            btnsFeast[9].interactable = false;
            txts[1].text = App.formatMoneyD((float)betPerMatch[0]);
            txts[3].text = App.formatMoneyD((float)betPerMatch[2]);
            txts[5].text = App.formatMoneyD((float)betPerMatch[1]);
            txts[7].text = App.formatMoneyD((float)betPerMatch[3]);
            txts[9].text = App.formatMoneyD((float)betPerMatch[4]);
            txts[11].text = App.formatMoneyD((float)betPerMatch[5]);
            for (int i = 0; i < feastGojs[8].transform.childCount; i++)
            {
                Destroy(feastGojs[8].transform.GetChild(i).gameObject);
            }
            DoAction("newgame");
            App.trace("[RECV] BAUCUA.PREPARE");
            res_PREPARE.readByte();
            CountDown(res_PREPARE.readByte());
            
        });

        var req_UpdatePot = new OutBounMessage("BAUCUA.UPDATE_POT");
        req_UpdatePot.addHead();
        handlers.Add("BAUCUA.UPDATE_POT");
        App.ws.sendHandler(req_UpdatePot.getReq(), delegate (InBoundMessage res_UpdatePot) {
            App.trace("[RECV] BAUCUA.UPDATE_POT");
            res_UpdatePot.readByte();
            res_UpdatePot.readInt();
            res_UpdatePot.readInt();
            res_UpdatePot.readInt();
            res_UpdatePot.readInt();
            res_UpdatePot.readInt();
            res_UpdatePot.readInt();
            txts[0].text = App.formatMoneyD(res_UpdatePot.readLong());
            txts[4].text = App.formatMoneyD(res_UpdatePot.readLong());
            txts[2].text = App.formatMoneyD(res_UpdatePot.readLong());
            txts[6].text = App.formatMoneyD(res_UpdatePot.readLong());
            txts[8].text = App.formatMoneyD(res_UpdatePot.readLong());
            txts[10].text = App.formatMoneyD(res_UpdatePot.readLong());
            
        });

        var req_Star = new OutBounMessage("BAUCUA.START");
        req_Star.addHead();
        handlers.Add("BAUCUA.START");
        App.ws.sendHandler(req_Star.getReq(), delegate (InBoundMessage res_Start) {
            App.trace("[RECV] BAUCUA.START");
            res_Start.readByte();           
            boolLs[4] = true;
            boolLs[3] = true;
            btnsFeast[0].interactable = true;
            btnsFeast[1].interactable = true;
            btnsFeast[2].interactable = true;
            btnsFeast[3].interactable = true;
            btnsFeast[4].interactable = true;
            btnsFeast[5].interactable = true;
            btnsFeast[6].interactable = true;
            btnsFeast[7].interactable = true;
            btnsFeast[8].interactable = true;
            btnsFeast[9].interactable = true;
            betPerMatch[0] = betPerMatch[1] = betPerMatch[2] = betPerMatch[3] = betPerMatch[4] = betPerMatch[5] = 0;
            feastGojs[10].SetActive(false);
            feastGojs[9].SetActive(false);
            feastGojs[2].SetActive(true);
            feastGojs[11].GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            
            DoAction("newgame");
            boolLs[2] = true;
            CountDown(res_Start.readByte());
            int sessionValue=res_Start.readInt();
            txtSession.text = "#" + sessionValue.ToString();
        });

        var req_Complete = new OutBounMessage("BAUCUA.COMPLETED");
        req_Complete.addHead();
        handlers.Add("BAUCUA.COMPLETED");
        App.ws.sendHandler(req_Complete.getReq(), delegate (InBoundMessage res_Complete) {
            App.trace("[RECV] BAUCUA.COMPLETED");
            res_Complete.readByte();         
            boolLs[2] = false;
            boolLs[3] = false;
            btnsFeast[0].interactable = false;
            btnsFeast[1].interactable = false;
            btnsFeast[2].interactable = false;
            btnsFeast[3].interactable = false;
            btnsFeast[4].interactable = false;
            btnsFeast[5].interactable = false;
            btnsFeast[6].interactable = false;
            btnsFeast[7].interactable = false;
            btnsFeast[8].interactable = false;
            btnsFeast[9].interactable = false;
            if (threads[2] != null)
                StopCoroutine(threads[2]);
            threads[2] = PrePareAction();
            StartCoroutine(threads[2]);
            feastGojs[2].SetActive(false);
            feastGojs[1].SetActive(false);
            feastGojs[10].SetActive(true);
            feastGojs[9].SetActive(true);
            feastGojs[9].transform.DOShakePosition(5f, 50f, 20);
            CountDown(res_Complete.readByte());
           
        });

        var req_ShowResult = new OutBounMessage("BAUCUA.SHOW_RESULT");
        req_ShowResult.addHead();
        handlers.Add("BAUCUA.SHOW_RESULT");
        App.ws.sendHandler(req_ShowResult.getReq(), delegate (InBoundMessage res_ShowResult) {
            App.trace("[RECV]BAUCUA.SHOW_RESULT");
            res_ShowResult.readByte();
            int mat1 = res_ShowResult.readByte(); // mat 1 cua xuc xac
            int mat2 = res_ShowResult.readByte(); // mat 2 cua xuc xac
            int mat3 = res_ShowResult.readByte(); // mat 3 cua xuc xac
            string result = res_ShowResult.readString(); // ket qua cua van dau
            boolLs[3] = false;
            btnsFeast[0].interactable = false;
            btnsFeast[1].interactable = false;
            btnsFeast[2].interactable = false;
            btnsFeast[3].interactable = false;
            btnsFeast[4].interactable = false;
            btnsFeast[5].interactable = false;
            btnsFeast[6].interactable = false;
            btnsFeast[7].interactable = false;
            btnsFeast[8].interactable = false;
            btnsFeast[9].interactable = false;          
            App.trace("xuc xac mat 1 : "+mat1+" || xuc xac mat 2 : "+mat2+" || xuc xac mat 3 : "+mat3+" || ket qua : "+result,"red");
            string[] perResult = result.Split('-');
            if (feastGojs[6].transform.parent.childCount >= 22)
            {
                DestroyObject(feastGojs[6].transform.parent.GetChild(1).gameObject);                
            }           
            GameObject historyNewClone = Instantiate(feastGojs[6], feastGojs[6].transform.parent, false);
            Image[] imageHisClone = historyNewClone.GetComponentsInChildren<Image>();
            for (int j = 0; j < perResult.Length; j++)
            {
                switch (perResult[j])
                {
                    case "1":
                        imgs[j].sprite = sprts[20];
                        SetIconHistory("Bầu", imageHisClone[j + j + 1]);
                        break;
                    case "2":
                        imgs[j].sprite = sprts[25];
                        SetIconHistory("Tôm", imageHisClone[j + j + 1]);
                        break;
                    case "3":
                        imgs[j].sprite = sprts[22];
                        SetIconHistory("Cua", imageHisClone[j + j + 1]);
                        break;
                    case "4":
                        imgs[j].sprite = sprts[21];
                        SetIconHistory("Cá", imageHisClone[j + j + 1]);
                        break;
                    case "5":
                        imgs[j].sprite = sprts[23];
                        SetIconHistory("Gà", imageHisClone[j + j + 1]);
                        break;
                    case "6":
                        imgs[j].sprite = sprts[24];
                        SetIconHistory("Hươu", imageHisClone[j + j + 1]);
                        break;
                }
                
            }
            historyNewClone.SetActive(true);
            feastGojs[11].transform.DOLocalMove(new Vector2(106,-133),.8f);
            
        });

        var req_DeviceChip = new OutBounMessage("BAUCUA.DIVIDE_CHIP");
        req_DeviceChip.addHead();
        handlers.Add("BAUCUA.DIVIDE_CHIP");
        App.ws.sendHandler(req_DeviceChip.getReq(), delegate (InBoundMessage res_Devide) {
            App.trace("[RECV]BAUCUA.DIVIDE_CHIP");
            res_Devide.readByte();
            string prize = res_Devide.readString();
            boolLs[3] = false;
            btnsFeast[0].interactable = false;
            btnsFeast[1].interactable = false;
            btnsFeast[2].interactable = false;
            btnsFeast[3].interactable = false;
            btnsFeast[4].interactable = false;
            btnsFeast[5].interactable = false;            
            string[] prizePerIcon = prize.Split(';');
            for(int i = 0; i < prizePerIcon.Length-1; i++)
            {
                string[] perVaue = prizePerIcon[i].Split('-');
                TextNumWin(perVaue[0],perVaue[1]);
            }
            App.trace("Prize : " + prize, "yellow");
            
        });

        var req_GameOver = new OutBounMessage("BAUCUA.GAMEOVER");
        req_GameOver.addHead();
        handlers.Add("BAUCUA.GAMEOVER");
        App.ws.sendHandler(req_GameOver.getReq(), delegate (InBoundMessage res_GameOver) {
            App.trace("[RECV]BAUCUA.GAMEOVER");
            res_GameOver.readByte();
            int count = res_GameOver.readByte();
            for (int i = 0; i < count; i++)
            {
                string result = res_GameOver.readString();
                //App.trace("result "+i+" : "+result,"yellow");
            }
            boolLs[1] = false;
            boolLs[3] = false;
            btnsFeast[0].interactable = false;
            btnsFeast[1].interactable = false;
            btnsFeast[2].interactable = false;
            btnsFeast[3].interactable = false;
            btnsFeast[4].interactable = false;
            btnsFeast[5].interactable = false;
            betPerMatch[0] = betPerMatch[1] = betPerMatch[2] = betPerMatch[3] = betPerMatch[4] = betPerMatch[5] = 0;            
            
            if (threads[0] != null)
                StopCoroutine(threads[0]);
           
        });
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

    public void Close()
    {
        //if (!boolLs[2])
        //    return;

        //DelHandlers();
        //App.trace("[SEND]BAUCUA.EXIT");
        //var req_TX_EXIT = new OutBounMessage("BAUCUA.EXIT");
        //req_TX_EXIT.addHead();
        //App.ws.send(req_TX_EXIT.getReq(), delegate (InBoundMessage res_TX_EXIT)
        //{
        //    App.trace("[RECV] BAUCUA.EXIT");
        //});

        //gameObject.SetActive(false);
        gameObject.transform.DOScale(0, 0.2f).OnComplete(()=> {
            myMoney = (int)CPlayer.chipBalance;
        });
    }
    private void EndSession()
    {
        DelHandlers();
        App.trace("[SEND]BAUCUA.EXIT");
        var req_TX_EXIT = new OutBounMessage("BAUCUA.EXIT");
        req_TX_EXIT.addHead();
        App.ws.send(req_TX_EXIT.getReq(), delegate (InBoundMessage res_TX_EXIT)
        {
        });
    }
    private void BeginAccessSession()
    {
        transform.SetSiblingIndex(22);
        boolLs[2] = true;
        boolLs[3] = true;
        boolLs[4] = true;
        betCur[0] = betCur[1] = betCur[2] = betCur[3] = betCur[4] = betCur[5] = 0;
        betPerMatch.Add(0);
        betPerMatch.Add(0);
        betPerMatch.Add(0);
        betPerMatch.Add(0);
        betPerMatch.Add(0);
        betPerMatch.Add(0);
        SetBet(1000);
        feastGojs[4].transform.parent.GetComponent<ContentSizeFitter>().enabled = false;
        threads = new IEnumerator[3];
        handlers = new List<string>();

        LoadData();
    }
}
