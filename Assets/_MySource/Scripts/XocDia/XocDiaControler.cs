using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;
using System;
using System.Linq;
using UnityEngine.SceneManagement;
using Core.Server.Api;

public class XocDiaControler : MonoBehaviour {

    [Header("=====Player=====")]
    public Image[] avatarsList;
    public GameObject[] infoObjectList, ownerList,sellBuyList;
    public Text[] balanceTextList, nickNameTextList, moneyErnedTxtList;
    [Header("===ANIM===")]
    public Animator slideSceneAnim;
    [Header("====KHÁC=====")]
    ///<summary>
    ///0: tiền bot-right|1: center|2: count down|3: His chẵn|4: His lẻ|5-10:Tổng tiền|
    ///11:16: My tiền|17: cược biên|18-24: cược - nhận cược biên
    ///24-29: Text của slider
    /// </summary>
    public Text[] xocDiaTextList;
    /// <summary>
    /// 0: Vàng|1: Trắng|2: Mờ
    /// </summary>
    public Color[] colorList;
    /// <summary>
    /// 0:addPlayerIcon|1-6:Kquả|7: cửa pressed|8: vị pressed|9:cửa|10:vị|11-14: chip x1-x20
    /// </summary>
    public Sprite[] spritesList;
    /// <summary>
    /// 0: Cả bộ|1: bát
    /// </summary>
    public RectTransform[] diaBatRtf;
    public Toggle[] betTogList;
    /// <summary>
    /// 0: His|1: Kquả|2: chan|3:le|4-7:vị|8: CHIP TO FLY
    /// </summary>
    public Image[] imgToClone;
    /// <summary>
    /// 0-1:chan le|2:5:vị
    /// </summary>
    public Button[] xocDiaBtnList;
    /// <summary>
    /// 0: Hủy|1 : Đặt lại|2: X2|3: Mua cửa
    /// </summary>
    public Button[] stateBtns;
    /// <summary>
    /// 0: Vàng|1: Trắng
    /// </summary>
    public Font[] xocDiaFontLs;

    /// <summary>
    /// Các vị trí đã có ng ngồi
    /// </summary>
    private bool[] exitsSlotList = { false, false, false, false, false, false, false };
    public Slider[] slider;

    private int currOwner, mySlotId, currTimeOut, sellGateId = -1, buyGateSlotId;
    private Dictionary<int, Player> playerList;
    private long[] chipList = { 0, 0, 0, 0, 0, 0, 0 };
    private bool isBuyGate = false;
    private string currentState = "";
    private Dictionary<string, Button> stateButtonByCommandCode;
	private Dictionary<int, Button> stateButtonByPosition;
    private State state;
    private Vector2[] XuClonepos;
    private Dictionary<int, List<XuRTF>> xuRtfList = new Dictionary<int, List<XuRTF>>();

    public static XocDiaControler instance;
    //public GameObject xdTable;
    void Awake()
    {
        Application.runInBackground = true;
        getInstance();
        //xdTable.SetActive(true);

    }

    void getInstance()
    {
        if (instance != null)
            Destroy(gameObject);
        else
        {
            instance = this;
            //DontDestroyOnLoad(gameObject);
        }

    }

    void Start()
    {
        StartCoroutine(LoadingControl.instance._start());
        if (CPlayer.betAmtOfTableToGo.Contains('-'))
        {

            int amtId = (-1) * int.Parse(CPlayer.betAmtOfTableToGo);
            //App.trace("FACC " + amtId);
            var req_getBetAmtList = new OutBounMessage("LIST_BET_AMT");
            req_getBetAmtList.addHead();

            App.ws.send(req_getBetAmtList.getReq(), delegate (InBoundMessage res)
            {

                int count = res.readByte();
                for (int i = 0; i < count; i++)
                {
                    int a = res.readInt();
                    //App.trace("BET = " + a);
                    if (i == amtId)
                    {
                        CPlayer.betAmtOfTableToGo = App.formatMoney(a.ToString());
                        tableBet = float.Parse(CPlayer.betAmtOfTableToGo);

                        string tempString = App.listKeyText["XOCDIA_NAME"].ToUpper();
                        xocDiaTextList[30].text = tempString + " - " + CPlayer.betAmtOfTableToGo + " "  + App.listKeyText["CURRENCY"];//"XÓC ĐĨA - " + CPlayer.betAmtOfTableToGo + " Gold";
                        break;
                    }


                }



                //App.trace("GET BET AMT DONE! BETAMT COUNT = " + count);
            });
        }
        else
        {
            string tempString = App.listKeyText["XOCDIA_NAME"];
            xocDiaTextList[30].text = tempString + " - " + App.formatMoney(CPlayer.betAmtOfTableToGo) + " " + App.listKeyText["CURRENCY"];//"XÓC ĐĨA - " + App.formatMoney(CPlayer.betAmtOfTableToGo) + " Gold";
            tableBet = float.Parse(CPlayer.betAmtOfTableToGo);
        }

        XuClonepos = new Vector2[7];
        int scW = 1736;
        int scH = 960;

        //int m = 90 * (scW / 1706);

        XuClonepos[0] = new Vector2(-(scW/2-90-30), -73);
        XuClonepos[1] = new Vector2(-(scW / 2 - 90 - 30),+ 73);
        XuClonepos[2] = new Vector2(-415, scH/2 - 124);
        XuClonepos[3] = new Vector2(0, scH/2 - 124);
        XuClonepos[4] = new Vector2(415, scH/2 - 124);
        XuClonepos[5] = new Vector2(scW / 2 - 90 - 30, 75);
        XuClonepos[6] = new Vector2(scW / 2 - 90 - 30, -75);


        for (int i = 0; i < 4; i++)
        {
            betTogList[i].GetComponentInChildren<Text>().text = (lanMucCuoc[i] * tableBet).ToString();
        }

        balanceTextList[7].text = App.formatMoney(CPlayer.chipBalance.ToString());
        Close_panelConfirmDialog();
        confirmShowing = false;


        registerHandler();
        getTableDataEx();
    }

    public void balanceChanged(int slotId,long chipBalance,long starBalance)
    {
        try
        {
            if (slotId == mySlotId)
            {
                balanceTextList[7].text = App.formatMoney(chipBalance.ToString());
            }
            int mSl = -1;
            foreach (Player pl in playerList.Values.ToList())
            {
                if (pl.SvSlotId == slotId)
                    mSl = pl.SlotId;
            }
            if (mSl > -1)
            {
                StartCoroutine(_balanceChanged(chipList[mSl], chipBalance, balanceTextList[mSl], slotId == mySlotId));
                chipList[mSl] = chipBalance;
                playerList[mSl].ChipBalance = chipBalance;
                playerList[mSl].StarBalance = starBalance;
            }
        }
        catch
        {

        }

    }
    private IEnumerator _balanceChanged(long start, long end, Text txt,bool isMe =false)
    {
        if (start == end)
            yield break;

        if(start < end)
        {
            txt.transform.localScale = new Vector2(1.2f, 1.2f);
        }
        else
        {
            txt.transform.localScale = new Vector2(.8f, .8f);
        }

        if (isMe)
        {

            int distance = (int)(end - start);
            if (distance > 20)
            {
                distance /= 20;
            }

            float curr = start;
            if (distance > 1)
                for (int i = 0; i < 20; i++)
                {
                    curr += distance;
                    txt.text = App.formatMoney(curr.ToString());

                    yield return new WaitForSeconds(.1f);
                }
            else
            {
                txt.text = App.formatMoney(end.ToString());
            }
            txt.text = App.formatMoney(end.ToString());
        }
        else
        {
            txt.text = App.formatMoneyAuto(end);
            yield return new WaitForSeconds(2f);
        }

        txt.transform.localScale = new Vector2(1, 1);
        yield break;
    }
    private Vector2 preBatPos;
    private void registerHandler()
    {
        var req_ENTER_STATE = new OutBounMessage("ENTER_STATE");
        req_ENTER_STATE.addHead();
        App.ws.sendHandler(req_ENTER_STATE.getReq(), delegate (InBoundMessage res_ENTER_STATE)
        {
            App.trace("RECV [ENTER_STATE]");
            int stateId = res_ENTER_STATE.readByte();
            enterState(stateById[stateId]);
            enterState2(stateById[stateId]);
        });


        var req_SLOT_IN_TABLE_CHANGED = new OutBounMessage("SLOT_IN_TABLE_CHANGED");
        req_SLOT_IN_TABLE_CHANGED.addHead();
        App.ws.sendHandler(req_SLOT_IN_TABLE_CHANGED.getReq(), delegate (InBoundMessage res) {
            App.trace("RECV [SLOT_IN_TABLE_CHANGED]");
            var nickName = res.readAscii();
            var slotId = res.readByte();
            var chipBalance = res.readLong();
            var score = res.readLong();
            var level = res.readByte();
            var avatarId = res.readShort();
            var avatar = res.readAscii();
            var gender = res.readByte() == 1;   //True: Name|False: Nữ
            var owner = res.readByte() == 1;
            var playerId = res.readLong();
            var starBalance = res.readLong();

            App.trace("is Owner = " + owner + "|slot = " + slotId + "|nick = " + nickName + "|slotId" + detecSlotIdBySvrId(slotId));

            if (nickName.Length == 0)    //Thoát khỏi bàn chơi
            {
                SoundManager.instance.PlayUISound(SoundFX.CARD_EXIT_TABLE);
                int slotTmp = -1;
                foreach(Player pl in playerList.Values.ToList())
                {
                    if (pl.SvSlotId ==slotId)
                    {
                        slotTmp = pl.SlotId;
                        App.trace("Có thằng thoát " + slotTmp);
                        playerList.Remove(slotTmp);
                        infoObjectList[slotTmp].SetActive(false);
                        avatarsList[slotTmp].sprite = spritesList[0];
                        avatarsList[slotTmp].overrideSprite = spritesList[0];
                        //avatarsList[slotTmp].material = null;
                        exitsSlotList[slotTmp] = false;
                        ownerList[slotTmp].SetActive(false);
                        //updateTableSlot();
                        //sellBuyList[slotId + 7 - 1].SetActive(false);
                        if (playerList.Count < 2)
                        {
                            updateNotiText("Vui lòng chờ...");
                            showCountDown(0, false);
                        }
                        return;
                    }
                }

            }

            Player player = null;

            if (owner)  //Thay cái
            {
                currOwner = slotId;
                if (slotId == mySlotId)  //Mình là cái
                {
                    App.trace("Mình lên làm cái");
                    ownerList[3].SetActive(false);
                    ownerList[0].SetActive(true);
                    //realOwnerSlotIdIntalbe = 0;
                    playerList[0].IsOwner = true;
                    if (playerList.ContainsKey(3))
                        playerList[3].IsOwner = false;


                    return;
                }


                //Thằng khác thành cái
                int slotTmp = -1;
                foreach (Player pl in playerList.Values.ToList())
                {
                    if (pl.SvSlotId == slotId)
                    {
                        slotTmp = pl.SlotId;
                        break;
                    }
                }
                playerList[slotTmp].IsOwner = true;
                playerList[0].IsOwner = false;

                Player plTmp = playerList[slotTmp];
                plTmp.SlotId = 3;
                if (playerList.ContainsKey(3))
                {
                    playerList[3].IsOwner = false;
                    playerList[slotTmp] = playerList[3];
                    playerList[slotTmp].SlotId = slotTmp;
                    playerList[3] = plTmp;
                    //setInfo(playerList[slotTmp], avatarsList[slotTmp], infoObjectList[slotTmp], balanceTextList[slotTmp], nickNameTextList[slotTmp], ownerList[slotTmp]);
                    //setInfo(playerList[3], avatarsList[3], infoObjectList[3], balanceTextList[3], nickNameTextList[3], ownerList[3]);

                    chipList[3] = playerList[3].ChipBalance;
                    balanceTextList[3].text = App.formatMoneyAuto(playerList[3].ChipBalance);
                    nickNameTextList[3].text = App.formatNickName(playerList[3].NickName, 10);
                    Sprite sprTmp = avatarsList[3].overrideSprite;
                    avatarsList[3].overrideSprite = avatarsList[slotTmp].overrideSprite;

                    chipList[slotTmp] = playerList[slotTmp].ChipBalance;
                    balanceTextList[slotTmp].text = App.formatMoneyAuto(playerList[slotTmp].ChipBalance);
                    nickNameTextList[slotTmp].text = App.formatNickName(playerList[slotTmp].NickName, 10);
                    avatarsList[slotTmp].overrideSprite = sprTmp;

                    App.trace("Thằng " + slotTmp + " lên làm cái đổi với thằng cái cũ");
                }
                else
                {
                    playerList.Remove(slotTmp);
                    infoObjectList[slotTmp].SetActive(false);
                    avatarsList[slotTmp].overrideSprite = spritesList[0];
                    exitsSlotList[slotTmp] = false;
                    ownerList[slotTmp].SetActive(false);

                    playerList[3] = plTmp;
                    playerList[3].SlotId = 3;
                    setInfo(playerList[3], avatarsList[3], infoObjectList[3], balanceTextList[3], nickNameTextList[3], ownerList[3]);
                    exitsSlotList[slotTmp] = false;
                    exitsSlotList[3] = true;
                    App.trace("Thằng " + slotTmp + " lên làm cái");
                }
                return;
            }

            SoundManager.instance.PlayUISound(SoundFX.CARD_JOIN_TABLE);
            //Nếu đã có trước đó
            foreach (Player mPl in playerList.Values.ToList())
            {
                if(mPl.SvSlotId == slotId)
                {
                    int slot = detecSlotIdBySvrId(slotId);
                    player = new Player(slot, slotId, playerId, nickName, avatarId, avatar, gender, chipBalance, starBalance, score, level, owner);
                    playerList[mPl.SlotId] = player;
                    setInfo(player, avatarsList[slot], infoObjectList[slot], balanceTextList[slot], nickNameTextList[slot], ownerList[slot]);
                    return;
                }
            }

            //Thêm bình thường

            int slotTmp1 = detecSlotIdBySvrId(slotId);
            player = new Player(slotTmp1, slotId, playerId, nickName, avatarId, avatar, gender, chipBalance, starBalance, score, level, owner);
            App.trace("Thằng slotID " + player.SlotId + " ngồi vào bàn|svSlot = " + slotId);
            playerList.Add(slotTmp1, player);
            setInfo(player, avatarsList[slotTmp1], infoObjectList[slotTmp1], balanceTextList[slotTmp1], nickNameTextList[slotTmp1], ownerList[slotTmp1]);
            exitsSlotList[slotTmp1] = true;

        });

        var req_GAMEOVER = new OutBounMessage("GAMEOVER");
        req_GAMEOVER.addHead();
        App.ws.sendHandler(req_GAMEOVER.getReq(), delegate (InBoundMessage res_GAMEOVER)
        {
            App.trace("RECV [GAMEOVER]");
            //úp bát
            StartCoroutine(shakeSound(true));
            DOTween.To(() => diaBatRtf[1].anchoredPosition, x => diaBatRtf[1].anchoredPosition = x, preBatPos, 2f);
            diaBatRtf[1].SetAsLastSibling();
            if(buyGateSlotId > -1)
            {
                moneyErnedTxtList[buyGateSlotId + 7].gameObject.SetActive(true);
                sellBuyList[buyGateSlotId].SetActive(false);
            }
            imgToClone[2].sprite = spritesList[9];
            imgToClone[3].sprite = spritesList[9];

            for(int i = 0; i < 4; i++)
            {
                imgToClone[i + 4].sprite = spritesList[10];
            }

            for(int i = 0; i < 12; i++)
            {
                xocDiaTextList[i + 5].text = "";
            }
            for (int i = 0; i < 6; i++)
            {
                sellBuyList[i + 14].SetActive(false);
            }

            unbetCount = 0;
            rebetCount = 0;
            rebetx2Count = 0;
            isBeted = false;
            sellGateId = -1;
            buyGateSlotId = -1;

            myBetedListToSave.Clear();
            myBetedListToSave = new Dictionary<int, int>(myBetedList);
            App.trace(myBetedListToSave.Count + "is saved!");
            myBetedList.Clear();



            if (this.mySlotId > 0)
                enterState2(stateByCode["waitStart"]);
            else
                enterState2(stateByCode["viewTable"]);
            isPlaying = false;
        });



        OutBounMessage req_DIVIDE_CHIP = new OutBounMessage("DIVIDE_CHIP");
        req_DIVIDE_CHIP.addHead();
        App.ws.sendHandler(req_DIVIDE_CHIP.getReq(), delegate (InBoundMessage res_DIVIDE_CHIP)
        {
            App.trace("RECV [DEVICE_CHIP]");
            int result = res_DIVIDE_CHIP.readByte();    //Số quân trắng
            int gateId = res_DIVIDE_CHIP.readByte();    //0-1
            int viId = res_DIVIDE_CHIP.readByte();      //2-5
            //App.trace("Về cửa = " + gateId);
            Transform tfm = imgToClone[1].transform.parent;
            int count = tfm.childCount;
            //App.trace("==================" + count);
            for (int i = count - 2; i > 0; i--)
            {
                DestroyImmediate(tfm.GetChild(i).gameObject);
            }
            //Show kquả trong bát
            for (int i = 0; i < 4; i++)
            {
                Image img = Instantiate(imgToClone[1], imgToClone[1].transform.parent, false);
                if (i < result)
                {
                    img.sprite = spritesList[5];
                }
                else
                {
                    img.sprite = spritesList[1];
                }
                Vector2 vect = Vector2.zero;
                switch (i)
                {
                    case 0:
                        vect = new Vector2(UnityEngine.Random.RandomRange(-50, 0), UnityEngine.Random.RandomRange(0, 50));
                        break;
                    case 1:
                        vect = new Vector2(UnityEngine.Random.RandomRange(0, 50), UnityEngine.Random.RandomRange(0, 50));
                        break;
                    case 2:
                        vect = new Vector2(UnityEngine.Random.RandomRange(0, 50), UnityEngine.Random.RandomRange(-50, 0));
                        break;
                    case 3:
                        vect = new Vector2(UnityEngine.Random.RandomRange(-50, 0), UnityEngine.Random.RandomRange(-50, 0));
                        break;
                }
                img.rectTransform.anchoredPosition = vect;
                img.gameObject.SetActive(true);

            }
            updateNotiText("Mở bát");
            //Mở bát
            SoundManager.instance.PlayUISound(SoundFX.CARD_MO_BAT);
            preBatPos = diaBatRtf[1].anchoredPosition;
            DOTween.To(() => diaBatRtf[1].anchoredPosition, x => diaBatRtf[1].anchoredPosition = x, new Vector2(0, 1000), 1f).OnComplete(() => {
                if (result % 2 == 0)
                {
                    imgToClone[2].sprite = spritesList[7];
                }
                else
                {
                    imgToClone[3].sprite = spritesList[7];
                }
                if (viId > 1)
                {
                    //App.trace("Về vị = " + viId);
                    imgToClone[viId + 2].sprite = spritesList[8];
                }

                int slotCount = res_DIVIDE_CHIP.readByte();
                StartCoroutine(bayChip(gateId, viId));
                StartCoroutine(moBat(slotCount, res_DIVIDE_CHIP, gateId, viId));
            });

        });

        var req_SDIE_BET = new OutBounMessage("SIDE_BET");
        req_SDIE_BET.addHead();
        App.ws.sendHandler(req_SDIE_BET.getReq(), delegate (InBoundMessage res_SIDE_BET)
         {
             App.trace("RECV: [SIDE_BET]");
             int type = res_SIDE_BET.readByte();    //1: có người muốn cược|#: đồng ý cược
             int slotId = res_SIDE_BET.readByte();
             int betId = res_SIDE_BET.readShort();
             int gateId = res_SIDE_BET.readByte();

             slotId = getSlotIdBySvrId(slotId);

             App.trace("type = " + type + "|slotID = " + slotId + "|betId = " + betId + "gateId = " + gateId);
             Debug.Log("SIDE_BET " + "type = " + type + "|slotID = " + slotId + "|betId = " + betId + "gateId = " + gateId);
             if (type == 1)
             {
                 string t = "";
                 foreach (Player pl in playerList.Values.ToList())
                 {
                     if (pl.SlotId == slotId)
                     {
                         t += pl.NickName + " cược biên";
                         break;
                     }
                 }
                 t += (gateId == 0 ? " CHẴN " : " LẺ ") + betId * tableBet + " Gold";
                 xocDiaTextList[17].text = t;

                 sellBuyList[13].SetActive(true);

                 //string t1 = "Cược " + (type == 0 ? "CHẴN " : "LẺ ") + betId * tableBet + " chip";
                 //xocDiaTextList[18 + slotId - 1].text = t1;
                 //sellBuyList[slotId - 1 + 14].SetActive(true);
                 mSideBetInfo = new SideBetInfo(2, detecSvrBySlotId(slotId), (short)betId, gateId);
             }
             else
             {  //Đồng ý cược với mình
                 string t = "Nhận " + (gateId == 0 ? "CHẴN\n" : "LẺ\n") + betId * tableBet + " Gold";
                 xocDiaTextList[18 + slotId - 1].text = t;
                 sellBuyList[slotId + 14 -1 ].SetActive(true);
             }
         });

        var req_OWNER_CHANGED = new OutBounMessage("OWNER_CHANGED");
        req_OWNER_CHANGED.addHead();
        App.ws.sendHandler(req_OWNER_CHANGED.getReq(), delegate (InBoundMessage res_OWNER_CHANGED)
        {
            App.trace("RECV [OWNER_CHANGED]");
            var slotId = res_OWNER_CHANGED.readByte();
            currOwner = slotId;

            if (slotId == mySlotId)  //Mình là cái
            {
                App.trace("Mình lên làm cái");
                ownerList[3].SetActive(false);
                ownerList[0].SetActive(true);
                //realOwnerSlotIdIntalbe = 0;
                playerList[0].IsOwner = true;
                if (playerList.ContainsKey(3))
                    playerList[3].IsOwner = false;


                return;
            }


            //Thằng khác thành cái
            int slotTmp = -1;
            foreach (Player pl in playerList.Values.ToList())
            {
                if (pl.SvSlotId == slotId)
                {
                    slotTmp = pl.SlotId;
                    break;
                }
            }
            playerList[slotTmp].IsOwner = true;
            playerList[0].IsOwner = false;

            Player plTmp = playerList[slotTmp];
            plTmp.SlotId = 3;
            if (playerList.ContainsKey(3))
            {
                playerList[3].IsOwner = false;
                playerList[slotTmp] = playerList[3];
                playerList[slotTmp].SlotId = slotTmp;
                playerList[3] = plTmp;
                //setInfo(playerList[slotTmp], avatarsList[slotTmp], infoObjectList[slotTmp], balanceTextList[slotTmp], nickNameTextList[slotTmp], ownerList[slotTmp]);
                //setInfo(playerList[3], avatarsList[3], infoObjectList[3], balanceTextList[3], nickNameTextList[3], ownerList[3]);

                chipList[3] = playerList[3].ChipBalance;
                balanceTextList[3].text = App.formatMoney(playerList[3].ChipBalance.ToString());
                nickNameTextList[3].text = App.formatNickName(playerList[3].NickName, 10);
                Sprite sprTmp = avatarsList[3].overrideSprite;
                avatarsList[3].overrideSprite = avatarsList[slotTmp].overrideSprite;

                chipList[slotTmp] = playerList[slotTmp].ChipBalance;
                balanceTextList[slotTmp].text = App.formatMoney(playerList[slotTmp].ChipBalance.ToString());
                nickNameTextList[slotTmp].text = App.formatNickName(playerList[slotTmp].NickName, 10);
                avatarsList[slotTmp].overrideSprite = sprTmp;

                App.trace("Thằng " + slotTmp + " lên làm cái đổi với thằng cái cũ");
            }
            else
            {
                playerList.Remove(slotTmp);
                infoObjectList[slotTmp].SetActive(false);
                avatarsList[slotTmp].overrideSprite = spritesList[0];
                exitsSlotList[slotTmp] = false;
                ownerList[slotTmp].SetActive(false);

                playerList[3] = plTmp;
                playerList[3].SlotId = 3;
                setInfo(playerList[3], avatarsList[3], infoObjectList[3], balanceTextList[3], nickNameTextList[3], ownerList[3]);
                exitsSlotList[slotTmp] = false;
                exitsSlotList[3] = true;
                App.trace("Thằng " + slotTmp + " lên làm cái");
            }
            /*
            if (slotId == mySlotId)  //Mình là cái
            {
                App.trace("Mình lên làm cái");
                ownerList[3].SetActive(false);
                ownerList[0].SetActive(true);
                //realOwnerSlotIdIntalbe = 0;
                playerList[0].IsOwner = true;
                if (playerList.ContainsKey(3))
                    playerList[3].IsOwner = false;


                return;
            }

            App.trace("Thằng khác lên làm cái");
            //Thằng khác thành cái
            int slotTmp = -1;
            foreach (Player pl in playerList.Values.ToList())
            {
                if (pl.SvSlotId == slotId)
                {
                    slotTmp = pl.SlotId;
                    break;
                }
            }
            playerList[slotTmp].IsOwner = true;
            playerList[0].IsOwner = false;
            ownerList[0].SetActive(false);

            Player plTmp = playerList[slotTmp];
            if (playerList.ContainsKey(3))
            {
                playerList[3].IsOwner = false;
                playerList[slotTmp] = playerList[3];
                playerList[3] = plTmp;
                setInfo(playerList[slotTmp], avatarsList[slotTmp], infoObjectList[slotTmp], balanceTextList[slotTmp], nickNameTextList[slotTmp], ownerList[slotTmp]);
                setInfo(playerList[3], avatarsList[3], infoObjectList[3], balanceTextList[3], nickNameTextList[3], ownerList[3]);
            }
            else
            {
                playerList.Remove(slotTmp);
                infoObjectList[slotTmp].SetActive(false);
                avatarsList[slotTmp].overrideSprite = spritesList[0];
                exitsSlotList[slotTmp] = false;
                ownerList[slotTmp].SetActive(false);

                playerList[3] = plTmp;
                setInfo(playerList[3], avatarsList[3], infoObjectList[3], balanceTextList[3], nickNameTextList[3], ownerList[3]);
                exitsSlotList[slotTmp] = false;
            }
            */
        });

        var req_START_MATCH = new OutBounMessage("START_MATCH");
        req_START_MATCH.addHead();
        //req_getTableChange.print();
        App.ws.sendHandler(req_START_MATCH.getReq(), delegate (InBoundMessage res_START_MATCH)
        {


            changeClientMode(false);
            stateBtns[7].gameObject.SetActive(false);
            SoundManager.instance.PlayUISound(SoundFX.CARD_START_MATCH);
            isPlaying = true;
            foreach(int key in xuRtfList.Keys.ToList())
            {
                for(int i = 0; i < xuRtfList[key].Count; i++)
                {
                    if (xuRtfList[key][i].Rtf != null)
                    {
                        Destroy(xuRtfList[key][i].Rtf.gameObject);
                    }
                }
            }
            xuRtfList.Clear();
            App.trace("RECV START_MATCH");
            foreach (Text mText in moneyErnedTxtList)
            {
                mText.gameObject.SetActive(false);
            }
            loadPlayerMatchPoint(res_START_MATCH);
            loadBoardData(res_START_MATCH);


            if (mySlotId >= 0)
                enterState2(stateById[beginStateId]);
            else
                enterState2(stateByCode["viewTable"]);
        });

        var req_KICK_PLAYER = new OutBounMessage("KICK_PLAYER");    //KÍCH NG CHƠI
        req_KICK_PLAYER.addHead();
        //req_getTableChange.print();
        App.ws.sendHandler(req_KICK_PLAYER.getReq(), delegate (InBoundMessage res)
        {
            App.trace("KICK_PLAYER");
            int status = res.readByte();
            string content = res.readString();
            App.trace("status = " + status + "|content = " + content);
            if (status == -1)
            {
                App.showErr(content);
                return;
            }
            if (status == 2)
            {
                isKicked = true;
                isPlaying = false;
                statusKick = content;
                backToTableList();

            }

        });

        #region //PHẦN RIÊNG
        var req_SET_PLAYER_ATTR = new OutBounMessage("SET_PLAYER_ATTR");
        req_SET_PLAYER_ATTR.addHead();
        App.ws.sendHandler(req_SET_PLAYER_ATTR.getReq(), delegate (InBoundMessage res_SET_PLAYER_ATTR)
        {
            int slotId = res_SET_PLAYER_ATTR.readByte();
            string icon = res_SET_PLAYER_ATTR.readAscii();
            string content = res_SET_PLAYER_ATTR.readAscii();
            int action = res_SET_PLAYER_ATTR.readByte();
            App.trace("slotId = " + slotId + "|icon = " + icon + "|content = " + content + "|action = " + action);
            Debug.Log("slotId = " + slotId + "|icon = " + icon + "|content = " + content + "|action = " + action);
            string[] attList = null;
            int amount = 0;
            int doorId = -1;

            switch (icon)
            {
                case "bet":
                    SoundManager.instance.PlayUISound(SoundFX.CARD_TAI_XIU_BET);
                    attList = content.Split('-');
                    if(attList.Length > 1)
                    {
                        amount = int.Parse(attList[0]);
                        doorId = int.Parse(attList[1]);
                        if (slotId == mySlotId)
                        {
                            string t = balanceTextList[7].text;
                            balanceTextList[7].text = App.formatMoney((App.formatMoneyBack(t) - amount).ToString());
                        }
                        List<int> listChip = new List<int>();
                        listChip = groupChip(amount);

                        for(int i = 0; i < listChip.Count; i++)
                        {
                            //App.trace("LIST CHIP LENG = " + listChip.Count + "|" + listChip[i]);
                            string t = xocDiaTextList[doorId + 5].text;
                            if(t.Length == 0)
                            {
                                t = "0";
                            }
                            xocDiaTextList[doorId + 5].text = (int.Parse(t) + listChip[i]).ToString();

                            if (slotId == mySlotId)
                            {
                                string t1 = xocDiaTextList[doorId + 11].text;
                                if (t1.Length == 0)
                                    t1 = "0";
                                xocDiaTextList[doorId + 11].text = (int.Parse(t1) + listChip[i]).ToString();
                            }

                            Image flyImg = Instantiate(imgToClone[8], imgToClone[8].transform.parent, false);
                            int stt = listChip[i]/(int)tableBet;
                            switch (stt)
                            {
                                case 1:
                                    stt = 1;
                                    break;
                                case 5:
                                    stt = 2;
                                    break;
                                case 10:
                                    stt = 3;
                                    break;
                                case 20:
                                    stt = 4;
                                    break;
                                default:
                                    stt = 1;
                                    break;
                            }
                            flyImg.sprite = spritesList[(int)(stt + 10)];
                            RectTransform rtf = flyImg.GetComponent<RectTransform>();

                            rtf.anchoredPosition = XuClonepos[getSlotIdBySvrId(slotId)];
                            //App.trace("RTF2 " + rtf.anchoredPosition.x + "|" + rtf.anchoredPosition.y + "PIVOT " + rtf.pivot.x + "|" + rtf.pivot.y);
                            RectTransform rtf2 = xocDiaBtnList[doorId].GetComponent<RectTransform>();
                            int randX = (int)rtf2.rect.width/2 - 60;
                            int randY = (int)rtf2.rect.height/2 - 60;
                            rtf.pivot = rtf2.pivot;
                            rtf.anchorMin = rtf2.anchorMin;
                            rtf.anchorMax = rtf2.anchorMax;
                            //App.trace("RTF2 " + rtf.anchoredPosition.x + "|" + rtf.anchoredPosition.y + "PIVOT " + rtf.pivot.x + "|" + rtf.pivot.y);
                            XuRTF xuRTF = new XuRTF(rtf, getSlotIdBySvrId(slotId));
                            flyImg.gameObject.SetActive(true);
                            if (xuRtfList.ContainsKey(doorId))
                            {

                                xuRtfList[doorId].Add(xuRTF);
                            }
                            else
                            {
                                List<XuRTF> ls = new List<XuRTF>();
                                ls.Add(xuRTF);
                                xuRtfList.Add(doorId, ls);
                            }
                            //App.trace("bay sl = " + xuRTF.SlotId);
                            DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, new Vector2(rtf2.anchoredPosition.x + UnityEngine.Random.RandomRange(-randX,randX), rtf2.anchoredPosition.y + UnityEngine.Random.RandomRange(-randY, randY)), .5f).SetEase(Ease.OutBack).OnComplete(()=> {
                                rtf.DOScale(.6f, .2f);
                            });


                        }
                    }


                    break;
                case "unbet":
                    //Bay chip về user hủy cược
                    attList = content.Split(';');
                    List<int> keyList = new List<int>();    //Danh sách các cửa đã đặt
                    List<int> amountList = new List<int>();
                    for(int i =0; i < attList.Length; i++)
                    {
                        string[] tmpArr = attList[i].Split('-');
                        if (int.Parse(tmpArr[0]) > 0)
                        {
                            keyList.Add(int.Parse(tmpArr[1]));
                            amountList.Add(int.Parse(tmpArr[0]));
                        }
                    }
                    int slot = getSlotIdBySvrId(slotId);

                    for (int i = 0; i < keyList.Count; i++)
                    {
                        int a = 0;
                        List<int> mgrChip = groupChip(amountList[i]);
                        foreach (int m in mgrChip)
                        {
                            a += m;
                        }
                        int gate = keyList[i];
                        if (xuRtfList.ContainsKey(gate))
                        {
                            foreach (XuRTF mxurtf in xuRtfList[gate])
                            {
                                if (mxurtf == null)
                                    continue;
                                if (slot == mxurtf.SlotId)
                                {
                                    RectTransform rtf = mxurtf.Rtf;
                                    DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, XuClonepos[mxurtf.SlotId], 1f).OnComplete(() =>
                                    {
                                        rtf.DOScale(Vector2.one / 2, .5f).OnComplete(() =>
                                        {
                                            xuRtfList[gate].Remove(mxurtf);
                                            Destroy(rtf.gameObject);
                                        });
                                    });
                                }

                            }
                        }
                        int tmp = int.Parse(xocDiaTextList[gate + 5].text) - a;
                        xocDiaTextList[gate + 5].text = tmp == 0 ? "" : tmp.ToString();

                    }
                    if (slotId == mySlotId)
                    {
                        balanceTextList[7].text = balanceTextList[0].text;
                        for (int i = 0; i < 6; i++)
                        {
                            xocDiaTextList[11 + i].text = "";
                        }
                        isBeted = false;
                    }
                    break;
                case "sell_gate":
                    if(content != "-1")
                    {
                        sellGateId = int.Parse(content);    //0: chẵn|1: lẻ

                        //xocDiaTextList[1].text = sellGateId == 0 ? "Nhà cái bán chẵn" : "Nhà cái bán lẻ";
                        string t = sellGateId == 0 ? "Nhà cái bán chẵn" : "Nhà cái bán lẻ";
                        //App.trace(t + "|" + sellGateId);
                        updateNotiText(t);

                        List<int> sellGatess = new List<int>();
                        if(sellGateId == 0)
                        {
                            sellGatess.Add(2);
                            sellGatess.Add(3);
                        }
                        else
                        {
                            sellGatess.Add(4);
                            sellGatess.Add(5);
                        }

                        //Bay chip từ cửa vị bán về user đã đặt
                        foreach(int i in sellGatess)
                        {
                            xocDiaTextList[i + 5].text = "";
                            xocDiaTextList[i + 11].text = "";
                            if (xuRtfList.ContainsKey(i))
                                foreach (XuRTF mxurtf in xuRtfList[i])
                                {
                                    if (mxurtf == null)
                                        continue;
                                    RectTransform rtf = mxurtf.Rtf;
                                    DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, XuClonepos[mxurtf.SlotId], 1f).OnComplete(() =>
                                    {
                                        rtf.DOScale(Vector2.one / 2, .5f).OnComplete(() =>
                                        {
                                            xuRtfList[i].Remove(mxurtf);
                                            Destroy(rtf.gameObject);
                                        });
                                    });
                                }
                        }

                    }
                    else
                    {
                        //Cân cửa
                    }
                    break;
                case "buy_gate":
                    isBuyGate = true;
                    buyGateSlotId = slotId;
                    //App.trace("buyGateSlotId = " + slotId);
                    slotId = getSlotIdBySvrId(slotId);
                    moneyErnedTxtList[slotId + 7].text = "MUA " + (sellGateId == 0 ? "CHẴN" : "LẺ");
                    sellBuyList[slotId].SetActive(true);
                    moneyErnedTxtList[slotId + 7].gameObject.SetActive(true);
                    stateBtns[3].gameObject.SetActive(false);
                    break;
            }

        });

        var req_SET_TURN = new OutBounMessage("SET_TURN");
        req_SET_TURN.addHead();
        App.ws.sendHandler(req_SET_TURN.getReq(), delegate (InBoundMessage res_SET_TURN)
        {
            App.trace("RECV [SET_TURN]");
            int slotId = res_SET_TURN.readByte();
            int turnTimeout = res_SET_TURN.readShort();

            if(slotId == -2)
            {
                if (turnTimeout > 0) {

                    if (regQuit)    //Nếu đăng ký rời bàn thì rời
                    {
                        backToTableList();
                        return;
                    }
                    showCountDown(turnTimeout);
                    updateNotiText("Nhà cái chuẩn bị xóc");
                    if (currOwner == mySlotId)
                        stateBtns[7].gameObject.SetActive(true);
                }

                //xocDiaTextList[1].transform.parent.gameObject.SetActive(true);

            }
            else
            {
                if (turnTimeout > 0)
                {
                    //currTimeOut = turnTimeout;
                    currTimeOut = turnTimeout;
                    App.trace("slotID = " + slotId + "|turnTimeOut = " + turnTimeout);
                    if (!isSaking)
                    {
                        showCountDown(turnTimeout);
                    }
                }
                else
                {
                    showCountDown(0, false);
                }

            }



            res_SET_TURN.readShort();   //BỎ
        });

        #endregion
    }

    private void getTableDataEx()
    {
        var req_GET_TABLE_DATA_EX = new OutBounMessage("GET_TABLE_DATA_EX");
        req_GET_TABLE_DATA_EX.addHead();
        req_GET_TABLE_DATA_EX.writeAcii("");
        //req_getTableChange.print();
        App.ws.send(req_GET_TABLE_DATA_EX.getReq(), delegate (InBoundMessage res)
        {
            App.trace("RECV GET_TABLE_DATA_EX");

            loadStateData(res); //DONE.
            loadTableData(res); //DONE.
            loadPlayerMatchPoint(res);  //DONE.
            loadBoardData(res); //DONE.

        });
    }
    private Dictionary<int, State> stateById;
    private Dictionary<string, State> stateByCode;
    private int beginStateId;
    private void loadStateData(InBoundMessage res)
    {
        stateByCode = new Dictionary<string, State>();
        stateById = new Dictionary<int, State>();

        int count = res.readByte();
        App.trace("loadstateData " + count);
        for (int i = 0; i < count; i++)
        {
            //App.trace(res.readByte() + "|" + res.readAscii() + "|" + res.readByte());
            State mstate = new State(res.readByte(), res.readAscii(), res.readByte());

            int commandCount = res.readByte();
            for (int j = 0; j < commandCount; j++)
            {
                //App.trace(res.readByte() + "|" + res.readAscii() + "|" + res.readString() + "|" + res.readByte() + "|" + res.readByte());
                StateCommand command = new StateCommand(res.readByte(), res.readAscii(), res.readString());
                command.waitResult = true;
                command.fillBoardState = res.readByte() == 1;
                command.takeConfirmation = res.readByte() == 1;

                mstate.commands.Add(command);
            }
            stateById.Add(mstate.Id, mstate);
            stateByCode.Add(mstate.Code, mstate);
        }
        beginStateId = res.readByte();

        State stateStart = new State(0, "waitStart", 1);
        StateCommand command_START = new StateCommand(0, "START", "Bắt đầu");
        command_START.action  = delegate(int s){
            App.trace("START NÈ");
        };
        stateStart.commands.Add(command_START);
        stateById.Add(stateStart.Id, stateStart);
        stateByCode.Add(stateStart.Code, stateStart);

        var state_viewTable = new State(1, "viewTable", 1);
        stateById.Add(state_viewTable.Id, state_viewTable);
        stateByCode.Add(state_viewTable.Code, state_viewTable);

        foreach(State tmpState in stateById.Values)
        {
            tmpState.commandsByPosition = new Dictionary<int, List<StateCommand>>();
            for(int i = 0; i < tmpState.commands.Count; i++)
            {
                StateCommand command = tmpState.commands[i];
                List<StateCommand> commands = null;
                if (tmpState.commandsByPosition.ContainsKey(command.Position))
                {
                    commands = tmpState.commandsByPosition[command.Position];
                }
                if (commands == null)
                {
                    commands = new List<StateCommand>();
                    tmpState.commandsByPosition[command.Position] =  commands;
                }
                commands.Add(command);
            }
        }

    }

    private void loadTableData(InBoundMessage res)
    {
        playerList = new Dictionary<int, Player>();

        mySlotId = res.readByte();
        App.trace("MY SLOT ID = " + mySlotId);
        bool isPlaying = res.readByte() == 1;

        int count = res.readByte();

        for (int i = 0; i < count; i++)
        {

            int slotId = res.readByte();
            long playerId = res.readLong();
            string nickName = res.readAscii();
            int avatarId = res.readShort();
            string avatar = res.readAscii();
            bool isMale = res.readByte() == 1;
            long chipBalance = res.readLong();
            long starBalance = res.readLong();
            long score = res.readLong();
            int level = res.readByte();
            bool isOwner = res.readByte() == 1;
            //App.trace("slotId = " + slotId + "|nickName = " + nickName + "|isOwner = " + isOwner + "|MySlotId = " + mySlotId);


            //App.trace("== Player " + i + " = " + nickName + "|slotId = " + slotId);
            if (isOwner)
            {
                currOwner = slotId;
            }

            //slotId = detecSlotIdBySvrId(slotId,isOwner);

            int tmp = slotId;
            if (slotId > -1)
            {
                Player player = new Player(detecSlotIdBySvrId(slotId, isOwner),tmp, playerId, nickName, avatarId, avatar, isMale, chipBalance, starBalance, score, level, isOwner);

                App.trace("PLAYER SLOT " + player.SlotId + "ADDED!" + player.SvSlotId + "|isOwner = " + isOwner);
                if (!playerList.ContainsKey(player.SlotId))
                {
                    exitsSlotList[player.SlotId] = true;
                    playerList.Add(player.SlotId, player);
                } else
                {
                    player.SlotId = detecSlotIdBySvrId(tmp);
                    exitsSlotList[player.SlotId] = true;
                }
            }
            /*
            try{
                setInfo(player, avatarsList[slotId], infoObjectList[slotId], balanceTextList[slotId], nickNameTextList[slotId], ownerList[slotId]);
                exitsSlotList[slotId] = true;
            }
            catch
            {
                App.trace("sai ID = " + slotId);
            }
            */




        }

        updateTableSlot();

        //App.trace("So slot " + count);
        int currentTurnSlotId = res.readByte();
        currTimeOut = res.readShort();
        showCountDown(currTimeOut);
        int slotRemainDuration = res.readShort();
        var currentState = res.readByte();
        enterState(stateById[currentState]);
        if (mySlotId >= 0)
            enterState2(stateById[beginStateId]);
    }

    private void loadPlayerMatchPoint(InBoundMessage res)
    {
        int count = res.readByte();
        for (int i = 0; i < count; i++)
        {
            int slotId = res.readByte();
            int point = res.readInt();
            //App.trace("slotId = " + slotId + ", point = " + point);
        }
        App.trace("loadPlayerMatchPoint = " + count);
    }

    private void loadBoardData(InBoundMessage res)
    {
        int count = res.readByte();
        //
        for (int i = 0; i < count; i++)
        {
            int slotId = res.readByte();
            string listBet = res.readString();
            bool isPlaying = res.readByte() == 1;
            App.trace("slotId = " + slotId + "|list bet = " + listBet + "|isPlaying = " + isPlaying);
            Debug.Log("loadBoardData => " + "slotId = " + slotId + "|list bet = " + listBet + "|isPlaying = " + isPlaying);
            string[] attList = listBet.Split(';');
            List<int> keyList = new List<int>();    //Danh sách các cửa đã đặt
            List<int> amountList = new List<int>();
            for (int j = 0; j < attList.Length; j++)
            {
                string[] tmpArr = attList[j].Split('-');
                if (int.Parse(tmpArr[0]) > 0)
                {
                    keyList.Add(int.Parse(tmpArr[1]));
                    amountList.Add(int.Parse(tmpArr[0]));
                }
            }
            for(int j = 0; j < keyList.Count; j++)
            {
                string t = xocDiaTextList[keyList[j] + 5].text;
                if (t.Length == 0)
                {
                    t = "0";
                }
                xocDiaTextList[keyList[j] + 5].text = (int.Parse(t) + amountList[j]).ToString();

                if (slotId == mySlotId)
                {
                    string t1 = xocDiaTextList[keyList[j] + 11].text;
                    if (t1.Length == 0)
                        t1 = "0";
                    xocDiaTextList[keyList[j] + 11].text = (int.Parse(t1) + amountList[j]).ToString();
                }

                Image flyImg = Instantiate(imgToClone[8], imgToClone[8].transform.parent, false);
                int stt = amountList[j] / (int)tableBet;
                switch (stt)
                {
                    case 1:
                        stt = 1;
                        break;
                    case 5:
                        stt = 2;
                        break;
                    case 10:
                        stt = 3;
                        break;
                    case 20:
                        stt = 4;
                        break;
                    default:
                        stt = 1;
                        break;
                }
                flyImg.sprite = spritesList[(int)(stt + 10)];
                RectTransform rtf = flyImg.GetComponent<RectTransform>();

                rtf.anchoredPosition = XuClonepos[getSlotIdBySvrId(slotId)];
                //App.trace("RTF2 " + rtf.anchoredPosition.x + "|" + rtf.anchoredPosition.y + "PIVOT " + rtf.pivot.x + "|" + rtf.pivot.y);
                RectTransform rtf2 = xocDiaBtnList[keyList[j]].GetComponent<RectTransform>();
                int randX = (int)rtf2.rect.width / 2 - 60;
                int randY = (int)rtf2.rect.height / 2 - 60;
                rtf.pivot = rtf2.pivot;
                rtf.anchorMin = rtf2.anchorMin;
                rtf.anchorMax = rtf2.anchorMax;
                //App.trace("RTF2 " + rtf.anchoredPosition.x + "|" + rtf.anchoredPosition.y + "PIVOT " + rtf.pivot.x + "|" + rtf.pivot.y);
                XuRTF xuRTF = new XuRTF(rtf, getSlotIdBySvrId(slotId));
                flyImg.gameObject.SetActive(true);
                if (xuRtfList.ContainsKey(keyList[j]))
                {

                    xuRtfList[keyList[j]].Add(xuRTF);
                }
                else
                {
                    List<XuRTF> ls = new List<XuRTF>();
                    ls.Add(xuRTF);
                    xuRtfList.Add(keyList[j], ls);
                }
                //App.trace("bay sl = " + xuRTF.SlotId);
                DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, new Vector2(rtf2.anchoredPosition.x + UnityEngine.Random.RandomRange(-randX, randX), rtf2.anchoredPosition.y + UnityEngine.Random.RandomRange(-randY, randY)), .5f).SetEase(Ease.OutBack).OnComplete(() => {
                    rtf.DOScale(.6f, .2f);
                });
            }
        }

        int hisCount = res.readByte();

        List<int> hisList = new List<int>();
        int chanCount = 0;
        int leCount = 0;
        for(int i = 0; i < hisCount; i++)
        {
            hisList.Add(res.readByte());
            if (hisList[i] % 2 == 0)
            {
                chanCount++;
            }
            else
            {
                leCount++;
            }
        }

        if(hisList.Count == 20)
        {
            hisList.RemoveAt(0);
        }
        hisList.Add(5);

        Transform tfm = imgToClone[0].transform.parent;
        count = tfm.childCount;
        //App.trace("==================" + count);
        for (int i = count - 1; i > 0; i--)
        {
            DestroyImmediate(tfm.GetChild(i).gameObject);
        }

        for (int i = 0; i < hisList.Count; i++)
        {
            Image img = Instantiate(imgToClone[0], imgToClone[0].transform.parent, false);
            img.sprite = spritesList[hisList[i] + 1];
            img.gameObject.SetActive(true);
        }

        xocDiaTextList[3].text = App.listKeyText["XOCDIA_CHAN"] + chanCount; //"Chẵn " + chanCount;
        xocDiaTextList[4].text = App.listKeyText["XOCDIA_LE"] + leCount;//"Lẻ " + leCount;
    }

    #region //CLASS PLAYER
    public class Player
    {
        string nickName, avatar;
        int slotId, svSlotId, avatarId, level;
        bool isMale, isOwner;
        long playerId, chipBalance, starBalance, score;

        public int SlotId
        {
            get
            {
                return slotId;
            }

            set
            {
                slotId = value;
            }
        }

        public int SvSlotId
        {
            get
            {
                return svSlotId;
            }

            set
            {
                svSlotId = value;
            }
        }

        public string NickName
        {
            get
            {
                return nickName;
            }

            set
            {
                nickName = value;
            }
        }

        public int AvatarId
        {
            get
            {
                return avatarId;
            }

            set
            {
                avatarId = value;
            }
        }

        public string Avatar
        {
            get
            {
                return avatar;
            }

            set
            {
                avatar = value;
            }
        }

        public int Level
        {
            get
            {
                return level;
            }

            set
            {
                level = value;
            }
        }

        public bool IsMale
        {
            get
            {
                return isMale;
            }

            set
            {
                isMale = value;
            }
        }

        public bool IsOwner
        {
            get
            {
                return isOwner;
            }

            set
            {
                isOwner = value;
            }
        }

        public long PlayerId
        {
            get
            {
                return playerId;
            }

            set
            {
                playerId = value;
            }
        }

        public long ChipBalance
        {
            get
            {
                return chipBalance;
            }

            set
            {
                chipBalance = value;
            }
        }

        public long StarBalance
        {
            get
            {
                return starBalance;
            }

            set
            {
                starBalance = value;
            }
        }

        public long Score
        {
            get
            {
                return score;
            }

            set
            {
                score = value;
            }
        }

        public Player(int slotId,int svSlotId, long playerId, string nickName, int avatarId, string avatar, bool isMale, long chipBalance, long starBalance, long score, int level, bool isOwner)
        {
            this.SlotId = slotId;
            this.SvSlotId = svSlotId;
            this.PlayerId = playerId;
            this.NickName = nickName;
            this.AvatarId = avatarId;
            this.IsMale = isMale;
            this.ChipBalance = chipBalance;
            this.StarBalance = starBalance;
            this.Score = score;
            this.Level = level;
            this.isOwner = isOwner;
            this.avatar = avatar;
            /*
             * var slotId = res.readByte();
            var playerId = res.readLong();
            var nickName = res.readAscii();
            var avatarId = res.readShort();
            var avatar = res.readAscii();
            var isMale = res.readByte() == 1;
            var chipBalance = res.readLong();
            var starBalance = res.readLong();
            var score = res.readLong();
            var level = res.readByte();
            var isOwner = res.readByte() == 1;
             * */
        }
    }
    #endregion
    private int realOwnerSlotIdIntalbe = -1;    //Vị trí thật sự của thằng chủ khi chưa đổi với số 3
    private int detecSlotIdBySvrId(int slotId, bool isOwner = false)
    {
        int rs = slotId;
        if (mySlotId > -1)
        {
            rs = rs - mySlotId;
            rs = rs < 0 ? (rs + 7) : rs;
        }

        if (isOwner)
        {
            if(currOwner != mySlotId)
            {
                return 3;
            }
        }
        if (rs == 3 || exitsSlotList[rs] == true)
        {
            for(int i = 0; i < 7; i++)
            {
                if (exitsSlotList[i] == false && i != 3)
                {
                    if(mySlotId > -1)
                    {
                        if (i != 0)
                        {
                            return i;
                        }
                    }
                    else
                    {
                        return i;
                    }
                }

            }
        }
        return rs;
    }

    private int detecSvrBySlotId(int slotId)
    {
        /*
        if (mySlotId != currOwner)
            if (slotId == realOwnerSlotIdIntalbe)
            {
                return slotId = 3;
            }


        int rs = slotId;
        if (mySlotId > -1)
        {
            rs = rs + mySlotId;
            rs = rs > 7 ? (rs - 7) : rs;
        }
        return rs;
        */
        foreach(Player pl in playerList.Values.ToList())
        {
            if (pl.SlotId == slotId)
                return pl.SvSlotId;
        }
        return 0;
    }

    /// <summary>
    /// Xác định slotId bằng SV Id
    /// </summary>
    /// <param name="slotId"></param>
    /// <returns></returns>
    private int getSlotIdBySvrId(int slotId)
    {
        foreach (Player pl in playerList.Values.ToList())
        {
            if (pl.SvSlotId == slotId)
                return pl.SlotId;
        }
        return 0;
    }

    private void setInfo(Player player, Image im, GameObject infoObj, Text balanceText, Text nickNamText, GameObject ownerImg)
    {
        //im.gameObject.transform.localScale = Vector3.one;
        StartCoroutine(App.loadImg(im, App.getAvatarLink2(player.Avatar, (int)player.PlayerId), player.SvSlotId == mySlotId));
        if (infoObj != null)
            infoObj.SetActive(true);
        chipList[player.SlotId] = player.ChipBalance;
        balanceText.text = "100.0 K";
        balanceText.text = player.SlotId == 0 ? App.formatMoney(player.ChipBalance.ToString()) : App.formatMoneyAuto(player.ChipBalance);
        nickNamText.text = App.formatNickName(player.NickName, 10);
        ownerImg.SetActive(player.IsOwner);

    }

    private Coroutine preCoroutine = null;
    private void showCountDown(int time, bool toShow = true)
    {
        if(preCoroutine != null)
            StopCoroutine(preCoroutine);
        if (toShow == false)
        {
            xocDiaTextList[2].gameObject.SetActive(false);
            return;
        }
        App.trace("Time change");
        preCoroutine = StartCoroutine(_showCountDown(time));
    }
    private IEnumerator _showCountDown(int time)
    {
        //Text txt = xocDiaTextList[2];
        int count = time;
        //xocDiaTextList[2].text = "";
        //txt.text = count.ToString();
        if (!xocDiaTextList[2].gameObject.activeSelf)
        {
            xocDiaTextList[2].gameObject.SetActive(true);
        }
        while (count > 0)
        {
            xocDiaTextList[2].text = count.ToString();

            yield return new WaitForSeconds(1f);
            count--;
        }
        //xocDiaTextList[2].gameObject.SetActive(false);
        yield break;
    }

    private IEnumerator moBat(int slotCount, InBoundMessage res_DIVIDE_CHIP, int gateId, int viId)
    {

        yield return new WaitForSeconds(4f);
        //xocDiaTextList[1].text = "Tổng kết";
        updateNotiText("Tổng kết");
        xocDiaTextList[2].gameObject.SetActive(false);  //Không hiển thị đếm nữa
        for (var i = 0; i < slotCount; i++)
        {
            int slotId = res_DIVIDE_CHIP.readByte();
            int amount = res_DIVIDE_CHIP.readInt();
            //App.trace("slotId = " + slotId + "|amount = " + amount);
            if (slotId == mySlotId)
            {
                if (amount > 0)
                    SoundManager.instance.PlayUISound(SoundFX.CARD_WIN);
                else
                    SoundManager.instance.PlayUISound(SoundFX.CARD_LOSE);
            }


            slotId = getSlotIdBySvrId(slotId);
            if (amount == 0)
                continue;
            Text txt = moneyErnedTxtList[slotId];
            txt.text = amount > 0 ? "+" + amount.ToString() : "-" + (-1 * amount).ToString();
            //txt.color = amount > 0 ? colorList[0] : colorList[1];
            txt.font = amount > 0 ? xocDiaFontLs[0] : xocDiaFontLs[1];
            txt.gameObject.SetActive(true);
            txt.transform.DOScale(new Vector3(1.2f, 1.2f, 1f), .5f).SetLoops(-1, LoopType.Yoyo).OnComplete(() => {
                txt.transform.localScale = Vector3.one;
                txt.gameObject.SetActive(false);
            });
        };

        yield break;
    }

    private IEnumerator bayChip(int gateId, int viId)
    {
        yield return new WaitForSeconds(1f);    //Chờ 1s cho bát mở ra
        if (sellGateId != -1)   //Nếu cái bán cửa
        {
            if (isBuyGate == false)
            {
                #region//----- k có ai mua => bay chip từ cửa bán về user
                if (xuRtfList.ContainsKey(sellGateId))
                {
                    //App.trace("BAN MA K AI MUA");
                    foreach (XuRTF mxurtf in xuRtfList[sellGateId])
                    {
                        if (mxurtf == null)
                            continue;
                        RectTransform rtf = mxurtf.Rtf;

                        DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, XuClonepos[mxurtf.SlotId], .5f).OnComplete(() =>
                        {
                            rtf.DOScale(Vector2.one / 2, .5f).OnComplete(() =>
                            {
                                xuRtfList[sellGateId].Remove(mxurtf);
                                Destroy(rtf.gameObject);
                            });
                        });
                    }
                    yield return new WaitForSeconds(.5f);
                }
                #endregion
            }
            else
            {
                #region//= --Cái bán và có ng mua
                if (sellGateId != gateId)    //=-- user mua x về y thì tiền cửa x sẽ bay về mặt thằng mua
                {

                    buyGateSlotId = getSlotIdBySvrId(buyGateSlotId);
                    if (xuRtfList.ContainsKey(sellGateId))
                        foreach (XuRTF mxurtf in xuRtfList[sellGateId])
                        {
                            if (mxurtf == null)
                                continue;
                            //App.trace("- MUA X VE Y");
                            RectTransform rtf = mxurtf.Rtf;
                            DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, XuClonepos[buyGateSlotId], 1f).OnComplete(() =>
                            {
                                rtf.DOScale(Vector2.one / 2, .5f).OnComplete(() =>
                                {
                                    xuRtfList[sellGateId].Remove(mxurtf);
                                    Destroy(rtf.gameObject);
                                });
                            });
                        }
                    yield return new WaitForSeconds(.5f);

                }
                else
                {

                    //=-- user mua x về x  (nó thua) => 0: tiền của nó bay ra cửa x | 1: tiền từ cửa x bay về user đặt cửa X
                    List<XuRTF> returnXuList = new List<XuRTF>();
                    if (xuRtfList.ContainsKey(sellGateId))
                    {
                        buyGateSlotId = getSlotIdBySvrId(buyGateSlotId);
                        List<XuRTF> tmpList = xuRtfList[sellGateId];
                        for (int j = 0; j < tmpList.Count; j++)
                        {
                            XuRTF xurtf0 = tmpList[j];
                            Image flyImg = Instantiate(imgToClone[8], imgToClone[8].transform.parent, false);
                            flyImg.sprite = xurtf0.Rtf.gameObject.GetComponent<Image>().sprite;
                            RectTransform rtf = flyImg.GetComponent<RectTransform>();

                            rtf.anchoredPosition = XuClonepos[buyGateSlotId];
                            //App.trace("RTF2 " + rtf.anchoredPosition.x + "|" + rtf.anchoredPosition.y + "PIVOT " + rtf.pivot.x + "|" + rtf.pivot.y);
                            RectTransform rtf2 = xocDiaBtnList[sellGateId].GetComponent<RectTransform>();
                            int randX = (int)rtf2.rect.width / 2 - 30;
                            int randY = (int)rtf2.rect.height / 2 - 30;
                            rtf.pivot = rtf2.pivot;
                            rtf.anchorMin = rtf2.anchorMin;
                            rtf.anchorMax = rtf2.anchorMax;
                            //App.trace("RTF2 " + rtf.anchoredPosition.x + "|" + rtf.anchoredPosition.y + "PIVOT " + rtf.pivot.x + "|" + rtf.pivot.y);

                            flyImg.gameObject.SetActive(true);

                            //xuRtfList[win[i]] = tmpList;
                            returnXuList.Add(new XuRTF(rtf, xurtf0.SlotId));

                            DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, new Vector2(rtf2.anchoredPosition.x + UnityEngine.Random.RandomRange(-randX, randX), rtf2.anchoredPosition.y + UnityEngine.Random.RandomRange(-randY, randY)), .5f).SetEase(Ease.OutBack).OnComplete(()=> {
                                rtf.DOScale(.6f, .25f);
                            }); ;
                        }
                    }
                    // bay từ cửa x về những thằng mua
                    yield return new WaitForSeconds(.5f);

                    if (xuRtfList.ContainsKey(sellGateId))
                        foreach (XuRTF mxurtf in xuRtfList[sellGateId])
                        {
                            if (mxurtf == null)
                                continue;
                            RectTransform rtf = mxurtf.Rtf;
                            DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, XuClonepos[mxurtf.SlotId], 1f).OnComplete(() =>
                            {
                                rtf.DOScale(Vector2.one / 2, .5f).OnComplete(() =>
                                {
                                    xuRtfList[sellGateId].Remove(mxurtf);
                                    Destroy(rtf.gameObject);
                                });
                            });
                        }
                    foreach (XuRTF mxurtf in returnXuList)
                    {
                        RectTransform rtf = mxurtf.Rtf;
                        DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, XuClonepos[mxurtf.SlotId], 1f).OnComplete(() =>
                        {
                            rtf.DOScale(Vector2.one / 2, .25f).OnComplete(() =>
                            {
                                //xuRtfList[sellGateId].Remove(mxurtf);
                                Destroy(rtf.gameObject);
                            });
                        });
                    }
                    yield return new WaitForSeconds(.5f);

                }
                #endregion
            }

        }
        yield return new WaitForSeconds(1f);
        #region    //=-- TRƯỜNG HỢP BÌNH THƯỜNG - Không mua bán gì

        int temp = 3;
        if (currOwner == mySlotId)
            temp = 0;
        //yield return new WaitForSeconds(2f);
        List<int> win = new List<int>();
        win.Add(gateId);
        if(viId > 1)
        {
            win.Add(viId);
        }
        if(xuRtfList.Count == 0)
        {
            yield break;
        }
        #region //Bay chip từ cửa thua về cái
        foreach (int key in xuRtfList.Keys.ToList())
        {
            if (key != gateId && key != viId)
            {

                foreach (XuRTF xurtf in xuRtfList[key])
                {
                    if (xurtf == null || xurtf.Rtf == null)
                        continue;
                    RectTransform rtf = xurtf.Rtf;
                    DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, XuClonepos[temp], .5f).OnComplete(() =>
                    {
                        rtf.DOScale(Vector2.one / 2, .25f).OnComplete(() =>
                        {
                            xuRtfList[key].Remove(xurtf);
                            Destroy(rtf.gameObject);
                        });
                    });
                }
                if (xuRtfList[key].Count == 0)
                    xuRtfList.Remove(key);
            }

        }
        #endregion
        yield return new WaitForSeconds(1f);
        #region //=----Bay chip từ cái ra cửa về
        List<XuRTF> caiList = new List<XuRTF>();
        for (int i = 0; i < win.Count; i++)
        {
            if (xuRtfList.ContainsKey(win[i]))
            {
                List<XuRTF> tmpList = xuRtfList[win[i]];
                for (int j = 0; j < tmpList.Count; j++)
                {
                    XuRTF xurtf0 = tmpList[j];
                    Image flyImg = Instantiate(imgToClone[8], imgToClone[8].transform.parent, false);
                    flyImg.sprite = xurtf0.Rtf.gameObject.GetComponent<Image>().sprite;
                    RectTransform rtf = flyImg.GetComponent<RectTransform>();

                    rtf.anchoredPosition = XuClonepos[getSlotIdBySvrId(currOwner)];
                    //App.trace("RTF2 " + rtf.anchoredPosition.x + "|" + rtf.anchoredPosition.y + "PIVOT " + rtf.pivot.x + "|" + rtf.pivot.y);
                    RectTransform rtf2 = xocDiaBtnList[win[i]].GetComponent<RectTransform>();
                    int randX = (int)rtf2.rect.width / 2 - 30;
                    int randY = (int)rtf2.rect.height / 2 - 30;
                    rtf.pivot = rtf2.pivot;
                    rtf.anchorMin = rtf2.anchorMin;
                    rtf.anchorMax = rtf2.anchorMax;
                    //App.trace("RTF2 " + rtf.anchoredPosition.x + "|" + rtf.anchoredPosition.y + "PIVOT " + rtf.pivot.x + "|" + rtf.pivot.y);

                    flyImg.gameObject.SetActive(true);
                    caiList.Add(new XuRTF(rtf, xurtf0.SlotId));
                    //xuRtfList[win[i]] = tmpList;


                    DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, new Vector2(rtf2.anchoredPosition.x + UnityEngine.Random.RandomRange(-randX, randX), rtf2.anchoredPosition.y + UnityEngine.Random.RandomRange(-randY, randY)), .5f).SetEase(Ease.OutBack).OnComplete(()=> {
                        rtf.DOScale(.6f, .25f);
                    });
                }
            }


        }
        #endregion
        yield return new WaitForSeconds(1f);
        #region //=----Bay chip từ cửa win về ng chơi

        for (int i = 0; i < win.Count; i++)
        {
            if(xuRtfList.ContainsKey(win[i]))
            foreach (XuRTF mxurtf in xuRtfList[win[i]])
            {
                if (mxurtf == null)
                    continue;
                RectTransform rtf =mxurtf.Rtf;
                //App.trace("BAY: " + mxurtf.SlotId + "|");
                DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, XuClonepos[mxurtf.SlotId], 1f).OnComplete(() =>
                {

                    rtf.DOScale(Vector2.one / 2, .25f).OnComplete(() =>
                    {
                        //xuRtfList[win[i]].Remove(mxurtf);
                        Destroy(rtf.gameObject);
                    });
                });
            }
        }
        for(int i = 0; i < caiList.Count; i++)
        {
            RectTransform rtf = caiList[i].Rtf;
            DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, XuClonepos[caiList[i].SlotId], .5f).OnComplete(() =>
            {
                rtf.DOScale(Vector2.one / 2, .25f).OnComplete(() =>
                {
                    Destroy(rtf.gameObject);
                });
            });
        }
        #endregion
        //yield break;
        #endregion



    }

    public class State
    {
        private int id, mode;
        private string code;
        public List<StateCommand> commands;
        public Dictionary<int, List<StateCommand>> commandsByPosition;

        public int Id
        {
            get
            {
                return id;
            }

            set
            {
                id = value;
            }
        }

        public int Mode
        {
            get
            {
                return mode;
            }

            set
            {
                mode = value;
            }
        }

        public string Code
        {
            get
            {
                return code;
            }

            set
            {
                code = value;
            }
        }

        public State(int id, string code, int mode)
        {
            this.Id = id;
            this.Code = code;
            this.Mode = mode;
            commands = new List<StateCommand>();
            commandsByPosition = new Dictionary<int, List<StateCommand>>();
        }
    }

    public class StateCommand
    {
        private int position;
        private string code, name;
        public bool fillBoardState, takeConfirmation, waitResult;
        public Action<int> action;

        public int Position
        {
            get
            {
                return position;
            }

            set
            {
                position = value;
            }
        }

        public string Code
        {
            get
            {
                return code;
            }

            set
            {
                code = value;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
            }
        }

        public StateCommand(int position, string code, string name)
        {
            this.Position = position;
            this.Code = code;
            this.Name = name;
        }
        public void printStateCommand()
        {
            App.trace("STATE: pos = " + Position + "|code =" + Code + "|name = " + Name);
        }
    }

    private void enterState(State state, InBoundMessage res = null)
    {
        if (stateButtonByCommandCode != null)
            stateButtonByCommandCode.Clear();
        else
            stateButtonByCommandCode = new Dictionary<string, Button>();

        if (stateButtonByPosition != null)
            stateButtonByPosition.Clear();
        else
            stateButtonByPosition = new Dictionary<int, Button>();
        this.state = state;

        foreach(int position in state.commandsByPosition.Keys)
        {
            List<StateCommand> commands = state.commandsByPosition[position];
            stateButtonByPosition.Add(position, createStateButton(commands));
        }
        /*
        for(position in state.commandsByPosition.keys())
        {
            var commands = state.commandsByPosition[position];
            stateButtonByPosition.set(position, createStateButton(commands));

        }
         * */



    }

    private bool isSaking = false;
    private void enterState2(State state, InBoundMessage res = null)
    {
        foreach (Button btn in stateBtns)
        {
            btn.gameObject.SetActive(false);
        }

        if (mySlotId == currOwner)
        {

            for (int i = 4; i < 7; i++)
            {

                stateBtns[i].gameObject.SetActive(true);
                stateBtns[i].image.color = colorList[2];
                stateBtns[i].interactable = false;
            }

        }
        foreach (Toggle tog in betTogList)
        {
            tog.gameObject.SetActive(false);
        }
        for(int i  = 0; i < 6; i++)
        {
            sellBuyList[i + 7].SetActive(false);
        }


        if (state == null)
            return;
        currentState = state.Code;
        //App.trace("ENTER STATE 2 = " + state.Code);
        for (int i = 0; i < 6; i++)
        {
            xocDiaBtnList[i].interactable = false;
        }

        //Ẩn cược biên góc dưới bên trái
        sellBuyList[13].SetActive(false);

        switch (state.Code)
        {
            case "shake":   //STATE 10 - Xóc đĩa

                //xocDiaTextList[1].text = "Nhà cái xóc đĩa";

                updateNotiText("Nhà cái xóc đĩa");
                batDiafirstXY = diaBatRtf[0].anchoredPosition;

                isSaking = true;
                showCountDown(0, false);
                shake();
                break;
            case "bet": //STATE 11 - Cược
                if (mySlotId == currOwner)   //Là chủ phòng
                {
                    //xocDiaTextList[1].text = "Chờ người chơi đặt cửa";

                    updateNotiText("Chờ người chơi đặt cửa");

                }
                else
                {
                    SoundManager.instance.PlayUISound(SoundFX.CARD_READY);
                    //xocDiaTextList[1].text = "Hãy chọn cửa để đặt";
                    for (int i = 0; i < 6; i++)
                    {
                        xocDiaBtnList[i].interactable = true;
                    }
                    updateNotiText("Hãy chọn cửa để đặt");

                    foreach(Toggle tg in betTogList)    //Hiển thị toggle chọn mức cược
                    {
                        tg.gameObject.SetActive(true);
                    }
                    for(int i = 0; i < 3; i++)    //Hiển thị các nút chức năng :hủy cược, đặt lại, ...
                    {
                        stateBtns[i].gameObject.SetActive(true);
                    }
                    for(int i = 1; i < 7; i++)  //Hiển thị các phần cược biên
                    {
                        App.trace("CUOC BIEN NE! slotId = " + i);

                        int own = 3;
                        if (exitsSlotList[i] == true && i != own)
                        {
                            int tmp = i - 1;
                            slider[tmp].minValue = tableBet;
                            slider[tmp].maxValue = 20 * tableBet;
                            slider[tmp].value = tableBet;
                            xocDiaTextList[tmp + 24].text = App.formatMoneyAuto(tableBet);
                            slider[tmp].onValueChanged.RemoveAllListeners();
                            slider[tmp].onValueChanged.AddListener((float f) =>
                            {
                                int a = (int)f;
                                try
                                {
                                    xocDiaTextList[tmp + 24].text = App.formatMoneyAuto(f);
                                    betToSideBet = (short)a;
                                }
                                catch
                                {
                                    App.trace("LỖI i = " + tmp);
                                }
                            });
                            sellBuyList[tmp + 7].SetActive(true);
                        }

                    }

                }

                //showCountDown(currTimeOut);
                break;
            case "sellGate":    //STATE 12 - Bán cửa

                if (mySlotId == currOwner)
                {
                    //xocDiaTextList[1].text = "Nhà cái bắt đầu cân cửa";
                    updateNotiText("Nhà cái bắt đầu cân cửa");
                    for (int i = 4; i < 7; i++)
                    {
                        stateBtns[i].image.color = colorList[3];
                        stateBtns[i].interactable = true;
                    }
                }
                else
                {
                    //xocDiaTextList[1].text = "Thời gian cân cửa";
                    updateNotiText("Thời gian cân cửa");
                }
                //showCountDown(currTimeOut);
                break;

            case "buyGate": //STATE 13 - Người chơi mua cửa
                if(currOwner!= mySlotId)
                {
                    stateBtns[3].gameObject.SetActive(true);
                }
                break;
        }
    }

    private Vector2 batDiafirstXY;
    private void shake(bool isStart = true,int dem = 0)
    {

        //Vector2 tmp = diaBatRtf.anchoredPosition;
        //App.trace(dem.ToString());
        if(dem > 59)
        {
            DOTween.To(() => diaBatRtf[0].anchoredPosition, x => diaBatRtf[0].anchoredPosition = x, batDiafirstXY, .05f).SetEase(Ease.InOutBack).OnComplete(() => { diaBatRtf[0].DOScale(1f, 1); isSaking = false; });

            return;
        }
        if (isStart == false)
        {
            DOTween.To(() => diaBatRtf[0].anchoredPosition, x => diaBatRtf[0].anchoredPosition = x, batDiafirstXY + getRandomRange(), .025f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.InOutBack).OnComplete(() => {
                shake(false,dem + 1);
            });
            return;
        }

        diaBatRtf[0].DOScale(1.5f, 1).OnComplete(() =>
        {
            shake(false,dem + 1);
            StartCoroutine(shakeSound());
        });

    }

    private IEnumerator shakeSound(bool upBat = false)
    {
        if (upBat)
        {
            yield return new WaitForSeconds(1.5f);
            SoundManager.instance.PlayUISound(SoundFX.CARD_MO_BAT);
        }
        else
        {
            SoundManager.instance.PlayUISound(SoundFX.CARD_SHAKE);
            yield return new WaitForSeconds(2f);
            SoundManager.instance.PlayUISound(SoundFX.CARD_SHAKE);
        }

    }
    private Vector2 getRandomRange()
    {
        int x = UnityEngine.Random.Range(-40, 40);
        int y = UnityEngine.Random.Range(-40, 40);
        return new Vector2(x,y);
    }
    private Button createStateButton(List<StateCommand> commands)
    {
        /*
        if(commands.Count == 1) //Center btns
        {

        }else
        {   //Bot btns
            string t = "";
            foreach (StateCommand stc in commands)
            {

                t += "[" + stc.Code + "|name = " + stc.Name + "|pos = " + stc.Position + "]|";
            }
            App.trace(t);
        }*/

        Button btn = null;
        return btn;
    }

    private float tableBet = 0;
    private short[] lanMucCuoc = { 1, 5, 10, 20 };
    private bool isBeted = false;   //Đã đặt cược ván hiện tại chưa
    private Dictionary<int, int> myBetedList = new Dictionary<int, int>();  //Cửa đặt - số chip đặt
    private Dictionary<int, int> myBetedListToSave = new Dictionary<int, int>();
    public void doBet(int mGateId)
    {

        var req = new OutBounMessage("BET");
        req.addHead();
        req.writeShort(lanMucCuoc[preSelectBetId]); //LẦN MỨC CƯỢC
        req.writeByte(mGateId);  //CỬA
        isBeted = true;
        App.ws.send(req.getReq(), null,true,0);

        if (myBetedList.ContainsKey(mGateId))
        {
            int a = myBetedList[mGateId];
            myBetedList[mGateId] = a + lanMucCuoc[preSelectBetId];
        }
        else
        {
            myBetedList.Add(mGateId, lanMucCuoc[preSelectBetId]);
        }
    }
    public void doBet(int mGateId, short lanMucCuoc)
    {
        Debug.Log("Cửa " + mGateId + " lần " + lanMucCuoc);
        //App.trace("Cửa " + mGateId + " lần " + lanMucCuoc);
        var req = new OutBounMessage("BET");
        req.addHead();
        req.writeShort(lanMucCuoc); //LẦN MỨC CƯỢC
        req.writeByte(mGateId);  //CỬA
        isBeted = true;
        App.ws.send(req.getReq(), null,true,0);

        if (myBetedList.ContainsKey(mGateId))
        {
            int a = myBetedList[mGateId];
            if(a + lanMucCuoc < 21)
            {
                myBetedList[mGateId] = a + lanMucCuoc;
            }

        }
        else
        {
            myBetedList.Add(mGateId, lanMucCuoc);
        }
    }
    private int preSelectBetId = 0;
    public void changeBet(int selectBetId)
    {
        if(preSelectBetId != selectBetId && betTogList[selectBetId].isOn)
        {
            preSelectBetId = selectBetId;
        }
    }

    private List<int> groupChip(int v){
        //vi du 190
        int[] listTemp = { 20 * (int)tableBet, 10 * (int)tableBet, 5 * (int)tableBet, (int)tableBet };
        var x = v;
        var listChipResult = new List<int>();

        if (x == listTemp[0])
        { //dung = 20 lan cuoc
            listChipResult.Add(listTemp[0]);
        }
        else
        { //truong hop < 20 lan cuoc
            if (x > listTemp[1]) //so sanh voi 10 lan cuoc
            {
                listChipResult.Add(listTemp[1]);// lon hon 10 lan cuoc thi push 10 lan cuoc vao
                var first = x % listTemp[1]; //chia lay so du
                if (first > listTemp[2]) //lon hon 5 lan cuoc
                {
                    listChipResult.Add(listTemp[2]); //push 5 lan cuoc
                    var second = first % listTemp[2]; //chia lay phan du voi 5 lan cuoc
                    if (second > listTemp[3]) //lon hon 3 lan cuoc thi chac chan se chia het cho 1 lan cuoc
                    {
                        var count = int.Parse((second / listTemp[3]) + "");
                        for (int i = 0; i < count; i++)
                        {
                            listChipResult.Add(listTemp[3]);
                        }
                    }
                }
                else if (first == listTemp[2])
                { //= 5 lan cuoc
                    listChipResult.Add(listTemp[2]);
                }
                else
                { //nho hon 5 lan cuoc
                    var count = int.Parse((first / listTemp[3]) + "");
                    for (int i = 0; i < count; i++)
                    {
                        listChipResult.Add(listTemp[3]);
                    }
                }

            }
            else if (x == listTemp[1]) //dung bang 10 lan cuoc
            {
                listChipResult.Add(listTemp[1]);
            }
            else
            { //nho hon 10 lan cuoc thi so sanh voi 5 lan cuoc
                if (x > listTemp[2]) //lon hon 5 lan cuoc
                {
                    listChipResult.Add(listTemp[2]);
                    var third = x % listTemp[2]; //chia lay phan du
                    var count = int.Parse((third / listTemp[3]) + "");
                    for (int i = 0; i < count; i++)
                    {
                        listChipResult.Add(listTemp[3]);
                    }
                }
                else if (x == listTemp[2]) //=5 lan
                {
                    listChipResult.Add(listTemp[2]);
                }
                else
                { //nho hon 5 lan thi chac chan se chia het cho 1 lan
                    var count = int.Parse((x / listTemp[3]) + "");
                    for (int i = 0; i < count; i++)
                    {
                        listChipResult.Add(listTemp[3]);
                    }
                }
            }
        }

        return listChipResult;
    }

    private void updateNotiText(string t)
    {
        xocDiaTextList[1].text = t;
        if (xocDiaTextList[1].transform.parent.gameObject.activeSelf == false)
        {
            xocDiaTextList[1].transform.parent.gameObject.SetActive(true);
        }
    }

    public class XuRTF
    {
        private RectTransform rtf;
        private int slotId;
        public XuRTF(RectTransform rtf, int slotID)
        {
            this.Rtf = rtf;
            this.SlotId = slotID;
        }

        public RectTransform Rtf
        {
            get
            {
                return rtf;
            }

            set
            {
                rtf = value;
            }
        }

        public int SlotId
        {
            get
            {
                return slotId;
            }

            set
            {
                slotId = value;
            }
        }
    }

    private int unbetCount = 0, rebetCount = 0;
    private int rebetx2Count = 0;
    public void doAction(string actionToDo)
    {
        switch (actionToDo)
        {
            case "rebet":
                //Nếu đặt rồi thì hủy đi và đặt lại ván trước
                //Nếu chưa đặt thì đặt lại ván trước
                unbetCount = 0;
                Debug.Log("DoAction Rebet");
                if (rebetCount > 0)
                {
                    return;
                }
                App.trace(myBetedListToSave.Count + " is loaded");

                if (myBetedListToSave.Count == 0)
                {
                    break;
                }

                if(isBeted == true)
                {
                    var req_UNBET1 = new OutBounMessage("UNBET");
                    req_UNBET1.addHead();
                    App.ws.send(req_UNBET1.getReq(), delegate (InBoundMessage res_UNBET) {
                    });
                    isBeted = false;
                }
                foreach(int key in myBetedListToSave.Keys.ToList())
                {
                    doBet(key,(short)myBetedListToSave[key]);
                }
                rebetCount++;

                break;

            case "unbet":
                Debug.Log("DoAction Unbet");
                //rebetCount = 0;
                //rebetx2Count = 0;
                if (unbetCount > 2)
                {
                    return;
                }
                for (int i = 0; i < 5; i++)
                {
                    xocDiaTextList[i + 11].text = "";
                }
                myBetedList.Clear();
                var req_UNBET = new OutBounMessage("UNBET");
                req_UNBET.addHead();
                App.ws.send(req_UNBET.getReq(), delegate(InBoundMessage res_UNBET){
                    unbetCount++;
                });
                break;
            case "betx2":
                unbetCount = 0;
                if (rebetx2Count > 0)
                {
                    return;
                }
                Debug.Log("DoAction X2");
                /*
                foreach(int key in myBetedListToSave.Keys.ToList())
                {
                    myBetedListToSave[key] = myBetedListToSave[key] * 2;
                }

                doAction("rebet");
                foreach (int key in myBetedListToSave.Keys.ToList())
                {
                    myBetedListToSave[key] = myBetedListToSave[key] / 2;
                }*/
                /*
                int totalValueToBet = 0;
                foreach (int val in myBetedList.Values.ToList())
                {
                    totalValueToBet += val;
                }
                */
                foreach (int key in myBetedList.Keys.ToList())
                {
                    doBet(key, (short)myBetedList[key]);
                }
                rebetx2Count++;
                break;
            case "buyGate":
                var req = new OutBounMessage("BUY_GATE");
                req.addHead();
                req.writeByte(sellGateId);
                App.ws.send(req.getReq(), delegate (InBoundMessage res)
                {
                });
                break;
        }
    }

    private short betToSideBet = -1;
    public void sendSideBet(int sl)
    {

        var req = new OutBounMessage("SIDE_BET");
        req.addHead();
        req.writeByte(1);   //đề nghị cược
        req.writeByte(detecSvrBySlotId(sl > 9 ? sl % 10 : sl));   //slotId
        if (betToSideBet < tableBet)
            betToSideBet = (short)tableBet;
        betToSideBet = (short)(betToSideBet / tableBet);
        if (betToSideBet < 0)
            betToSideBet = 1;
        req.writeShort(betToSideBet);  //id mức cược
        req.writeByte(sl > 9 ? 1 : 0);   //cửa cược 0: chẵn|1: chẵn

        App.ws.send(req.getReq(), delegate (InBoundMessage res)
        {
            xocDiaTextList[18 + (sl % 10) - 1].text = App.listKeyText["GAME_REQUEST_SENT"];//"Đã gửi yêu cầu.";
            sellBuyList[(sl % 10) - 1 + 14].SetActive(true);
            sellBuyList[(sl > 9 ? sl % 10 : sl) + 7 - 1].SetActive(false);
            /*  Tắt cược biên với ng chơi khác
            for (int i = 0; i < 6; i++)
            {
                sellBuyList[i + 7].SetActive(false);
            }*/
        });
    }

    private void sendSideBet(SideBetInfo mSideBetInfo)
    {
        var req = new OutBounMessage("SIDE_BET");
        req.addHead();
        req.writeByte(mSideBetInfo.Type);   //2: chấp nhận cược
        req.writeByte(mSideBetInfo.SlotId);   //slotId
        req.writeShort(mSideBetInfo.BetId);  //id mức cược
        req.writeByte(mSideBetInfo.GateId);   //cửa cược 0: chẵn|1: chẵn
        int slotId = mSideBetInfo.SlotId;
        slotId = getSlotIdBySvrId(slotId);
        App.ws.send(req.getReq(), delegate (InBoundMessage res)
        {
            string t = "Cược " + (mSideBetInfo.GateId == 0 ? "CHẴN\n" : "LẺ\n") + mSideBetInfo.BetId * tableBet + " Gold";
            xocDiaTextList[18 + slotId - 1].text = t;
            sellBuyList[slotId + 14 - 1].SetActive(true);

            for (int i = 0; i < 6; i++)
            {
                sellBuyList[i + 7].SetActive(false);
            }
        });

    }

    private class SideBetInfo
    {
        private int type, slotId, gateId;
        private short betId;

        public int Type
        {
            get
            {
                return type;
            }

            set
            {
                type = value;
            }
        }

        public int SlotId
        {
            get
            {
                return slotId;
            }

            set
            {
                slotId = value;
            }
        }

        public int GateId
        {
            get
            {
                return gateId;
            }

            set
            {
                gateId = value;
            }
        }

        public short BetId
        {
            get
            {
                return betId;
            }

            set
            {
                betId = value;
            }
        }

        public SideBetInfo(int type, int slotId, short betId, int gateId)
        {
            this.Type = type;
            this.SlotId = slotId;
            this.BetId = betId;
            this.GateId = gateId;
        }
    }
    private SideBetInfo mSideBetInfo;
    public void acceptSideBet(bool isOk)
    {
        sellBuyList[13].SetActive(false);
        if (!isOk)
        {
            //sellBuyList[7-12]
            sellBuyList[getSlotIdBySvrId(mSideBetInfo.SlotId) + 7 - 1].SetActive(false);
            return;
        }

        sendSideBet(mSideBetInfo);
    }

    public void doSellGate(int gateToSell)
    {
        var reqSellGate = new OutBounMessage("SELL_GATE");
        reqSellGate.addHead();
        reqSellGate.writeByte(gateToSell);
        App.ws.send(reqSellGate.getReq(), delegate (InBoundMessage res)
        {

        });
    }

    #region //CHAT
    [Header("CHAT")]
    public GameObject[] chatPanels;
    public Image[] chatImoIco;
    public void showChatBox()
    {
        LoadingControl.instance.showChatBox(LoadingControl.CHANNEL_TABLE);
    }

    public void showChatPanels(string sender, string content, string emo, Sprite emoSprite = null)
    {
        foreach (Player pl in playerList.Values.ToList())
        {
            if (pl.NickName == sender && pl.SlotId > -1)
            {
                if (emoSprite == null)
                {
                    chatPanels[pl.SlotId].GetComponentInChildren<Text>().text = content;
                    if (!chatPanels[pl.SlotId].activeSelf)
                        StartCoroutine(_showChatPanels(chatPanels[pl.SlotId]));
                    return;
                }
                App.trace("emo = " + emo);
                chatImoIco[pl.SlotId].sprite = emoSprite;
                if (!chatImoIco[pl.SlotId].gameObject.activeSelf)
                {
                    chatImoIco[pl.SlotId].gameObject.SetActive(true);
                    chatImoIco[pl.SlotId].transform.DOScale(1.2f, 4f).SetEase(Ease.OutBounce).OnComplete(() => {
                        chatImoIco[pl.SlotId].gameObject.SetActive(false);
                        chatImoIco[pl.SlotId].transform.localScale = Vector3.one;
                    });
                    return;
                }
                chatImoIco[pl.SlotId].transform.DORestart();
                return;
            }
        }
    }
    IEnumerator _showChatPanels(GameObject goj)
    {
        goj.SetActive(true);
        yield return new WaitForSeconds(3);
        goj.SetActive(false);
        yield break;
    }
    #endregion

    #region //ĐĂNG KÝ RỜI BÀN
    private bool isPlaying = false, isKicked = false;
    public bool regQuit = false;
    private string statusKick = "";
    public GameObject notiText;
    public GameObject btnBackImg;
    public void showNoti()
    {
        if (!isPlaying)
        {
            LoadingControl.instance.delCoins();
            backToTableList();
            return;
        }
        this.regQuit = !regQuit;
        notiText.GetComponentInChildren<Text>().text = !regQuit ? "Bạn đã hủy đăng ký rời bàn." : "Bạn đã đăng ký rời bàn.";
        btnBackImg.transform.localScale = new Vector2(regQuit ? -1 : 1, 1);
        btnBackImg.GetComponent<RectTransform>().anchoredPosition = new Vector2(regQuit ? 115f : 7.5f, -7.5f);
        notiText.SetActive(true);
        StartCoroutine(_showNoti());
    }
    IEnumerator _showNoti()
    {

        yield return new WaitForSeconds(2f);
        notiText.SetActive(false);
        yield break;
    }

    public void backToTableList()
    {
        if (!isPlaying)
        {
            DOTween.PauseAll();
            //run = false;
            delAllHandle();
            if (isKicked ==false)
            {
                EnterParentPlace(delegate() {
                    if (LoadingControl.instance.chatBox.activeSelf)
                    {
                        LoadingControl.instance.chatBox.SetActive(true);

                    }
                    StartCoroutine(openTable());
                });
            }
            else
            {
                if (LoadingControl.instance.chatBox.activeSelf)
                {
                    LoadingControl.instance.chatBox.SetActive(true);

                }
                StartCoroutine(openTable());
            }
        }
    }

    IEnumerator openTable()
    {
        /*========THAY=========
        CPlayer.preScene = isKicked ? "XocDiaK" : "XocDia";

        yield return new WaitForSeconds(0.5f);

        SceneManager.LoadScene("TableList");

        //LobbyControll.instance.LobbyScene.SetActive(true);
        slideSceneAnim.Play("TableAnimation");
        //yield return async;

        Destroy(gameObject, 1f);

        yield return new WaitForSeconds(0.5f);
        LoadingControl.instance.loadingScene.SetActive(false);
        if (isKicked == true)
            App.showErr(statusKick);
        //yield break;
        ========THAY=====*/
        CPlayer.preScene = isKicked ? "XocDiaK" : "XocDia";
        //LoadingControl.instance.blackkkkkk.SetActive(true);
        LoadingUIPanel.Show();
        SceneManager.LoadScene("TableList");
        yield return new WaitForSeconds(0.05f);
        if (isKicked == true)
            App.showErr(statusKick);
    }

    private void EnterParentPlace(Action callback)
    {
        CPlayer.clientCurrentMode = 0; // modeview
        CPlayer.clientTargetMode = 0;//mode view
        LoadingUIPanel.Show();
        var req = new OutBounMessage("ENTER_PARENT_PLACE");
        req.addHead();
        req.writeString("");
        req.writeByte(CPlayer.clientCurrentMode);
        App.ws.send(req.getReq(), delegate (InBoundMessage res)
        {
            if(callback!= null)
            {
                callback();
            }
        });
    }
    private string[] handelerCommand = {"SET_PLAYER_POINT", "SET_PLAYER_STATUS", "KICK_PLAYER", "ENTER_STATE", "SET_TURN", "SLOT_IN_TABLE_CHANGED",
    "OWNER_CHANGED", "START_MATCH", "GAMEOVER", "SET_PLAYER_ATTR", "MOVE"};
    private void delAllHandle()
    {

        foreach (string t in handelerCommand)
        {
            //App.trace(t);
            var req = new OutBounMessage(t);
            req.addHead();
            App.ws.delHandler(req.getReq());
        }
    }
    #endregion

    #region PLAYER INFO

    public void showPlayerInfo(int slotIdToShow)
    {

        if (exitsSlotList[slotIdToShow] == false)
        {
            LoadingControl.instance.sendInvite();
            return;
        }
        Player pl = null;
        string typeShowInfo = "";
        if (slotIdToShow == 0)
        {
            LoadingControl.instance.showPlayerInfo(CPlayer.nickName, (long)CPlayer.chipBalance, (long)CPlayer.manBalance, CPlayer.id, true, avatarsList[slotIdToShow].overrideSprite, "me");
            //ProfileController.instance.Show();
            return;
        }
        if (mySlotId == currOwner && slotIdToShow != 0)
            typeShowInfo = "kick";
        foreach (Player mpl in playerList.Values.ToList())
        {
            if (mpl.SlotId == slotIdToShow)
            {
                pl = mpl;
                //LoadingControl.instance.showPlayerInfo(pl.NickName, pl.ChipBalance, pl.StarBalance, pl.PlayerId, playerAvatarList[slotIdToShow].sprite,typeShowInfo);
                var req_info = new OutBounMessage("PLAYER_PROFILE");
                //Debug.Log("WRITE LONG = " + CPlayer.id);
                //App.trace("PPLAYER ID = " + CPlayer.id);
                req_info.addHead();
                req_info.writeLong(pl.PlayerId);
                req_info.writeByte(0x0f);
                req_info.writeAcii("");

                App.ws.send(req_info.getReq(), delegate (InBoundMessage res) {
                    var nickName = res.readAscii();
                    var fullName = res.readString();
                    var avatar = res.readAscii();
                    var isMale = res.readByte() == 1;
                    //App.trace("isMale = " + isMale);
                    var dateOfBirth = res.readAscii();
                    var message = res.readString();
                    var chipBalance = res.readLong();
                    var starBalance = res.readLong();
                    var phone = res.readAscii();
                    var email = res.readAscii();
                    var address = res.readAscii();
                    var cmnd = res.readAscii();
                    var isLocalPlayer = res.readByte() == 1;
                    var clubCode = res.readAscii();
                    var clubName = res.readString();
                    var path = res.readAscii();
                    var pathName = res.readString();
                    var clubId = res.readLong();
                    var attSize = res.readByte();
                    for (int i = 0; i < attSize; i++)
                    {
                        var code = res.readAscii();
                        var value = res.readAscii();
                    }
                    var itemSize = res.readByte();
                    for (int i = 0; i < itemSize; i++)
                    {
                        string itemImage = res.readAscii();
                        string itemName = res.readString();
                        string itemDesc = res.readString();
                        string itemPosition = res.readAscii();
                        string itemTour = res.readString();
                        string itemCreated = res.readAscii();
                    }

                    var count = res.readByte();
                    //App.trace("Số bản ghi " + count);
                    for (int i = 0; i < count; i++)
                    {
                        string gameId = res.readAscii();
                        string zoneName = res.readString();
                        string levelName = res.readString();
                        int win = res.readInt();
                        int lose = res.readInt();
                        int draw = res.readInt();
                        int level = res.readShort();
                        long score = res.readLong();
                        long previousScore = res.readLong();
                        long nextScore = res.readLong();

                    }

                    var isCPlayerFriend = res.readByte() == 1; // 0 : not friend 1: friend
                    LoadingControl.instance.showPlayerInfo(pl.NickName, pl.ChipBalance, pl.StarBalance, pl.PlayerId, isCPlayerFriend, avatarsList[slotIdToShow].overrideSprite, typeShowInfo);
                });
                return;
            }
        }

    }
    #endregion

    private void updateTableSlot(bool isUpdate = false)
    {
        for(int slotId = 0; slotId < 7; slotId++)
        {
            exitsSlotList[slotId] = false;
            //avatarsList[slotId].overrideSprite = spritesList[0];
            infoObjectList[slotId].SetActive(false);
            chipList[slotId] = 0;
            //balanceTextList[slotId].text = "";
            //nickNameTextList[slotId].text = "";
            ownerList[slotId].SetActive(false);
        }

        if (isUpdate == true)
            return;

        foreach(Player player in playerList.Values.ToList()) {
            int slotId = player.SlotId;
            App.trace("SET PLAYER SLOT " + slotId);
            setInfo(player, avatarsList[slotId], infoObjectList[slotId], balanceTextList[slotId], nickNameTextList[slotId], ownerList[slotId]);
            exitsSlotList[slotId] = true;
        }
    }

    private bool confirmShowing = false;
    public GameObject panelConfirmDialog;
    public void Close_panelConfirmDialog()
    {
        panelConfirmDialog.SetActive(false);
    }
    public void Ok_panelConfirmDialog()
    {
        panelConfirmDialog.SetActive(false);
        confirmShowing = false;
        showNoti();
    }
    public void changeClientMode(bool isShow)
    {
        if (isShow == false)
        {
            if(confirmShowing == true)
            {
                confirmShowing = false;
                //LoadingControl.instance.closeConfirmDialog();
            }
            return;
        }
        confirmShowing = true;
        panelConfirmDialog.SetActive(true);
        // LoadingControl.instance.btnDoConfirm.onClick.RemoveAllListeners();
        /* LoadingControl.instance.btnDoConfirm.onClick.AddListener(() => {
             //App.trace("OK ĐỔI " + App.formatMoney(price.ToString()) +" MAN thành " + App.formatMoney(amount.ToString()) +" ChIP");
             LoadingControl.instance.closeConfirmDialog();
             confirmShowing = false;
             showNoti();
         });*/


        //LoadingControl.instance.confirmText.text = "Bỏ làm cái đồng nghĩa với việc rời khỏi bàn chơi.\nBạn có chắc chắn bỏ cái không?";
      //  LoadingControl.instance.blackPanel.SetActive(true);
       // LoadingControl.instance.confirmDialogAnim.Play("DialogAnim");
    }

    public void openSettingPanel()
    {
        LoadingControl.instance.openSettingPanel();
    }
}

