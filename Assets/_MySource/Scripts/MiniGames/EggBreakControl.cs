using DG.Tweening;
using Core.Server.Api;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EggBreakControl : MonoBehaviour {

    public Image[] imgs;

    /// <summary>
    /// 0: hisPanel|1: his-ele|12: help
    /// </summary>
    public GameObject[] gojs;

    /// <summary>
    /// 0-6: bet|7: pot|8: balance
    /// </summary>
    public Text[] txts;

    /// <summary>
    /// 0-6: bet|7: balance
    /// </summary>
    public Button[] btns;

    /// <summary>
    /// 0:bet-act|1:bet-unact
    /// </summary>
    public Sprite[] sprts;

    /// <summary>
    /// 0: white|1: yellow
    /// </summary>
    public Color[] colors;

    /// <summary>
    /// 0: nor|1: bold
    /// </summary>
    public Font[] fonts;

    /// <summary>
    /// 0-6: betAmount|7: curbetId|8: currbet
    /// </summary>
    private int[] numList = new int[9];

    private Egg[] eggs = new Egg[5];

    /// <summary>
    /// 0: isSpin
    /// </summary>
    private bool[] boolLs = new bool[2];

    private void OnEnable()
    {
        boolLs[0] = true;

        for (int i = 0; i < 5; i++)
        {
            GameObject[] eggGojs = new GameObject[10];
            App.trace(gojs[7 + i].transform.childCount.ToString());
            for (int j = 0; j < 10; j++)
            {
                
                eggGojs[j] = gojs[7 + i].transform.GetChild(j).gameObject;
            }
            eggs[i] = new Egg(eggGojs, gojs[7 + i].GetComponent<Animator>(), eggGojs[8].GetComponentInChildren<Text>(), eggGojs[8].GetComponent<Image>());
        }
        
        //5 eggs with nor animation in array
        List<int> arr = new List<int>();            //random list
        while (arr.Count < 5)
        {
            int rand = UnityEngine.Random.Range(0, 5);
            if (arr.Contains(rand))
                continue;
            arr.Add(rand);
        }
        for (int i = 0; i < 5; i++)
        {
            //gojs[7 + i].transform.GetChild(1 + arr[i]).gameObject.SetActive(true);
            eggs[i].Fxs[1 + arr[i]].SetActive(true);
            //gojs[7 + i].GetComponent<Animator>().Play("Egg_state_0" + (arr[i] + 1));
            eggs[i].Anim.Play("Egg_state_0" + (arr[i] + 1));
            //gojs[7 + i].transform.localPosition = new Vector2(-568 + 284 * i, -43);
            gojs[7 + i].transform.localPosition = new Vector2(-568 + 284 * i, -43);
        }
        //return;
        /*
        //5 eggs with nor animation
        List<int> arr = new List<int>();
        while(arr.Count < 5)
        {
            int rand = UnityEngine.Random.Range(0, 5);
            if (arr.Contains(rand))
                continue;
            arr.Add(rand);
        }

        for (int i = 0; i < 5; i++)
        {
            gojs[7 + i].transform.GetChild(1 + arr[i]).gameObject.SetActive(true);
            gojs[7 + i].GetComponent<Animator>().Play("Egg_state_0" + (arr[i] + 1));
            gojs[7 + i].transform.localPosition = new Vector2(-568 + 284 * i, -43);
        }
        

        //Invoke("ResetEgg", 10f);
        */
        numList[7] = -1;
        LoadData();
        CPlayer.potchangedDapTrung += PotChanged;
    }

    public void LoadData()
    {
        txts[8].text = App.FormatMoney(CPlayer.chipBalance);


        CPlayer.changed += EggBalanceChanged;

        var req_INFO = new OutBounMessage("EGGY.GET_INFO");
        req_INFO.addHead();
        App.ws.send(req_INFO.getReq(), delegate (InBoundMessage res_INFO)
         {
             App.trace("[RECV] EGGY.GET_INFO");
             int count = res_INFO.readByte();
             for (int i = 0; i < count; i++)
             {
                 string valS = res_INFO.readString();
                 int valI = res_INFO.readInt();
                 txts[i].text = valS;
                 App.trace(valS + "|" + valI);
                 numList[i] = valI;
             }

             int potBreak = res_INFO.readInt();
             txts[7].text = App.FormatMoney(potBreak);
             count = res_INFO.readByte();
             for (int i = 0; i < count; i++)
             {
                 string txt = res_INFO.readString();

             }

             ChangeBet(1);
         });
    }

    public void ShowHis()
    {
        if (boolLs[0] == false)
            return;
        if (gojs[0].activeSelf == true)
        {
            gojs[0].SetActive(false);
        }
        else
        {
            gojs[0].SetActive(true);

            foreach (Transform rtf in gojs[1].transform.parent)       //Delete exits element before
            {
                if (rtf.gameObject.name != gojs[1].name)
                {
                    Destroy(rtf.gameObject);
                }
            }

            ShowHis(0);
        }
    }

    private void ShowHis(int page)
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
        req_HIS.writeString("daptrung");         //game name
        req_HIS.writeByte(1);                       //page index
        req_HIS.writeString("");                    //from date
        req_HIS.writeString("");                    //to date
        App.ws.send(req_HIS.getReq(), delegate (InBoundMessage res_HIS) {
            int count = res_HIS.readByte();
            App.trace("Count = " + count);
            for (int i = 0; i < count; i++)
            {
                long index = res_HIS.readLong();
                string time = res_HIS.readString();
                string game = res_HIS.readString();
                string bet = res_HIS.readString();
                string change = res_HIS.readString();
                string balance = res_HIS.readString();
                App.trace("id = " + index + "| time = " + time + "|game = " + game + "|bet = " + bet + "|change = " + change + "|balan = " + balance);

                GameObject goj = Instantiate(gojs[1], gojs[1].transform.parent, false);
                Text[] txtArr = goj.GetComponentsInChildren<Text>();
                txtArr[0].text = index.ToString();
                txtArr[1].text = time;
                txtArr[2].text = App.FormatMoney(bet);
                txtArr[3].text = App.FormatMoney(change);
                txtArr[4].text = App.FormatMoney(balance);
                goj.SetActive(true);
            }
        });


        /*
        var req_EGG_HIS = new OutBounMessage("EGGY.GET_HISTORY");
        req_EGG_HIS.addHead();
        App.ws.send(req_EGG_HIS.getReq(), delegate (InBoundMessage res_EGG_HIS)
        {
            int count = res_EGG_HIS.readByte();
            for (int i = 0; i < count; i++)
            {
                int stt = res_EGG_HIS.readInt();
                string time = res_EGG_HIS.readString();
                string betAmount = res_EGG_HIS.readString();
                string change = res_EGG_HIS.readString();
                App.trace("stt = " + stt + "|time = " + time + "|bet = " + betAmount + "|change = " + change);
            }
        });
        */
    }

    public void Spin(int eggId)
    {
        if (boolLs[0] == false)
            return;

        

        var req_EGG_START = new OutBounMessage("EGGY.START");
        req_EGG_START.addHead();
        req_EGG_START.writeInt(numList[8]);
        //App.trace(numList[8], "green");
        App.ws.send(req_EGG_START.getReq(), delegate (InBoundMessage res_EGG_START)
        {
            boolLs[0] = false;
            List<int[]> otherPrize = new List<int[]>();

            int prize = res_EGG_START.readInt();
            //otherPrize[eggId] = prize;         //Set prize for user
            int isPotBreak = res_EGG_START.readByte();          //true if = 1;
            int count = res_EGG_START.readByte();
            App.trace("COUNT = " + count + "|" + prize);
            //return;
            List<int> arr = new List<int>();            //random list
            while (arr.Count < 4)
            {
                int rand = UnityEngine.Random.Range(0, count-1);
                if (arr.Contains(rand))
                    continue;
                arr.Add(rand);
                App.trace("PHAC = " + rand, "yellow");
            }
            arr.Add(count-1);
            //App.trace("PHAC = " + arr.Count, "yellow");
            for (int i = 0; i < count; i++)
            {
                int val = res_EGG_START.readInt();
                int isPot = res_EGG_START.readByte();       //==1 is true
                //App.trace("PHẮC CỪN WAO. HŨ NÀI " + val + "|" + isPot + "|" + i, "green");
                if (arr.Contains(i))
                {
                    //if (isPot == 1)
                        //App.trace("CÁI ĐCM", "yellow");
                    otherPrize.Add(new int[] {val, isPot });
                    //App.trace("PHẮC CỪN WAO. HŨ NÀI " + val + "|" + isPot, "green");
                }
            }

            int tmp = UnityEngine.Random.Range(0, 5);
            int[] tmpArr = otherPrize[4];
            otherPrize[4] = otherPrize[tmp];
            otherPrize[tmp] = tmpArr;

            otherPrize[eggId] = new int[] { prize, isPotBreak };
            //otherPrize[eggId] = new int[] { prize, 1 };

            long id = res_EGG_START.readLong();
            bool isBigWin = res_EGG_START.readByte() == 1;
            if (isBigWin)
            {

            }
            //App.trace("[RECV] EGGY.START won = " + prize + "| id = " + id + "|count = " + count);
            BreakTheEgg(eggId, otherPrize);
        });


        
    }

    public void Close()
    {
        if (boolLs[0] == false)
            return;
        for (int i = 0; i < 5; i++)
        {
            foreach (var item in eggs[i].Fxs)
            {
                if(item.name != "Body")
                    item.SetActive(false);
            }
        }
        CPlayer.potchangedDapTrung -= PotChanged;
        CPlayer.changed -= EggBalanceChanged;
        for (int i = 0; i < 7; i++)
        {
                btns[i].image.sprite = sprts[1];
        }
        gameObject.SetActive(false);
    }

    public void CloseHis()
    {
        gojs[0].SetActive(false);
    }

    public void ShowHelp(bool isOpen)
    {
        if(isOpen== false)
            gojs[12].SetActive(false);
        else
            gojs[12].SetActive(true);
    }

    public void ChangeBet(int id)
    {
        if (boolLs[0] == false)
            return;
        if (numList[7] == id)
            return;
        if(numList[7] > -1)
            btns[numList[7]].image.sprite = sprts[1];

        numList[7] = id;
        numList[8] = numList[id];
        btns[id].image.sprite = sprts[0];
        //App.trace("PhẮC" + numList[8]);
    }

    private void EggBalanceChanged(string type)
    {
        if (type == "chip")
        {
            //txts[8].text = string.Format("{0:0,0}", CPlayer.chipBalance);
            //return;
            if (CPlayer.preChipBalance >= CPlayer.chipBalance)
            {
                txts[8].text = string.Format("{0:0,0}", CPlayer.chipBalance);
            }
        }
    }

    private void showBigWin()
    {

    }

    private IEnumerator BreakTheEgg(int id, float delay = 0, bool isFirst = false, int value = 0, bool isPot = false, bool isMe = false)
    {
        //yield return new WaitForSeconds(2f);
        if (isPot)
            App.trace("BỐ M LÀ HŨ ĐÂY " + id, "red");
        if(delay > 0)
            yield return new WaitForSeconds(delay);

        //gojs[7].transform.GetChild(7).gameObject.SetActive(true);
        eggs[id].Fxs[7].SetActive(true);
        //gojs[7].transform.GetChild(7).gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);
        //gojs[7].transform.GetChild(8).gameObject.SetActive(true);
        if (isPot)
        {
            eggs[id].Pot.sprite = sprts[2];
            if(isMe)
                eggs[id].Fxs[9].SetActive(true);
        }
        else
            eggs[id].Pot.sprite = sprts[3];
        eggs[id].Fxs[0].SetActive(false);
        eggs[id].Fxs[8].SetActive(true);
        //gojs[7].GetComponent<Animator>().Play("Idle");\
        eggs[id].Anim.Play("Idle");
        //StartCoroutine(TweenNum(gojs[7].transform.GetChild(8).GetComponentInChildren<Text>(), 0, 123456, 3, 1));

        eggs[id].Prize.color = colors[0];
        eggs[id].Prize.font = fonts[0];
        if (isFirst)
        {
            eggs[id].Prize.color = colors[1];
            eggs[id].Prize.font = fonts[1];
        }

        StartCoroutine(TweenNum(eggs[id].Prize, 0, value, 3, 1));
        if (isMe && numList[7] != 0)            //if its not fake spin
        {
            StartCoroutine(TweenNum(txts[8], (int)CPlayer.preChipBalance, (int)CPlayer.chipBalance, 2, 1, 2f));
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

    private class Egg
    {
        private GameObject[] fxs;
        private Animator anim;
        private Text prize;
        private Image pot;

        public GameObject[] Fxs
        {
            get
            {
                return fxs;
            }

            set
            {
                fxs = value;
            }
        }

        public Animator Anim
        {
            get
            {
                return anim;
            }

            set
            {
                anim = value;
            }
        }

        public Text Prize
        {
            get
            {
                return prize;
            }

            set
            {
                prize = value;
            }
        }

        public Image Pot
        {
            get
            {
                return pot;
            }

            set
            {
                pot = value;
            }
        }

        public Egg(GameObject[] fxs, Animator anim, Text prize, Image pot)
        {
            this.Fxs = fxs;
            this.Anim = anim;
            this.Prize = prize;
            this.Pot = pot;
        }
    }

    private void ResetEgg()
    {
        for (int i = 0; i < 5; i++)
        {
            foreach (var item in eggs[i].Fxs)
            {
                if (item.name != "Body")
                    item.SetActive(false);
                else
                    item.SetActive(true);
            }
        }

        List<int> arr = new List<int>();            //random list
        while (arr.Count < 5)
        {
            int rand = UnityEngine.Random.Range(0, 5);
            if (arr.Contains(rand))
                continue;
            arr.Add(rand);
        }
        for (int i = 0; i < 5; i++)
        {
            eggs[i].Fxs[1 + arr[i]].SetActive(true);
            eggs[i].Anim.Play("Egg_state_0" + (arr[i] + 1));
            gojs[7 + i].transform.localPosition = new Vector2(-568 + 284 * i, -43);
        }

        boolLs[0] = true;
    }

    public void BreakTheEgg(int id, List<int[]> prizes)
    {
        /*
        if (boolLs[0] == false)
            return;
            
        boolLs[0] = false;
        */
        foreach (var item in eggs[id].Fxs)
        {
            if (item.name != "Body")
                item.SetActive(false);
        }
        eggs[id].Fxs[6].SetActive(true);

        StartCoroutine(BreakTheEgg(id, 0f, true, prizes[id][0], prizes[id][1]==1, true));
        for (int i = 0; i < 5; i++)
        {
            if(id != i)
                StartCoroutine(BreakTheEgg(i, 0f,false, prizes[i][0], prizes[i][1] == 1));
        }
        Invoke("ResetEgg", 3.5f);
    }

    private void PotChanged()
    {
        InBoundMessage res = CPlayer.res_potMiniGameDapTrung;
        int count = res.readByte();
        for (int i = 0; i < count; i++)
        {
            string gameId = res.readString();
            int count1 = res.readByte();
            //if(gameId == "xengfull")
            //App.trace("ĐẸT " + gameId + "green");
            //App.trace("<color=green>[MINI POKER] = </color>" + gameId);
            for (int j = 0; j < count1; j++)
            {
                int bet = res.readInt();
                int value = res.readInt();
                if(gameId == "daptrung")
                {
                    int pre = (int)App.formatMoneyBack(txts[7].text);
                    if (pre < value)
                        StartCoroutine(TweenNum(txts[7], pre, value, 3, 1f, 0));
                    else
                        txts[7].text = App.FormatMoney(value);
                    break;
                }
            }
            if (gameId == "daptrung")
            {
                break;
            }
        }
    }
}
