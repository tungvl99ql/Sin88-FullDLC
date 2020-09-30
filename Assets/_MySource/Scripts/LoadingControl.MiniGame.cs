using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;
using System;
using System.Linq;
using Core.Server.Api;

namespace Core.Server.Api
{


    public partial class LoadingControl : MonoBehaviour
    {

        //#region //IQUAY


        //[Header("=====IQUAY - XÈNG=====")]
        //public GameObject iQuayPanel;


        //public Toggle[] iQuaytopTogList, iQuayManChipTog;
        ///// <summary>
        ///// Các thành phần Body chính 1: Nội dung|2: Tham gia|3: His|4: Toast Panel|5: Blur Image
        ///// </summary>
        //public GameObject[] iQuayMainPanels;
        ///// <summary>
        ///// Các Element để Instiate trong Iquay 1:His|2: Ô chọn các mức cược|3: GUID
        ///// </summary>
        //public GameObject[] iQuayElements;
        ///// <summary>
        ///// Danh sách các màu IQUAY 1: alpha = 64| 2: alpha = 255|3: balck|4: red|5: yellow
        ///// </summary>
        //public Color[] iQuayColorList;
        ///// <summary>
        ///// Danh sach vong quay IQUAY 1-12: 12 item|13-14: bg left-right|15-16-17: 3 ảnh kết quả|18: Ảnh nhận thưởng|19: Ảnh nút Quay
        ///// </summary>
        //public Image[] iQuayIcoList;
        ///// <summary>
        ///// Danh sách các sprites QUAY 0-1: bg left|2-3: bg_right | 4-12: 11 sprites| 12: sprite mặc định 3 v Quay|13-14: sprite kquả|15-16: sprite quay
        ///// </summary>
        //public Sprite[] iQuaySprites;
        ///// <summary>
        ///// Danh sách các text IQuay 0: MID - KQUẢ|1: btn kquả|2: btn quay|3: Man|4: Chip|5: Blur Text
        ///// </summary>
        //public Text[] iQuayTextList;
        //private int round = 0;
        //private string items = "none";
        //private string prize = "";
        //private int fee = 0;
        //private bool getSuccess = false;
        //private bool allowGet;
        //private bool allowClick = true; // khong cho phep tat khi dang quay
        //private bool allowSpin = true;
        //private int activeBet;
        //private string feeType = "";
        //private bool isSpining = false;
        //private bool iQuayIsPlaying = false;
        //private List<int> iQuayManList, iQuayChipList;
        //private List<IQuayGuide> iQuayGuideList;
        //private class IQuayGuide
        //{
        //    private int round;
        //    private string preItem, nextItem, prize;

        //    public int Round
        //    {
        //        get
        //        {
        //            return round;
        //        }

        //        set
        //        {
        //            round = value;
        //        }
        //    }

        //    public string PreItem
        //    {
        //        get
        //        {
        //            return preItem;
        //        }

        //        set
        //        {
        //            preItem = value;
        //        }
        //    }

        //    public string NextItem
        //    {
        //        get
        //        {
        //            return nextItem;
        //        }

        //        set
        //        {
        //            nextItem = value;
        //        }
        //    }

        //    public string Prize
        //    {
        //        get
        //        {
        //            return prize;
        //        }

        //        set
        //        {
        //            prize = value;
        //        }
        //    }

        //    public IQuayGuide(int round, string preItem, string nextItem, string prize)
        //    {
        //        this.Round = round;
        //        this.PreItem = preItem;
        //        this.NextItem = nextItem;
        //        this.Prize = prize;
        //    }
        //}

        //public void showIQuay(bool toShow)
        //{
        //    if (isSpining)
        //    {
        //        return;
        //    }
        //    RectTransform rtf = iQuayPanel.GetComponent<RectTransform>();
        //    if (toShow)
        //    {
        //        CPlayer.currMiniGame = "iquay";

        //        preIQuayBetTogId = -1;

        //        var req = new OutBounMessage("XENG.GET_INFO");
        //        req.addHead();
        //        //loadingScene.SetActive(true);
        //        LoadingUIPanel.Show();
        //        App.ws.send(req.getReq(), delegate (InBoundMessage res)
        //        {
        //            if (iQuayManList == null)
        //            {
        //                iQuayManList = new List<int>();
        //            }
        //            else
        //            {
        //                iQuayManList.Clear();
        //            }
        //            if (iQuayChipList == null)
        //            {
        //                iQuayChipList = new List<int>();
        //            }
        //            else
        //            {
        //                iQuayChipList.Clear();
        //            }
        //            string man = res.readAscii();
        //            var countManList = res.readByte();  //số mức cược của Man
        //            for (int i = 0; i < countManList; i++)
        //            {
        //                //App.trace(res.readInt().ToString());
        //                iQuayManList.Add(res.readInt());
        //            }

        //            string chip = res.readAscii();
        //            var countChipList = res.readByte(); //số mức cược của Chip
        //            for (int i = 0; i < countChipList; i++)
        //            {
        //                //App.trace(res.readInt().ToString());
        //                iQuayChipList.Add(res.readInt());
        //            }

        //            bool isPlaying = res.readByte() == 1;   //True: Đang chơi | False: K
        //            if (isPlaying)
        //            {
        //                iQuayIsPlaying = true;
        //                this.fee = activeBet = res.readInt(); // mức cược
        //                this.feeType = res.readAscii(); // kiểu MAN - CHIP
        //                this.round = res.readInt(); // luot hien tai
        //                this.items = res.readAscii(); // danh sach cac phan thuong
        //                this.prize = res.readAscii(); // gia tri phan thuong (x ...)
        //                var prizeType = res.readAscii(); // kieu phan thuong MAN _ CHIP
        //                this.allowGet = res.readByte() == 1 ? true : false; //co nhan thuong dc ko
        //                this.allowSpin = res.readByte() == 1 ? true : false; // co quay tiep dc ko;
        //                App.trace("luc dau vao = allowSpin" + this.allowSpin + "allowGet = " + allowGet + "|feeType = " + feeType + "|fee = " + fee);


        //            }
        //            showIQuayRsBox();
        //            iQuaytopTogList[1].isOn = true;
        //            preIQuayTopTogId = -1;
        //            changeTopTogIQuay(1);
        //            updateIQuayBtnState();
        //            rtf.anchoredPosition = new Vector2(0, 960);
        //            iQuayPanel.SetActive(true);
        //            DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, Vector2.zero, .35f).OnComplete(() =>
        //            {
        //                //exchangeScene.SetActive(false);
        //                //loadingScene.SetActive(false);
        //                LoadingUIPanel.Hide();
        //                spinIQuay2();
        //            });
        //        });

        //        var reqGuid = new OutBounMessage("XENG.GET_GUIDE");
        //        reqGuid.addHead();
        //        App.ws.send(reqGuid.getReq(), delegate (InBoundMessage res)
        //        {
        //            if (iQuayGuideList == null)
        //            {
        //                iQuayGuideList = new List<IQuayGuide>();
        //            }
        //            else
        //            {
        //                iQuayGuideList.Clear();
        //            }

        //            int count = res.readByte();
        //            for (int i = 0; i < count; i++)
        //            {
        //                int round = res.readInt();
        //                string preItem = res.readAscii();
        //                string nextItem = res.readAscii();
        //                string prize = res.readAscii();
        //                //App.trace("round = " + round + " pre = " + preItem + " next = " + nextItem + " pirze = " + prize);
        //                iQuayGuideList.Add(new IQuayGuide(round, preItem, nextItem, prize));
        //            }
        //        });
        //        setIQuayBalance("chip");
        //        setIQuayBalance("man");
        //        CPlayer.changed += setIQuayBalance;
        //        return;
        //    }
        //    CPlayer.changed -= setIQuayBalance;
        //    DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, new Vector2(0, 1200), .35f).OnComplete(() =>
        //    {
        //        MiniGameController.instance.close();
        //    });
        //}

        //private void setIQuayBalance(string type)
        //{
        //    if (type == "man")
        //        iQuayTextList[3].text = CPlayer.manBalance >= 0 ? App.formatMoney(CPlayer.manBalance.ToString()) : "0";
        //    if (type == "chip")
        //    {
        //        iQuayTextList[4].text = CPlayer.chipBalance >= 0 ? App.formatMoney(CPlayer.chipBalance.ToString()) : "0";

        //    }
        //}

        //private int preIQuayTopTogId = -1;
        //public void changeTopTogIQuay(int id)
        //{
        //    /*
        //    if (isSpining)
        //        return;
        //        */
        //    if (preIQuayTopTogId != id && iQuaytopTogList[id].isOn)
        //    {
        //        if (preIQuayTopTogId > -1)
        //            iQuayMainPanels[preIQuayTopTogId].SetActive(false);
        //        iQuayMainPanels[id].SetActive(true);

        //        preIQuayTopTogId = id;

        //        _changeTopTogIQuay(id);



        //    }
        //}

        //private void _changeTopTogIQuay(int mType)
        //{
        //    switch (mType)
        //    {
        //        case 2: //SHOW LỊCH SỬ
        //            var req = new OutBounMessage("XENG.GET_HISTORY");
        //            req.addHead();
        //            //loadingScene.SetActive(true);
        //            LoadingUIPanel.Show();
        //            App.ws.send(req.getReq(), delegate (InBoundMessage res)
        //            {
        //                Transform tfm = iQuayElements[0].gameObject.transform.parent;
        //                int count = tfm.childCount;
        //                for (int i = count - 1; i > 0; i--)
        //                {
        //                    Destroy(tfm.GetChild(i).gameObject, .1f);
        //                }

        //                count = res.readByte();
        //                for (int i = 0; i < count; i++)
        //                {
        //                    string nickName = res.readString();
        //                    string desc = res.readString();
        //                    string type = res.readAscii();
        //                    string time = res.readString();

        //                    GameObject tmp = Instantiate(iQuayElements[0], iQuayElements[0].transform.parent, false) as GameObject;
        //                    Text[] txtArr = tmp.GetComponentsInChildren<Text>();
        //                    txtArr[0].text = nickName;
        //                    txtArr[1].text = desc + " " + type;
        //                    txtArr[2].text = time;
        //                    tmp.SetActive(true);
        //                }
        //                //loadingScene.SetActive(false);
        //                LoadingUIPanel.Hide();
        //            });
        //            break;
        //        case 1: //SHOW QUAY
        //            /*
        //            if (feeType == "")
        //            {
        //                preiQuayManChipId = -1;
        //                iQuayManChipTog[0].isOn = false;
        //                iQuayManChipTog[1].isOn = true;
        //                changeIQuayManChip(0);
        //            }
        //            if (feeType == "man")
        //            {
        //                preiQuayManChipId = -1;
        //                iQuayManChipTog[0].isOn = false;
        //                iQuayManChipTog[1].isOn = true;
        //                changeIQuayManChip(0);
        //            }
        //            if (feeType == "chip")
        //            {
        //                preiQuayManChipId = -1;
        //                iQuayManChipTog[0].isOn = false;
        //                iQuayManChipTog[1].isOn = true;
        //                changeIQuayManChip(0);
        //            }
        //            */
        //            if (iQuayIsPlaying == false && preiQuayManChipId != -2)
        //            {
        //                //preiQuayManChipId = -1;
        //                iQuayManChipTog[0].isOn = false;
        //                iQuayManChipTog[1].isOn = true;
        //                changeIQuayManChip(1);
        //            }
        //            updateIQuayBtnState();
        //            break;
        //        case 0: //SHOW GUIDE
        //            Func<string, Sprite> getSprite = new Func<string, Sprite>((string s) =>
        //            {
        //                for (int i = 0; i < 8; i++)
        //                {
        //                    if ("icon_" + s == iQuaySprites[i + 4].name)
        //                    {
        //                        return iQuaySprites[i + 4];
        //                    }
        //                }
        //                return null;
        //            });
        //            Transform tfm1 = iQuayElements[2].gameObject.transform.parent;
        //            int count1 = tfm1.childCount;
        //            for (int i = count1 - 1; i > 0; i--)
        //            {
        //                Destroy(tfm1.GetChild(i).gameObject);
        //            }
        //            string[] tmpArr = items.Split(';');
        //            string tmpPrize = (tmpArr.Length > 0 && prize != "0.0") ? tmpArr[tmpArr.Length - 1] : "none";
        //            for (int i = 0; i < iQuayGuideList.Count; i++)
        //            {
        //                if (iQuayGuideList[i].PreItem == tmpPrize)
        //                {
        //                    GameObject goj = Instantiate(iQuayElements[2], iQuayElements[2].transform.parent, false) as GameObject;
        //                    goj.GetComponentsInChildren<Text>()[1].text = "x" + iQuayGuideList[i].Prize;
        //                    goj.GetComponentsInChildren<Image>()[1].sprite = getSprite(iQuayGuideList[i].NextItem);
        //                    goj.SetActive(true);
        //                }
        //            }
        //            iQuayElements[2].transform.parent.gameObject.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        //            break;
        //    }
        //}

        //private void updateIQuayBtnState()
        //{
        //    if (prize == "0.0" || prize == "")
        //    {
        //        allowSpin = true;
        //        iQuayTextList[2].text = "BẮT ĐẦU";
        //        iQuayMainPanels[4].SetActive(false);
        //    }
        //    else
        //    {
        //        iQuayTextList[2].text = "QUAY TIẾP";
        //        iQuayTextList[5].text = "CƯỢC " + App.formatMoney(fee != 0 ? fee.ToString() : activeBet.ToString()) + " " + feeType.ToUpper();
        //        iQuayMainPanels[4].SetActive(true);
        //    }

        //    iQuayIcoList[17].sprite = allowGet ? iQuaySprites[13] : iQuaySprites[14];
        //    iQuayIcoList[18].sprite = allowSpin ? iQuaySprites[15] : iQuaySprites[16];
        //    iQuayTextList[1].color = allowGet ? iQuayColorList[4] : iQuayColorList[2];
        //    iQuayTextList[2].color = allowSpin ? iQuayColorList[3] : iQuayColorList[2];
        //}


        //private int preiQuayManChipId = -1;
        //public void changeIQuayManChip(int id)
        //{

        //    if (preiQuayManChipId != id && iQuayManChipTog[id].isOn)
        //    {

        //        if (id == 1)
        //        {
        //            iQuayTextList[4].transform.parent.gameObject.SetActive(true);
        //            iQuayTextList[3].transform.parent.gameObject.SetActive(false);
        //        }
        //        else
        //        {
        //            iQuayTextList[4].transform.parent.gameObject.SetActive(false);
        //            iQuayTextList[3].transform.parent.gameObject.SetActive(true);
        //        }

        //        preiQuayManChipId = id;
        //        Transform tfm = iQuayElements[1].gameObject.transform.parent;
        //        int count = tfm.childCount;
        //        for (int i = count - 1; i > 0; i--)
        //        {
        //            Destroy(tfm.GetChild(i).gameObject);
        //        }
        //        if (iQuayBetTogList == null)
        //        {
        //            iQuayBetTogList = new List<Toggle>();
        //        }
        //        else
        //        {
        //            iQuayBetTogList.Clear();
        //        }


        //        count = id == 0 ? iQuayManList.Count : iQuayChipList.Count;
        //        for (int i = 0; i < count; i++)
        //        {
        //            GameObject tmp = Instantiate(iQuayElements[1], iQuayElements[1].transform.parent, false) as GameObject;
        //            Text txt = tmp.GetComponentInChildren<Text>();
        //            txt.text = id == 0 ? App.formatMoney(iQuayManList[i].ToString()) : App.formatMoney(iQuayChipList[i].ToString());
        //            tmp.SetActive(true);
        //            int bet = i;
        //            Toggle tog = tmp.GetComponent<Toggle>();
        //            tog.onValueChanged.AddListener(delegate (bool on)
        //            {
        //                // App.trace(i.ToString());
        //                changeIQuayBet(bet, id == 0 ? iQuayManList[bet] : iQuayChipList[bet], id);
        //            });
        //            iQuayBetTogList.Add(tog);
        //        }

        //        if (fee != 0)
        //        {
        //            preIQuayBetTogId = id == 0 ? iQuayManList.IndexOf(activeBet) : iQuayChipList.IndexOf(activeBet);
        //            changeIQuayBet(preIQuayBetTogId, id == 0 ? iQuayManList[preIQuayBetTogId] : iQuayChipList[preIQuayBetTogId], id == 0 ? 0 : 1);
        //        }
        //        else
        //        {
        //            preIQuayBetTogId = -1;
        //            changeIQuayBet(1, id == 0 ? iQuayManList[1] : iQuayChipList[1], id == 0 ? 0 : 1);
        //        }

        //        /*
        //        if (feeType == "")
        //            changeIQuayBet(1, iQuayChipList[1], 1);
        //        else if (feeType == "man")
        //        {
        //            changeIQuayBet(iQuayManList.IndexOf(fee), fee, 0);
        //        }
        //        else if (feeType == "chip")
        //        {
        //            changeIQuayBet(iQuayChipList.IndexOf(fee), fee, 1);
        //        }
        //        */
        //    }
        //}

        //private int preIQuayBetTogId = -1;
        //private List<Toggle> iQuayBetTogList;
        ///// <summary>
        ///// THAY ĐỔI ĐỔI MỨC CƯỢC
        ///// </summary>
        //private void changeIQuayBet(int id, int betAmount, int feeTypeId)
        //{
        //    if (id == preIQuayBetTogId)
        //    {
        //        return;
        //    }

        //    for (int i = 0; i < iQuayBetTogList.Count; i++)
        //    {
        //        if (i == id)
        //        {
        //            iQuayBetTogList[i].isOn = true;
        //            iQuayBetTogList[i].interactable = false;
        //            continue;
        //        }
        //        iQuayBetTogList[i].isOn = false;
        //        iQuayBetTogList[i].interactable = true;
        //    }
        //    preIQuayBetTogId = id;
        //    this.activeBet = betAmount;
        //    this.feeType = feeTypeId == 0 ? "man" : "chip";
        //}

        //private int iQuayLastPrizeId = 0;
        //private void spinIQuay(bool isFirst = false)
        //{
        //    if (isSpining)
        //    {
        //        return;
        //    }

        //    for (int i = 0; i < 12; i++)
        //    {
        //        iQuayIcoList[i].color = iQuayColorList[0];
        //    }
        //    string[] itemList = items.Split(';');
        //    for (int i = 0; i < 12; i++)
        //    {
        //        //App.trace(iQuayIcoList[i].sprite.name);
        //        if (iQuayIcoList[i].sprite.name == "icon_" + itemList[itemList.Length - 1])
        //        {
        //            changeTopTogIQuay(0);
        //            StartCoroutine(_spinIQuay(i, .005f, isFirst));
        //            break;
        //        }
        //    }


        //}

        //private void spinIQuay2()
        //{
        //    for (int i = 0; i < 12; i++)
        //    {
        //        iQuayIcoList[i].color = iQuayColorList[1];
        //    }
        //    if (items == "none" || items == "" || prize == "0.0" || prize == "")
        //    {
        //        StartCoroutine(_spinIQuay2());
        //        return;
        //    }
        //    string[] itemList = items.Split(';');
        //    for (int i = 0; i < 12; i++)
        //    {
        //        //App.trace(iQuayIcoList[i].sprite.name);
        //        if (iQuayIcoList[i].sprite.name == "icon_" + itemList[itemList.Length - 1])
        //        {
        //            //changeTopTogIQuay(0);
        //            StartCoroutine(_spinIQuay2(i));
        //            break;
        //        }
        //    }


        //}

        //private IEnumerator _spinIQuay2(int last = 0)
        //{
        //    isSpining = true;
        //    int count = 0;

        //    int spinNum = 12 + last;
        //    for (int i = 0; i < spinNum; i++)
        //    {

        //        count++;
        //        if (count > 11)
        //        {
        //            count = 0;
        //        }
        //        iQuayIcoList[count].color = i < 12 ? iQuayColorList[0] : iQuayColorList[1];

        //        yield return new WaitForSeconds(.2f);
        //        if (i > 11)
        //            iQuayIcoList[count].color = iQuayColorList[0];
        //        iQuayIcoList[12].sprite = iQuaySprites[i % 2 == 0 ? 1 : 0];
        //        iQuayIcoList[13].sprite = iQuaySprites[i % 2 == 0 ? 3 : 2];
        //    }
        //    for (int i = 0; i < 5; i++)
        //    {
        //        iQuayIcoList[count].color = iQuayColorList[i % 2 == 0 ? 0 : 1];
        //        yield return new WaitForSeconds(.3f);

        //    }
        //    iQuayIcoList[count].color = iQuayColorList[1];
        //    isSpining = false;
        //    yield break;
        //}

        //private IEnumerator _spinIQuay(int spinTo, float acceleration = 0, bool isFirst = false)
        //{
        //    isSpining = true;
        //    int count = iQuayLastPrizeId;
        //    spinTo = spinTo - iQuayLastPrizeId;
        //    if (spinTo < 0)
        //        spinTo += 12;
        //    int spinNum = 12 * 3 + spinTo;
        //    if (isFirst)
        //    {
        //        spinNum -= 12 * 3;
        //    }

        //    for (int i = 0; i < spinNum; i++)
        //    {

        //        count++;
        //        if (count > 11)
        //        {
        //            count = 0;
        //        }
        //        iQuayIcoList[count].color = iQuayColorList[1];

        //        yield return new WaitForSeconds(.1f + .002f * acceleration * i * i * i);
        //        iQuayIcoList[count].color = iQuayColorList[0];
        //        iQuayIcoList[12].sprite = iQuaySprites[i % 2 == 0 ? 1 : 0];
        //        iQuayIcoList[13].sprite = iQuaySprites[i % 2 == 0 ? 3 : 2];
        //    }
        //    for (int i = 0; i < 5; i++)
        //    {
        //        iQuayIcoList[count].color = iQuayColorList[i % 2 == 0 ? 0 : 1];
        //        yield return new WaitForSeconds(.3f);

        //    }
        //    iQuayLastPrizeId = count;
        //    iQuayIcoList[count].color = iQuayColorList[1];
        //    isSpining = false;
        //    showIQuayRsBox();
        //    updateIQuayBtnState();
        //    showIQuayNotiAfterSpin();

        //    iQuaytopTogList[preIQuayTopTogId].isOn = false;
        //    iQuaytopTogList[1].isOn = true;
        //    changeTopTogIQuay(1);
        //    yield break;
        //}

        //private void showIQuayRsBox()
        //{
        //    //App.trace(items);
        //    if (items != "none" && prize != "0.0")
        //    {
        //        Func<string, Sprite> getSprite = new Func<string, Sprite>((string s) =>
        //        {
        //            for (int i = 0; i < 8; i++)
        //            {
        //                if ("icon_" + s == iQuaySprites[i + 4].name)
        //                {
        //                    return iQuaySprites[i + 4];
        //                }
        //            }
        //            return null;
        //        });

        //        string[] itemsList = items.Split(';');
        //        for (int i = 0; i < itemsList.Length; i++)
        //        {
        //            iQuayIcoList[14 + i].sprite = getSprite(itemsList[i]);
        //        }
        //        iQuayTextList[0].text = "x" + prize;
        //        return;
        //    }
        //    //App.trace("CHƯA QUAY!");
        //    iQuayTextList[0].text = "";
        //    for (int i = 0; i < 3; i++)
        //    {
        //        iQuayIcoList[14 + i].sprite = iQuaySprites[12];
        //    }
        //}

        //public void getIQuayPrize()
        //{
        //    if (!this.allowGet || isSpining)
        //    {
        //        return;
        //    }
        //    var req = new OutBounMessage("XENG.END");
        //    req.addHead();
        //    App.ws.send(req.getReq(), delegate (InBoundMessage res)
        //    {
        //        iQuayIsPlaying = false;
        //        this.allowGet = false;
        //        this.allowSpin = true;
        //        this.items = "none";
        //        prize = "0.0";
        //        showIQuayRsBox();
        //        updateIQuayBtnState();
        //        StartCoroutine(_showIQuayNoti(res.readStrings()));
        //    });
        //}
        //private void showIQuayNotiAfterSpin()
        //{
        //    StartCoroutine(_showIQuayNoti(iQuayNotiText));

        //}
        //private string iQuayNotiText = "";
        //private IEnumerator _showIQuayNoti(string noti)
        //{
        //    iQuayMainPanels[3].GetComponentInChildren<Text>().text = noti;
        //    iQuayMainPanels[3].SetActive(true);
        //    yield return new WaitForSeconds(3f);
        //    iQuayMainPanels[3].SetActive(false);
        //    yield break;
        //}

        //public void startIQuaySpin()
        //{
        //    if (isSpining || !allowSpin)
        //        return;



        //    var req = new OutBounMessage("XENG.START");
        //    req.addHead();
        //    req.writeInt(this.activeBet);//mức cược
        //    req.writeAcii(this.feeType);//kiểu man chip
        //    App.ws.send(req.getReq(), delegate (InBoundMessage res)
        //    {
        //        iQuayIsPlaying = true;
        //        iQuaytopTogList[preIQuayTopTogId].isOn = false;
        //        iQuaytopTogList[0].isOn = true;
        //        changeTopTogIQuay(0);
        //        preiQuayManChipId = -2;
        //        this.items = res.readAscii();
        //        this.allowSpin = res.readByte() == 1 ? true : false;
        //        this.allowGet = res.readByte() == 1 ? true : false;
        //        this.prize = res.readAscii();
        //        //isSpining = allowSpin;
        //        //App.trace("prize = " + prize + "|items = " + items + "|allowSpin" + allowSpin + "|allowGet = " + allowGet);
        //        string s = "";
        //        if (prize == "0.0")
        //        {
        //            s = "Thật đáng tiếc, hãy chọn BẮT ĐẦU quay để làm lại nào";
        //            iQuayNotiText = s;
        //            //items = "none";
        //            iQuayIsPlaying = false;
        //            //preiQuayManChipId = -1;
        //        }
        //        else
        //        {
        //            float val = float.Parse(prize) * activeBet;
        //            s = allowSpin ? "Bạn nhận được " + val + " " + feeType + ", vui lòng NHẬN THƯỞNG hoặc QUAY TIẾP" :
        //                "Bạn nhận được " + val + " " + feeType + ", vui lòng NHẬN THƯỞNG ";
        //            //StartCoroutine(_showIQuayNoti(s));
        //            iQuayNotiText = s;
        //        }

        //        spinIQuay();
        //    });
        //}
        //#endregion

        //#region //TAI XIU
        //[Header("====TAI XIU=====")]
        //public GameObject taiXiuPanel;
        //public GameObject taiXiuHelpPanel, taiXiuHisPanel, taiXiuHisElement;
        ///// <summary>
        ///// 0: Thông báo chính|1: Count down|2: Phiên|3-4: Số ng đặt tài - xỉu|5-6: Số tiền đặt tài-xỉu|7-8: Mình đặt tài-xỉu|9: Tiền của mình|10: Tiền đặt của mình|11: Thông báo phụ
        ///// </summary>
        //public Text[] taiXiuTextList;
        //public Image taiXiuHis, taiXiuRsImage, taiImg, xiuImg, taiXiuAvarImg;
        ///// <summary>
        ///// 0-1: His chan-le|2-7: cac mat xuc xacs|8-9: img nhap nhay tai-xiu
        ///// </summary>
        //public Sprite[] taiXiuSpritesList;
        //public Toggle[] taiXiuTogList;
        //public InputField taiXiuIpf;
        ///// <summary>
        ///// 0-1: Btn tài-xỉu
        ///// </summary>
        //public Button[] taiXiuBtns;

        //private string taiXiuCurrentState = "";

        //private void pingToSever()
        //{
        //    App.trace("SEND PING");
        //    var reqPing = new OutBounMessage("PING");
        //    reqPing.addHead();
        //    App.ws.send(reqPing.getReq(), delegate (InBoundMessage res)
        //    {
        //        App.trace("RECV [PING]");
        //    }, true, 0);
        //}

        //public void showTaiXiu(bool toOpen)
        //{
        //    RectTransform rtf = taiXiuPanel.GetComponent<RectTransform>();

        //    if (!toOpen)
        //    {

        //        Transform tfmHis = taiXiuRsImage.transform.parent;
        //        int countHis = tfmHis.childCount;
        //        for (int i = countHis - 1; i > 0; i--)
        //        {
        //            Destroy(tfmHis.GetChild(i).gameObject);
        //        }
        //        DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, new Vector2(0, 960), .35f).OnComplete(() =>
        //        {
        //            taiXiuPanel.SetActive(false);
        //            MiniGameController.instance.close();
        //        });
        //        var req_EXT_TAIXIU = new OutBounMessage("TAIXIU.EXIT");
        //        req_EXT_TAIXIU.addHead();
        //        App.trace("SEND [TAIXIU.EXIT]");
        //        App.ws.send(req_EXT_TAIXIU.getReq(), delegate (InBoundMessage res)
        //        {
        //            taixiuDelHanler();
        //            App.trace("RECV [TAIXIU.EXIT]");
        //            var req_GET_CURRENT_PATH = new OutBounMessage("GET_CURRENT_PATH");
        //            req_GET_CURRENT_PATH.addHead();
        //            App.ws.send(req_GET_CURRENT_PATH.getReq(), delegate (InBoundMessage res_GET_CURRENT_PATH)
        //            {
        //                string path = "Lobby";
        //                if (CPlayer.gameName != "") path += "." + CPlayer.gameName;
        //                var count = res_GET_CURRENT_PATH.readByte();
        //                for (var i = 0; i < count; i++)
        //                {
        //                    var type = res_GET_CURRENT_PATH.readAscii();
        //                    var id = res_GET_CURRENT_PATH.readAscii();
        //                    var name = res_GET_CURRENT_PATH.readString();
        //                    App.trace(i + "========" + type + "|" + id + "|" + name);
        //                    if (type == "room")
        //                    {
        //                        path += "." + id;

        //                    }
        //                    else if (type == "table")
        //                    {
        //                        path += "." + id;
        //                    }
        //                }
        //                App.trace("PATH = " + path);
        //                var avatar = res_GET_CURRENT_PATH.readAscii();
        //                var avatarId = res_GET_CURRENT_PATH.readInt();
        //                var chipBalance = res_GET_CURRENT_PATH.readLong();
        //                var starBalance = res_GET_CURRENT_PATH.readLong();
        //                var score = res_GET_CURRENT_PATH.readLong();
        //                var level = res_GET_CURRENT_PATH.readByte();

        //                CancelInvoke("pingToServer");
        //            });
        //        }, true, 1);
        //        return;
        //    }
        //    CPlayer.currMiniGame = "taixiu";
        //    taixiuRegHanlder();

        //    var req = new OutBounMessage("TAIXIU.ENTER");
        //    req.addHead();
        //    App.ws.send(req.getReq(), delegate (InBoundMessage res)
        //    {
        //        taiXiuCurrentState = res.readString();
        //        int seconds = res.readByte();
        //        taiXiuTextList[3].text = App.formatMoney(res.readInt().ToString()); //Số ng đặt tài
        //        taiXiuTextList[4].text = App.formatMoney(res.readInt().ToString()); //Số ng đặt xỉu
        //        taiXiuTextList[5].text = App.formatMoney(res.readLong().ToString());    //Tiền cửa tài
        //        taiXiuTextList[6].text = App.formatMoney(res.readLong().ToString());    //Tiền cửa xỉu
        //        taiXiuTextList[7].text = App.formatMoney(res.readLong().ToString());  //Tiền mình đặt tài
        //        taiXiuTextList[8].text = App.formatMoney(res.readLong().ToString());  //Tiền mình đặt xỉu

        //        taiXiuTextList[9].text = App.formatMoney(CPlayer.chipBalance.ToString());

        //        int countHs = res.readByte();
        //        Transform tfmHis = taiXiuHis.transform.parent;
        //        int countHis = tfmHis.childCount;
        //        for (int i = countHis - 1; i > 0; i--)
        //        {
        //            DestroyImmediate(tfmHis.GetChild(i).gameObject);
        //        }

        //        for (int i = 0; i < countHs; i++)
        //        {
        //            int gateId = res.readByte();
        //            Image img = Instantiate(taiXiuHis, taiXiuHis.transform.parent, false);
        //            img.sprite = gateId == 0 ? taiXiuSpritesList[0] : taiXiuSpritesList[1];
        //            img.gameObject.SetActive(true);
        //        }

        //        taiXiuTextList[2].text = "#" + App.formatMoney(res.readInt().ToString());



        //        taiXiuEnterState(taiXiuCurrentState);
        //        taiXiuShowCountDown(seconds);
        //        rtf.anchoredPosition = new Vector2(0, 960);
        //        taiXiuIpf.text = "50";
        //        if (CPlayer.fakeAva)
        //            taiXiuAvarImg.material = null;
        //        else
        //            taiXiuAvarImg.material = LoadingControl.instance.circleMaterial;
        //        taiXiuAvarImg.sprite = CPlayer.avatarSprite;
        //        taiXiuPanel.SetActive(true);
        //        DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, Vector2.zero, .35f).OnComplete(() =>
        //        {
        //            InvokeRepeating("pingToServer", 0f, 30f);
        //        });
        //    });
        //}

        //private void taiXiuEnterState(string state, string content = "", bool delay = false)
        //{
        //    taiXiuBtns[0].interactable = false;
        //    taiXiuBtns[1].interactable = false;
        //    switch (state)
        //    {
        //        case "bet":
        //            state = "Hãy chọn cửa để đặt";
        //            taiXiuBtns[0].interactable = true;
        //            taiXiuBtns[1].interactable = true;
        //            break;
        //        case "prepare":
        //            state = "Chuẩn bị";
        //            break;
        //        case "sellGate":
        //            state = "Hệ thống đang cân cửa";
        //            break;
        //        case "showResult":
        //            state = content;
        //            break;
        //        default:
        //            state = "Vui lòng chờ...";
        //            break;
        //    }
        //    if (!delay)
        //    {
        //        taiXiuTextList[0].text = state;
        //    }
        //    else
        //    {
        //        StartCoroutine(_taiXiuEnterState(state));
        //    }
        //}
        //private IEnumerator _taiXiuEnterState(string t)
        //{
        //    yield return new WaitForSeconds(1);
        //    taiXiuTextList[0].text = t;
        //    //yield break;
        //}
        //private void taiXiuShowCountDown(int time, bool toShow = true)
        //{
        //    StopCoroutine("_showCountDown");
        //    if (toShow == false)
        //    {
        //        taiXiuTextList[1].gameObject.SetActive(false);
        //        return;
        //    }
        //    StartCoroutine(_taiXiuShowCountDown(time));
        //}
        //private IEnumerator _taiXiuShowCountDown(int time)
        //{
        //    //Text txt = xocDiaTextList[2];
        //    int count = time;
            //xocDiaTextList[2].text = "";
        //    //txt.text = count.ToString();
        //    if (!taiXiuTextList[1].gameObject.activeSelf)
        //    {
        //        taiXiuTextList[1].gameObject.SetActive(true);
        //    }
        //    while (count > 0)
        //    {
        //        taiXiuTextList[1].text = count.ToString();

        //        yield return new WaitForSeconds(1f);
        //        count--;
        //    }
        //    //taiXiuTextList[1].gameObject.SetActive(false);
        //    yield break;
        //}

        //private string taiXiuRs = "";
        //private void taixiuRegHanlder()
        //{
        //    var req_UPDATE_POT = new OutBounMessage("TAIXIU.UPDATE_POT");
        //    req_UPDATE_POT.addHead();
        //    taixiuHanlerCmd.Add("TAIXIU.UPDATE_POT");
        //    App.ws.sendHandler(req_UPDATE_POT.getReq(), delegate (InBoundMessage res_UPDATE_POT)
        //    {



        //        App.trace("RECV [TAIXIU.UPDATE_POT]");

        //        res_UPDATE_POT.readByte();

        //        taiXiuTextList[3].text = App.formatMoney(res_UPDATE_POT.readInt().ToString());   //Số ng đặt tài
        //        taiXiuTextList[4].text = App.formatMoney(res_UPDATE_POT.readInt().ToString());  //Số ng đặt xỉu
        //        taiXiuTextList[5].text = App.formatMoney(res_UPDATE_POT.readLong().ToString());    //Tiền cửa tài
        //        taiXiuTextList[6].text = App.formatMoney(res_UPDATE_POT.readLong().ToString());    //Tiền cửa xỉu
        //    });

        //    var req_PREPARE = new OutBounMessage("TAIXIU.PREPARE");
        //    req_PREPARE.addHead();
        //    taixiuHanlerCmd.Add("TAIXIU.PREPARE");
        //    App.ws.sendHandler(req_PREPARE.getReq(), delegate (InBoundMessage res_PREPARE)
        //    {

        //        res_PREPARE.readByte();


        //        taiXiuEnterState("prepare");
        //        taiXiuShowCountDown(res_PREPARE.readByte());
        //    });

        //    var req_START = new OutBounMessage("TAIXIU.START");
        //    req_START.addHead();
        //    taixiuHanlerCmd.Add("TAIXIU.START");
        //    App.ws.sendHandler(req_START.getReq(), delegate (InBoundMessage res_START)
        //    {
        //        res_START.readByte();
        //        App.trace("RECV [TAIXIU.START]");
        //        taiXiuShowCountDown(res_START.readByte());
        //        taiXiuTextList[2].text = "#" + App.formatMoney(res_START.readInt().ToString()); //Update số phiên\
        //        taiXiuEnterState("bet");

        //    });

        //    var req_SELL_GATE = new OutBounMessage("TAIXIU.SELL_GATE");
        //    req_SELL_GATE.addHead();
        //    taixiuHanlerCmd.Add("TAIXIU.SELL_GATE");
        //    App.ws.sendHandler(req_SELL_GATE.getReq(), delegate (InBoundMessage res_SELL_GATE)
        //    {
        //        App.trace("RECV [TAIXIU.SELL_GATE]");
        //        res_SELL_GATE.readByte();
        //        taiXiuShowCountDown(res_SELL_GATE.readByte());
        //        taiXiuEnterState("sellGate");
        //    });

        //    var req_REFUND = new OutBounMessage("TAIXIU.REFUND");
        //    req_REFUND.addHead();
        //    taixiuHanlerCmd.Add("TAIXIU.REFUND");
        //    App.ws.sendHandler(req_REFUND.getReq(), delegate (InBoundMessage res_REFUND)
        //    {
        //        App.trace("RECV [TAIXIU.REFUND]");
        //        res_REFUND.readByte();
        //        var amount = res_REFUND.readLong();
        //        var gateId = res_REFUND.readByte();

        //        if (gateId == 0)
        //        {

        //            //taiXiuTextList[11].text = "Trả lại " + App.formatMoney((App.formatMoneyBack(taiXiuTextList[7].text) - amount).ToString()) + " cửa Tài";
        //            long tmp = App.formatMoneyBack(taiXiuTextList[7].text) - amount;
        //            StartCoroutine(taiXiuShowNote("Trả lại " + App.formatMoney((tmp).ToString()) + " Tài"));
        //            taiXiuTextList[7].text = App.formatMoney(amount.ToString());
        //            taiXiuTextList[9].text = App.formatMoney((App.formatMoneyBack(taiXiuTextList[9].text) + tmp).ToString());
        //        }
        //        else
        //        {
        //            //taiXiuTextList[11].text = "Trả lại " + App.formatMoney((App.formatMoneyBack(taiXiuTextList[8].text) - amount).ToString()) + " cửa Xỉu";
        //            //taiXiuTextList[11].transform.parent.gameObject.SetActive(true);
        //            long tmp = App.formatMoneyBack(taiXiuTextList[8].text) - amount;
        //            StartCoroutine(taiXiuShowNote("Trả lại " + App.formatMoney((tmp).ToString()) + " Xỉu"));
        //            taiXiuTextList[8].text = App.formatMoney(amount.ToString());
        //            taiXiuTextList[9].text = App.formatMoney((App.formatMoneyBack(taiXiuTextList[9].text) + tmp).ToString());
        //        }
        //    });

        //    var req_DIVIDE_CHIP = new OutBounMessage("TAIXIU.DIVIDE_CHIP");
        //    req_DIVIDE_CHIP.addHead();
        //    taixiuHanlerCmd.Add("TAIXIU.DIVIDE_CHIP");
        //    App.ws.sendHandler(req_DIVIDE_CHIP.getReq(), delegate (InBoundMessage res_DIVIDE_CHIP)
        //    {
        //        App.trace("TAIXIU.DIVIDE_CHIP");
        //        res_DIVIDE_CHIP.readByte();
        //        int amount = res_DIVIDE_CHIP.readInt();
        //        StartCoroutine(taiXiuShowNote(amount > 0 ? ("+ " + App.formatMoney(amount.ToString())) : ("- " + App.formatMoney(Math.Abs(amount).ToString()))));
        //    });

        //    var req_SHOW_RS = new OutBounMessage("TAIXIU.SHOW_RESULT");
        //    req_SHOW_RS.addHead();
        //    taixiuHanlerCmd.Add("TAIXIU.SHOW_RESULT");
        //    App.ws.sendHandler(req_SHOW_RS.getReq(), delegate (InBoundMessage res_SHOW_REULT)
        //    {
        //        taiXiuShowCountDown(0, false);
        //        App.trace("RECV [TAIXIU.SHOW_RESULT]");
        //        res_SHOW_REULT.readByte();

        //        int[] rs = new int[3];
        //        rs[0] = res_SHOW_REULT.readByte();
        //        rs[1] = res_SHOW_REULT.readByte();
        //        rs[2] = res_SHOW_REULT.readByte();
        //        int gateId = res_SHOW_REULT.readByte();

        //        string content = gateId == 0 ? "Tài: " : "Xỉu: ";
        //        taiXiuRs = gateId == 0 ? "Tài: " : "Xỉu: ";
        //        content = content + App.formatMoney((rs[0] + rs[1] + rs[2]).ToString()) + " điểm";
        //        StartCoroutine(_taiXiuShowRs(gateId));
        //        //App.trace(content);
        //        taiXiuEnterState("showResult", content, true);



        //        for (int i = 0; i < 3; i++)
        //        {
        //            Image rsImg = Instantiate(taiXiuRsImage, taiXiuRsImage.transform.parent, false);
        //            rsImg.sprite = taiXiuSpritesList[rs[i] + 2 - 1];

        //            RectTransform rtf = rsImg.GetComponent<RectTransform>();
        //            rtf.anchoredPosition = new Vector2(UnityEngine.Random.RandomRange(-250, -150), UnityEngine.Random.RandomRange(-350, -250));
        //            rsImg.gameObject.SetActive(true);
        //            Vector2 mVec = Vector2.zero;
        //            switch (i)
        //            {
        //                case 0:
        //                    mVec = new Vector2(UnityEngine.Random.RandomRange(50, 100), UnityEngine.Random.RandomRange(-100, -50));
        //                    break;
        //                case 1:
        //                    mVec = new Vector2(UnityEngine.Random.RandomRange(75, 100), UnityEngine.Random.RandomRange(-150, -125));
        //                    break;
        //                case 2:
        //                    mVec = new Vector2(UnityEngine.Random.RandomRange(100, 150), UnityEngine.Random.RandomRange(-125, -50));
        //                    break;
        //            }
        //            DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, mVec, .5f).SetEase(Ease.OutBack).OnComplete(() =>
        //            {

        //            });

        //        }



        //    });

        //    var req_GAME_OVER = new OutBounMessage("TAIXIU.GAMEOVER");
        //    req_GAME_OVER.addHead();
        //    taixiuHanlerCmd.Add("TAIXIU.GAMEOVER");
        //    App.ws.sendHandler(req_GAME_OVER.getReq(), delegate (InBoundMessage res_GAME_OVER)
        //    {
        //        res_GAME_OVER.readByte();
        //        App.trace("RECV [TAIXIU.GAMEOVER]");


        //        float a = float.Parse(taiXiuTextList[9].text);

        //        if (a < CPlayer.chipBalance)    //Thắng
        //        {


        //            Vector2 start = taiXiuRs == "Tài" ? taiXiuBtns[0].GetComponent<RectTransform>().position : taiXiuBtns[1].GetComponent<RectTransform>().position;
        //            start.y = start.y * 960 / Screen.height - 100;
        //            start.x = start.x * 1706 / Screen.width + 150;
        //            Vector2 end = taiXiuTextList[9].GetComponent<RectTransform>().position;
        //            end.x = end.x * 1706 / Screen.width - 100;
        //            end.y = end.y * 960 / Screen.height - 50;
        //            flyCoins("line", 10, start, end);
        //        }

        //        taiXiuTextList[9].text = App.formatMoney(CPlayer.chipBalance.ToString());

        //        taiXiuRestart();
        //        Transform tfmHis = taiXiuHis.transform.parent;
        //        int countHis = tfmHis.childCount;
        //        for (int i = countHis - 1; i > 0; i--)
        //        {
        //            DestroyImmediate(tfmHis.GetChild(i).gameObject);
        //        }
        //        int count = res_GAME_OVER.readByte();
        //        for (int i = 0; i < count; i++)
        //        {
        //            int gateId = res_GAME_OVER.readByte();
        //            Image img = Instantiate(taiXiuHis, taiXiuHis.transform.parent, false);
        //            img.sprite = gateId == 0 ? taiXiuSpritesList[0] : taiXiuSpritesList[1];
        //            img.gameObject.SetActive(true);
        //        }


        //    });

        //    var req_PONG = new OutBounMessage("PONG");
        //    req_PONG.addHead();
        //    taixiuHanlerCmd.Add("PONG");
        //    App.ws.sendHandler(req_PONG.getReq(), delegate (InBoundMessage res_PONG)
        //    {
        //        App.trace("RECV [PONG]");
        //    });
        //}

        //private List<string> taixiuHanlerCmd = new List<string>();
        //private void taixiuDelHanler()
        //{
        //    foreach (string t in taixiuHanlerCmd)
        //    {
        //        //App.trace(t);
        //        var req = new OutBounMessage(t);
        //        req.addHead();
        //        App.ws.delHandler(req.getReq());
        //    }
        //}

        //private IEnumerator taiXiuShowNote(string t)
        //{
        //    taiXiuTextList[11].text = t;
        //    taiXiuTextList[11].transform.parent.gameObject.SetActive(true);
        //    yield return new WaitForSeconds(4);
        //    taiXiuTextList[11].transform.parent.gameObject.SetActive(false);
        //    yield break;
        //}


        //private void taiXiuRestart()
        //{
        //    Transform tfmHis = taiXiuRsImage.transform.parent;
        //    int countHis = tfmHis.childCount;
        //    for (int i = countHis - 1; i > 0; i--)
        //    {
        //        Destroy(tfmHis.GetChild(i).gameObject);
        //    }
        //    for (int i = 0; i < 6; i++)
        //    {
        //        taiXiuTextList[i + 3].text = "0";
        //    }
        //}

        //private IEnumerator _taiXiuShowRs(int gateId)
        //{

        //    int showCount = 20;
        //    Image img = gateId == 0 ? taiImg : xiuImg;
        //    Sprite pre = img.overrideSprite;
        //    int imgId = gateId == 0 ? 8 : 9;
        //    Transform tmf = img.transform;
        //    for (int i = 0; i < 5; i++)
        //    {

        //        img.overrideSprite = taiXiuSpritesList[imgId];
        //        tmf.DOScale(1.1f, .25f);
        //        yield return new WaitForSeconds(.25f);
        //        tmf.DOScale(1f, .25f);
        //        yield return new WaitForSeconds(.25f);
        //        img.overrideSprite = pre;
        //    }
        //}

        //private int taiXiuPreTogId = -1;
        //public void taiXiuChangeTog(int id)
        //{
        //    if (id != taiXiuPreTogId && taiXiuTogList[id].isOn == true)
        //    {
        //        for (int i = 0; i < 4; i++)
        //        {
        //            if (i != id)
        //            {
        //                taiXiuTogList[i].isOn = false;
        //                taiXiuTogList[i].interactable = true;
        //            }
        //            else
        //            {
        //                taiXiuTogList[i].interactable = false;
        //            }
        //        }
        //        switch (id)
        //        {
        //            case 0:
        //                taiXiuIpf.text = "1000";
        //                break;
        //            case 1:
        //                taiXiuIpf.text = "10000";
        //                break;
        //            case 2:
        //                taiXiuIpf.text = "500000";
        //                break;
        //            case 3:
        //                taiXiuIpf.text = "1000000";
        //                break;
        //        }
        //    }
        //}
        //public void taiXiuChangeBet(int id)
        //{
        //    int mBet = int.Parse(taiXiuIpf.text);
        //    switch (id)
        //    {
        //        case 0:
        //            taiXiuIpf.text = (mBet + 1000).ToString();
        //            break;
        //        case 1:
        //            taiXiuIpf.text = (mBet + 10000).ToString();
        //            break;
        //        case 2:
        //            taiXiuIpf.text = (mBet + 100000).ToString();
        //            break;
        //        case 3:
        //            taiXiuIpf.text = (mBet + 500000).ToString();
        //            break;
        //        case 4:
        //            taiXiuIpf.text = (mBet + 1000000).ToString();
        //            break;

        //    }
        //}
        //public void taiXiuX2(bool isDel)
        //{
        //    if (isDel)
        //    {
        //        taiXiuIpf.text = "0";
        //        return;
        //    }
        //    taiXiuIpf.text = (int.Parse(taiXiuIpf.text) * 2).ToString();
        //}

        //public void taiXiuDoBet(int gateId)
        //{
        //    var req = new OutBounMessage("TAIXIU.BET");
        //    req.addHead();
        //    long amount = long.Parse(taiXiuIpf.text);
        //    req.writeLong(amount);
        //    req.writeByte(gateId);
        //    App.ws.send(req.getReq(), delegate (InBoundMessage res)
        //    {
        //        taiXiuTextList[9].text = App.formatMoney((App.formatMoneyBack(taiXiuTextList[9].text) - amount).ToString());
        //        if (gateId == 0)
        //        {
        //            taiXiuTextList[7].text = (App.formatMoneyBack(taiXiuTextList[7].text) + amount).ToString();
        //            return;
        //        }
        //        taiXiuTextList[8].text = (App.formatMoneyBack(taiXiuTextList[8].text) + amount).ToString();

        //    });
        //}

        //public void taiXiuOpenMore(string type)
        //{
        //    taiXiuHelpPanel.transform.parent.gameObject.SetActive(true);
        //    switch (type)
        //    {
        //        case "help":
        //            taiXiuHelpPanel.SetActive(true);
        //            break;
        //        case "his":
        //            Transform tfmHis = taiXiuHisElement.transform.parent;
        //            int countHis = tfmHis.childCount;
        //            for (int i = countHis - 1; i > 0; i--)
        //            {
        //                DestroyImmediate(tfmHis.GetChild(i).gameObject);
        //            }

        //            var req = new OutBounMessage("TAIXIU.GET_INFO");
        //            req.addHead();
        //            App.ws.send(req.getReq(), delegate (InBoundMessage res)
        //            {
        //                int count = res.readByte();
        //                for (int i = 0; i < count; i++)
        //                {

        //                    GameObject goj = Instantiate(taiXiuHisElement, taiXiuHisElement.transform.parent, false);
        //                    Text[] txtArr = goj.GetComponentsInChildren<Text>();
        //                    for (int j = 0; j < txtArr.Length; j++)
        //                    {
        //                        if (j == 2)
        //                        {
        //                            txtArr[2].text = "Tài: " + res.readStrings() + "\nXỉu: " + res.readStrings();
        //                        }
        //                        else
        //                        {
        //                            txtArr[j].text = res.readStrings();
        //                        }
        //                    }
        //                    goj.SetActive(true);
        //                }
        //            });

        //            taiXiuHisPanel.SetActive(true);
        //            break;
        //    }
        //}

        //public void taiXiuCloseMore()
        //{
        //    taiXiuHelpPanel.transform.parent.gameObject.SetActive(false);

        //    taiXiuHelpPanel.SetActive(false);

        //    taiXiuHisPanel.SetActive(false);

        //}
        //#endregion

        #region //FLY COINS
        public GameObject coin;

        private Dictionary<string, GameObject> coinsList;

        /// <summary>
        /// Hiệu ứng bay chip
        /// </summary>
        /// <param name="type">[Truyền 1 trong 3]volcano: Núi lửa| line: Bay thẳng| fireWorks: Pháo hoa</param>
        /// <param name="coinNum">[Bắt buộc]Số đồng xu bay</param>
        /// <param name="startPos">[Bắt buộc]Vị trí khởi đầu</param>
        /// <param name="endPos">[Chỉ truyền khi type = "line"| Còn lại Vector2.zero]Vị trí cuối</param>
        /// <param name="flyDistanceTime">[Không cần truyền]Thời gian giữa bay 2 đồng xu</param>
        public void flyCoins(string type, int coinNum, Vector2 startPos, Vector2 endPos, float flyDistanceTime = .5f, float delayTime = 0, Transform rtfff = null)
        {
            if (coinsList == null)
            {
                coinsList = new Dictionary<string, GameObject>();
            }

            //coin.transform.parent.gameObject.SetActive(true);
            switch (type)
            {
                case "volcano":
                    StartCoroutine(_flyCoinsByType("volcano", coinNum, startPos, endPos, delayTime));
                    break;
                case "fireWorks":
                    StartCoroutine(_flyCoinsByType("fireWorks", coinNum, startPos, endPos, delayTime));
                    break;
                case "line":
                    StartCoroutine(_flyCoinsByType("line", coinNum, startPos, endPos, delayTime, rtfff));
                    break;
            }
        }
        private IEnumerator _flyCoinsByType(string type, int coinNum, Vector2 startPos, Vector2 endPos, float delayTime = 0, Transform rtfff = null)
        {
            float scY = Screen.height / 960;
            switch (type)
            {
                case "volcano":
                    yield return new WaitForSeconds(.5f);
                    for (int i = 0; i < coinNum; i++)
                    {
                        GameObject mCoind = Instantiate(coin, coin.transform.parent, false);
                        mCoind.SetActive(true);
                        RectTransform rtf = mCoind.GetComponent<RectTransform>();
                        Vector2 pos = new Vector2(UnityEngine.Random.RandomRange(-50, 50), UnityEngine.Random.RandomRange(-50, 50));
                        //rtf.anchoredPosition = new Vector2(500, 500);
                        rtf.anchoredPosition = startPos;
                        Vector2 endPoss = new Vector2(UnityEngine.Random.RandomRange(startPos.x - 400, startPos.x + 400), UnityEngine.Random.RandomRange(startPos.y - 400, startPos.y - 100));
                        //coinsList.Add("vol" + i, mCoind);
                        string coinId = "vol" + i;
                        if (!coinsList.ContainsKey(coinId))
                        {
                            coinsList.Add(coinId, mCoind);
                        }
                        else
                        {
                            while (coinsList.ContainsKey(coinId))
                            {
                                coinId = "vol" + (i + UnityEngine.Random.RandomRange(500, 1000));
                            }
                            coinsList.Add(coinId, mCoind);
                        }
                        rtf.DOJump(endPoss, 400, 1, 1f).SetEase(Ease.OutCubic).SetId("vol" + i).OnComplete(() =>
                        {
                            rtf.DOScale(.25f, .1f).OnComplete(() =>
                            {
                                Destroy(mCoind);
                            });
                        }); ;
                        yield return new WaitForSeconds(.05f);
                    }
                    break;
                case "fireWorks":
                    Vector2 posToFly = Vector2.zero;
                    int radius = 500;
                    for (int i = 0; i < coinNum * 2; i++)
                    {
                        GameObject mCoind = Instantiate(coin, coin.transform.parent, false);
                        mCoind.SetActive(true);
                        RectTransform rtf = mCoind.GetComponent<RectTransform>();
                        Vector2 pos = new Vector2(UnityEngine.Random.RandomRange(-50, 50), UnityEngine.Random.RandomRange(-50, 50));
                        rtf.anchoredPosition = startPos;
                        var angle = UnityEngine.Random.RandomRange(0, 360);
                        float xx = (float)System.Math.Cos(angle) * radius;
                        float yy = (float)System.Math.Sin(angle) * radius;
                        posToFly = new Vector2(xx, yy);
                        //coinsList.Add("fire" + i, mCoind);
                        string coinId = "fire" + i;
                        if (!coinsList.ContainsKey(coinId))
                        {
                            coinsList.Add(coinId, mCoind);
                        }
                        else
                        {
                            while (coinsList.ContainsKey(coinId))
                            {
                                coinId = "fire" + (i + UnityEngine.Random.RandomRange(500, 1000));
                            }
                            coinsList.Add(coinId, mCoind);
                        }
                        DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, startPos + posToFly, .5f + i * .1f).SetEase(Ease.OutCubic).SetId("fire" + i).OnComplete(() =>
                        {
                            rtf.DOScale(.25f, .25f).OnComplete(() =>
                            {
                                Destroy(mCoind);
                            });
                        });
                        if (i == coinNum)
                            yield return new WaitForSeconds(.5f);
                    }
                    break;
                case "line":
                    //App.trace("START FLY BY LINE");
                    yield return new WaitForSeconds(delayTime);
                    if (rtfff != null)
                        rtfff.gameObject.SetActive(true);
                    for (int i = 0; i < coinNum; i++)
                    {
                        GameObject mCoind = Instantiate(coin, coin.transform.parent, false);
                        mCoind.SetActive(true);
                        RectTransform rtf = mCoind.GetComponent<RectTransform>();
                        if (rtfff != null)
                            rtf.parent = rtfff;
                        Vector2 pos = new Vector2(UnityEngine.Random.RandomRange(-50, 50), UnityEngine.Random.RandomRange(-50, 50));
                        rtf.anchoredPosition = startPos;
                        string coinId = "line" + i;
                        if (!coinsList.ContainsKey(coinId))
                        {
                            coinsList.Add(coinId, mCoind);
                        }
                        else
                        {
                            while (coinsList.ContainsKey(coinId))
                            {
                                coinId = "line" + (i + UnityEngine.Random.RandomRange(500, 1000));
                            }
                            coinsList.Add(coinId, mCoind);
                        }
                        DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, pos + endPos, .5f).SetEase(Ease.OutCubic).SetId(coinId).OnComplete(() =>
                        {
                            rtf.DOScale(.25f, .1f).OnComplete(() =>
                            {
                                Destroy(mCoind, .1f);
                            });
                        });

                        yield return new WaitForSeconds(.05f);
                    }
                    if (rtfff != null)
                        rtfff.gameObject.SetActive(false);
                    break;
            }
            StartCoroutine(_delCoins());
        }

        IEnumerator _delCoins()
        {
            yield return new WaitForSeconds(5f);
            delCoins();
        }

        public void delCoins(Transform rtf = null)
        {
            if (coinsList == null || coinsList.Count == 0)
                return;
            foreach (string key in coinsList.Keys.ToList())
            {
                try
                {
                    DOTween.Kill(key);
                    if (rtf != null)
                        coinsList[key].transform.parent = rtf;
                    Destroy(coinsList[key], 0.01f);
                    coinsList.Clear();
                }
                catch
                {

                }
            }
            App.trace("LIST : " + coinsList.Count, "red");
            Transform tfm = coin.transform.parent;
            int count = tfm.childCount;
            for (int i = count - 1; i > -1; i--)
            {
                //tfm.GetChild(i).gameObject.SetActive(false);
                if (tfm.GetChild(i).gameObject.name.Contains("Clone"))
                {
                    try
                    {

                        DestroyImmediate(tfm.GetChild(i).gameObject);
                    }
                    catch
                    {

                    }
                }


            }
        }
        #endregion
    }
}
