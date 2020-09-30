using DG.Tweening;
using Core.Server.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MauBinhController : MonoBehaviour {

    public static MauBinhController instance;

    public Animator slideSceneAnim;
    /// <summary>
    /// 0-3: Info Object List|4-7: owner list|8-10: Blur ava|11 : Help|12:14: Lớp trans các quân bài của mình
    /// |15: trans
    /// </summary>
    public GameObject[] mauBinhGojList;

    /// <summary>
    /// 0: Card của mình (để clone)|1: -12: Bài của mình|13: Card loại 2 (của player khác để clone)|14: lớp phủ của mình|15-18: avatar|19-22: time leap
    /// </summary>
    public Image[] mauBinhImgs;
    /// <summary>
    /// Mặt các quân bài 0: back|1-52: bài
    /// </summary>
    public Sprite[] faces;
    /// <summary>
    /// 0-2: Tên bộ các chi của mình|3-6: TÊN chi|7-10: Kết quả so|11: So chi thứ mấy|12: Tổng kết|13-16: balance text list|17-20: nick name text|21-23: Sort done?
    /// |24: My Sort Done|25-28: Earn Txt|29: Count down|30: Table Name|31: Text thắng trắng
    /// </summary>
    public Text[] mbTextList;
    /// <summary>
    /// 0: Xếp xong|1: Xếp lại|2: Gợi ý
    /// </summary>
    public Button[] stateBtns;

    public Sprite addPlayerIcon;

    [HideInInspector]
    public bool isDragging = false;

    /// <summary>
    /// 0: Vàng|1: Trắng|2: Xanh
    /// </summary>
    public Font[] mbFont;

    public Sprite[] tableBackground;

    private List<CardPrepare> cardPrepareList = new List<CardPrepare>();

    /// <summary>
    /// List bài từ sv Dic[slotId, List card ids]|SlotId is svId
    /// </summary>
    private Dictionary<int, List<int>> svIds = new Dictionary<int, List<int>>();
    private bool showTotal = false, divideCardDone = false;
    private List<GameRS> lsGameRs = new List<GameRS>();
    private bool isWhiteWin = false;

    [Header("Vua bai dep")]
    private bool isBeautiful = false; //co dc bai dep hay ko

    void Awake()
    {
        Input.multiTouchEnabled = false;
        Application.runInBackground = true;
        getInstance();
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

    // Use this for initialization
    void Start() {
        //divideCards();

        CPlayer.baiDep = "MAUBINH.BAIDEP";

        //set background cua ban choi theo tung muc cuoc
        int bgIndex = UnityEngine.Random.Range(0,tableBackground.Length-1);

        //if (CPlayer.betAmtId > 3 && CPlayer.betAmtId < 8)
        //{
        //    bgIndex = 1;
        //}
        //else if (CPlayer.betAmtId >= 8)
        //{
        //    bgIndex = 2;
        //}

        mauBinhImgs[23].sprite = tableBackground[bgIndex];

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
                        string tempString = App.listKeyText["MAUBINH_NAME"].ToUpper();
                        mbTextList[30].text = tempString + " - " + CPlayer.betAmtOfTableToGo + " " + App.listKeyText["CURRENCY"]; //"MẬU BINH - " + CPlayer.betAmtOfTableToGo + " Gold";
                        break;
                    }


                }



                //App.trace("GET BET AMT DONE! BETAMT COUNT = " + count);
            });
        }
        else
        {
            string tempString = App.listKeyText["MAUBINH_NAME"].ToUpper();
            mbTextList[30].text = tempString + " - " + App.formatMoney(CPlayer.betAmtOfTableToGo) + " " + App.listKeyText["CURRENCY"]; //"MẬU BINH - " + App.formatMoney(CPlayer.betAmtOfTableToGo) + " Gold";
        }

        registerHandler();
        getTableDataEx();
    }
    /// <summary>
    /// List card lấy từ sv [0,1;0,2;0,3]
    /// </summary>
    private List<CardCompare> cardCompareList = new List<CardCompare>();
    private List<ThreeBandCompare> threeBandCompare = new List<ThreeBandCompare>();
    private List<CompareData> oddLsCompareData = new List<CompareData>(), evenLsCompareData = new List<CompareData>();
    private void registerHandler()
    {
        #region //HANDLER [KICK]
        var req_KICK_PLAYER = new OutBounMessage("KICK_PLAYER");    //KÍCH NG CHƠI
        req_KICK_PLAYER.addHead();
        //req_getTableChange.print();
        handelerCommand.Add("KICK_PLAYER");
        App.ws.sendHandler(req_KICK_PLAYER.getReq(), delegate (InBoundMessage res)
        {
            App.trace("RECV [KICK_PLAYER]");
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
                //App.showErr(content);
                backToTableList();

            }

        });
        #endregion


        var req_START_MATCH = new OutBounMessage("START_MATCH");
        req_START_MATCH.addHead();
        handelerCommand.Add("START_MATCH");
        App.ws.sendHandler(req_START_MATCH.getReq(), delegate (InBoundMessage res_START_MATCH)
        {
            LoadingControl.instance.delCoins();
            SoundManager.instance.PlayUISound(SoundFX.CARD_START_MATCH);
            isWhiteWin = false;
            submited = false;
            isPlaying = true;
            showTotal = false;
            svIds.Clear();  //Xóa dánh sách bài game trước đó
            myCardIdList.Clear();   //XÓA DANH SÁCH BÀI CỦA MÌNH
            #region //XÓA LIST BÀI GAME TRƯỚC
            foreach (int key in playerCardList.Keys.ToList())
            {
                List<PlayerCard> ls = playerCardList[key];
                for (int i = 0; i < ls.Count; i++)
                {
                    DestroyObject(ls[i].Rtf.gameObject);
                }
            }
            playerCardList.Clear();
            mauBinhImgs[1].transform.parent.parent.localScale = Vector2.one;
            #endregion

            for(int i = 0; i < 4; i++)  //Ẩn tiền kiếm được ở game trước
            {
                mbTextList[25 + i].gameObject.SetActive(false);
                mbTextList[7 + i].gameObject.SetActive(false);
                mbTextList[3 + i].transform.parent.gameObject.SetActive(false);
            }

            for(int i = 0; i < 3; i++)  //Ẩn các text xếp xong
            {
                mbTextList[21 + i].gameObject.SetActive(false);
                mauBinhGojList[12 + i].SetActive(false);    //Xóa lớp trans quân bài của mình mình
                mauBinhGojList[i + 8].SetActive(false); //Xóa lớp đen trên ava
            }

            App.trace("RECV [START_MATCH]");

            mbTextList[11].gameObject.SetActive(false); //Ẩn text so chi thứ mấy
            mauBinhImgs[14].gameObject.SetActive(false);    //Ẩn lớp phủ của mình

            loadPlayerMatchPoint(res_START_MATCH);
            loadBoardData(res_START_MATCH, true);
            changePlayersMode(true);
        });

        var req_DROP = new OutBounMessage("DROP");
        req_DROP.addHead();
        handelerCommand.Add("DROP");
        App.ws.sendHandler(req_DROP.getReq(), delegate (InBoundMessage res_DROP)
        {
            App.trace("RECV [DROP]");

            oddLsCompareData.Clear();
            evenLsCompareData.Clear();
            int comparisonCount = res_DROP.readByte();
            for (int i = 0; i < comparisonCount; i++)
            {

                int playerCount = res_DROP.readByte();

                for (int j = 0; j < playerCount; j++)
                {
                    int slotId = res_DROP.readByte();
                    string title = res_DROP.readAscii();
                    /*
                    switch (title)
                    {
                        case "lost3band":
                            title = "SẬP 3 CHI";
                            break;
                        case "win3band":
                            title = "THẮNG 3 CHI";
                            break;
                    }
                    */
                    string subTitle = res_DROP.readAscii();
                    int point = res_DROP.readByte();
                    App.trace("=========" + slotId + "==========");
                    App.trace("slotId = " + slotId + "|title = " + title + "|sub = " + subTitle + "|point = " + point);
                    int bandCount = res_DROP.readByte();
                    List<CardCompare> ccL = new List<CardCompare>();
                    for(int k = 0; k < bandCount; k++)
                    {
                        int bandIndex = res_DROP.readByte() - 1;
                        string bandName = res_DROP.readString();
                        int bandPoint = res_DROP.readByte();
                        point += bandPoint;
                        App.trace("bandIndex = " + bandIndex + "|bandName = " + bandName + "|bandPoint = " + bandPoint);
                        ccL.Add(new CardCompare(bandIndex, bandName, bandPoint, slotId));

                    }
                    CompareData cpDt = new CompareData(slotId, new ThreeBandCompare(slotId, subTitle, title, point), ccL, point, title);
                    if(j % 2 == 0)
                    {
                        evenLsCompareData.Add(cpDt);
                    }
                    else
                    {
                        oddLsCompareData.Add(cpDt);
                    }
                }
            }
            for(int i = 0; i < 4; i++)  //Ẩn hết các nút điều khiển
            {
                stateBtns[i].gameObject.SetActive(false);
            }
            //StartCoroutine(_so());
            if(isPlaying && divideCardDone)
                StartCoroutine(_showCompare());
        });


        var req_SHOW_PLAYER_CARD = new OutBounMessage("SHOW_PLAYER_CARD");
        req_SHOW_PLAYER_CARD.addHead();
        handelerCommand.Add("SHOW_PLAYER_CARD");
        App.ws.sendHandler(req_SHOW_PLAYER_CARD.getReq(), delegate (InBoundMessage res_SHOW_PLAYER_CARD)
        {
            if(isSorted == false)
            {
                sortDone(false);
            }

            for(int i =0; i < 3; i++)   //ẨN CÁC TEXT "XẾP XONG"
            {
                mbTextList[21 + i].gameObject.SetActive(false);
            }

            App.trace("RECV [SHOW_PLAYER_CARD]");


            int slotId = res_SHOW_PLAYER_CARD.readByte();
            List<int> ids = res_SHOW_PLAYER_CARD.readBytes();
            string bandName = res_SHOW_PLAYER_CARD.readAscii();
            int earnValue = res_SHOW_PLAYER_CARD.readShort();

            //CardUtils.svrIdsToIds(ids);

            svIds.Add(slotId, ids);
            if(bandName != "")
            {
                if(slotId == mySlotId)
                {
                    SoundManager.instance.PlayEffectSound(SoundFX.CARD_WIN);
                    //SoundManager.instance.PlayUISound(SoundFX.CARD_WIN);
                    // if (checkWhiteWin(bandName)) {
                    //     isBeautiful = true;
                    //     VuaBaiDepController.instance.canSent = true;

                    //     VuaBaiDepController.instance.PlayCanSentAnim();
                    // }

                }
                else
                {
                    SoundManager.instance.PlayEffectSound(SoundFX.CARD_LOSE);
                    //SoundManager.instance.PlayUISound(SoundFX.CARD_LOSE);
                }
                bandName = getWhiteWinTitle(bandName);
                slotId = getSlotIdBySvrId(slotId);

                //an phan goi y xep bai
                sortDone(false);

                //an state button doi voi truong hop thang trang
                stateBtns[3].gameObject.SetActive(false);
                stateBtns[1].gameObject.SetActive(false);

                mbTextList[3 + slotId].text = bandName;
                mbTextList[3 + slotId].transform.parent.gameObject.SetActive(true);
                isWhiteWin = true;
                mbTextList[31].text = App.listKeyText["WHITE_WIN"].ToUpper(); //"THẮNG TRẮNG";
                mbTextList[31].transform.parent.localScale = .5f * Vector2.one;
                mbTextList[31].transform.parent.gameObject.SetActive(true);
                mbTextList[31].transform.parent.DOScale(1, .5f).SetEase(Ease.OutBounce).SetLoops(2);
            }

        });

        var req_SET_PLAYER_ATTR = new OutBounMessage("SET_PLAYER_ATTR");
        req_SET_PLAYER_ATTR.addHead();
        handelerCommand.Add("SET_PLAYER_ATTR");
        App.ws.sendHandler(req_SET_PLAYER_ATTR.getReq(), delegate (InBoundMessage res_SET_PLAYER_ATTR)
        {
            SoundManager.instance.PlayUISound(SoundFX.CARD_READY);
            App.trace("RECV [SET_PLAYER_ATTR]");
            int slotId = res_SET_PLAYER_ATTR.readByte();
            string icon = res_SET_PLAYER_ATTR.readAscii();
            string content = res_SET_PLAYER_ATTR.readAscii();
            int action = res_SET_PLAYER_ATTR.readByte();

            // App.trace("win = " + checkWhiteWin(icon) + ", slot = " + slotId + ", myslotid = " + mySlotId, "red");

            //la bai dep game mau binh
            // if(checkWhiteWin(icon) && slotId == mySlotId) {
            //     isBeautiful = true;
            //     VuaBaiDepController.instance.canSent = true;
            // }

            if(action == -2 && icon == "done")
            {
                playerAttUpdate(getSlotIdBySvrId(slotId), true);
                return;
            }
            playerAttUpdate(getSlotIdBySvrId(slotId), false);
        });

        #region //HANDLER [SLOT_CHANGE OWNER_CHANGE]
        var req_SLOT_IN_TABLE_CHANGED = new OutBounMessage("SLOT_IN_TABLE_CHANGED");
        req_SLOT_IN_TABLE_CHANGED.addHead();
        //req_getTableChange.print();
        handelerCommand.Add("SLOT_IN_TABLE_CHANGED");
        App.ws.sendHandler(req_SLOT_IN_TABLE_CHANGED.getReq(), delegate (InBoundMessage res)
        {

            var nickName = res.readAscii();
            var slotId = res.readByte();
            var chipBalance = res.readLong();
            var score = res.readLong();
            var level = res.readByte();
            var avatarId = res.readShort();
            var avatar = res.readAscii();
            var isMale = res.readByte() == 1;
            var isOwner = res.readByte() == 1;
            var playerId = res.readLong();
            var starBalance = res.readLong();
            App.trace("SLOT_IN_TABLE_CHANGED slot = " + slotId + "nick = " + nickName);

            if (nickName == CPlayer.nickName)
            {
                mySlotId = slotId;
                //return;
            }
            if (isOwner)
            {
                currOwner = slotId;
            }

            Player player = new Player(detecSlotIdBySvrId(slotId), slotId, playerId, nickName, avatarId, avatar, isMale, chipBalance, starBalance, score, level, isOwner);
            player.PlayMode = false;
            slotId = detecSlotIdBySvrId(slotId);

            if (nickName.Length == 0)    //Có thằng thoát khỏi bàn chơi
            {
                SoundManager.instance.PlayUISound(SoundFX.CARD_EXIT_TABLE);
                playerList.Remove(slotId);
                mauBinhGojList[slotId].SetActive(false);

                mauBinhImgs[slotId + 15].sprite = addPlayerIcon;
                mauBinhImgs[slotId + 15].overrideSprite = addPlayerIcon;
                mauBinhImgs[slotId + 15].material = null;
                //playerAvatarList[slotId].transform.localScale = new Vector3(.5f, .5f, 1);
                exitsSlotList[slotId] = false;
                mauBinhGojList[slotId + 4].SetActive(false);

                if (playerList.Count < 2)
                {
                    if (preCoroutine != null)
                    {
                        StopCoroutine(preCoroutine);
                    }
                    mbTextList[29].gameObject.SetActive(false);

                }


                return;
            }



            if (playerList.ContainsKey(slotId)) //Có thằng thay thế
            {
                playerList[slotId] = player;
                setInfo(player, mauBinhImgs[slotId + 15], mauBinhGojList[slotId], mbTextList[slotId + 13], mbTextList[slotId + 17], mauBinhGojList[slotId + 4]);
                //playerAvatarList[slotId].transform.localScale = new Vector3(1f, 1f, 1);
                exitsSlotList[slotId] = true;
                return;
            }

            //Thêm bình thường
            SoundManager.instance.PlayUISound(SoundFX.CARD_JOIN_TABLE);
            playerList.Add(slotId, player);
            setInfo(player, mauBinhImgs[slotId + 15], mauBinhGojList[slotId], mbTextList[slotId + 13], mbTextList[slotId + 17], mauBinhGojList[slotId + 4]);
            //playerAvatarList[slotId].transform.localScale = new Vector3(1f, 1f, 1);
            exitsSlotList[slotId] = true;

        });

        var req_OWNER_CHANGED = new OutBounMessage("OWNER_CHANGED");
        req_OWNER_CHANGED.addHead();
        handelerCommand.Add("OWNER_CHANGED");
        App.ws.sendHandler(req_OWNER_CHANGED.getReq(), delegate (InBoundMessage res)
        {
            int slotId = res.readByte();
            currOwner = slotId;
            slotId = detecSlotIdBySvrId(slotId);

            for(int i = 0; i < 4; i++)
            {
                if (mauBinhGojList[i + 4].activeSelf)
                {
                    mauBinhGojList[i + 4].SetActive(false);
                    return;
                }
            }
            if (slotId > -1)
            {
                mauBinhGojList[slotId + 4].SetActive(true);
            }
        });
        #endregion

        #region //HANDLER [GAMEOVER]
        var req_GAMEOVER = new OutBounMessage("GAMEOVER");
        req_GAMEOVER.addHead();
        handelerCommand.Add("GAMEOVER");
        App.ws.sendHandler(req_GAMEOVER.getReq(), delegate (InBoundMessage res)
        {
            divideCardDone = false;
            isPlaying = false;
            lsGameRs.Clear();

            //gui bai dep
            // if(isBeautiful) {

            //     //dem nguoc 5s
            //     StartCoroutine(_showTheLe());

            // }

            App.trace("RECV [GAMEOVER]");
            int count = res.readByte();
            for (int i = 0; i < count; i++)
            {
                int slotId = res.readByte();
                int grade = res.readByte();
                long earnValue = res.readLong();
                //slotId = getSlotIdBySvrId(slotId);
                //App.trace("SLOT " + slotId + "|GRADE: " + grade + "|EARN: " + earnValue);
                string title = "";
                switch (grade)
                {
                    case 1:
                        title = App.listKeyText["BOARD_FIRST"];//"NHẤT";
                        break;
                    case 2:
                        title = App.listKeyText["BOARD_SECOND"];//"NHÌ";
                        break;
                    case 3:
                        title = App.listKeyText["BOARD_THIRD"];//"BA";
                        break;
                    case 4:
                        title = App.listKeyText["BOARD_LAST"];//"BÉT";
                        break;

                }
                lsGameRs.Add(new GameRS(slotId, title, earnValue));
            }
            string matchResult = res.readStrings();
            //App.trace("KET QUA = " + matchResult);
            //this.matchFinished(message, slotResults, matchResult, M.get('MATCH_RESULT'), form, buttons);

            coinFlyed = false;

            StartCoroutine(showTotalMoney());
            StartCoroutine(_ClearCardWhenGameOver());

            //an phan goi y xep bai
            sortDone(false);

            //an state button doi voi truong hop thang trang
            stateBtns[3].gameObject.SetActive(false);
            stateBtns[1].gameObject.SetActive(false);





        });
        #endregion

        #region //HANDLER [SET_TURN]
        var req_SET_TURN = new OutBounMessage("SET_TURN");
        req_SET_TURN.addHead();
        handelerCommand.Add("SET_TURN");
        App.ws.sendHandler(req_SET_TURN.getReq(), delegate (InBoundMessage res_SET_TURN)
        {
            int slotId = res_SET_TURN.readByte();
            int turnTimeOut = res_SET_TURN.readShort();
            int playerRemainDuration = res_SET_TURN.readShort();

            App.trace("RECV [SET_TURN]");
            App.trace("slotId = " + slotId + "| turnTimeOut = " + turnTimeOut + "|playerRemainDuration  = " + playerRemainDuration);
            if(slotId == -2)
            {
                preCoroutine = StartCoroutine(_showCountDOwn(turnTimeOut));
                return;
            }
            if(turnTimeOut == 0)
            {
                run = false;
                for (int i = 0; i < 4; i++)
                {
                    mauBinhImgs[19 + i].gameObject.SetActive(false);
                }
                return;
            }
            for(int i = 0; i < 4; i++)
            {
                mauBinhImgs[19 + i].fillAmount = 1;
                if(exitsSlotList[i] == true)
                    mauBinhImgs[19 + i].gameObject.SetActive(true);
            }
            time = turnTimeOut;
            curr = 0;
            run = true;
        });
        #endregion
    }


    // IEnumerator _showTheLe() {
    //     yield return new WaitForSeconds(5f);
    //      VuaBaiDepController.instance.canSent = false;
    //      isBeautiful = false;
    //     VuaBaiDepController.instance.PlayCanSentAnim();

    // }


    IEnumerator _ClearCardWhenGameOver()
    {
        if (isPlaying)
            yield return null;
        yield return new WaitForSeconds(5f);
        LoadingControl.instance.delCoins();
        svIds.Clear();  //Xóa dánh sách bài game trước đó
        myCardIdList.Clear();   //XÓA DANH SÁCH BÀI CỦA MÌNH
        #region //XÓA LIST BÀI GAME TRƯỚC
        foreach (int key in playerCardList.Keys.ToList())
        {
            List<PlayerCard> ls = playerCardList[key];
            for (int i = 0; i < ls.Count; i++)
            {
                DestroyObject(ls[i].Rtf.gameObject);
            }
        }
        playerCardList.Clear();
        mauBinhImgs[1].transform.parent.parent.localScale = Vector2.one;
        #endregion

        for (int i = 0; i < 4; i++)  //Ẩn tiền kiếm được ở game trước
        {
            mbTextList[25 + i].gameObject.SetActive(false);
            mbTextList[7 + i].gameObject.SetActive(false);
            mbTextList[3 + i].transform.parent.gameObject.SetActive(false);
        }

        for (int i = 0; i < 3; i++)  //Ẩn các text xếp xong
        {
            mbTextList[21 + i].gameObject.SetActive(false);
            mauBinhGojList[12 + i].SetActive(false);    //Xóa lớp trans quân bài của mình mình
            mauBinhGojList[i + 8].SetActive(false); //Xóa lớp đen trên ava
        }

        mbTextList[11].gameObject.SetActive(false); //Ẩn text so chi thứ mấy
        mauBinhImgs[14].gameObject.SetActive(false);    //Ẩn lớp phủ của mình
    }

    private void getTableDataEx()
    {

        var req_GET_TABLE_DATA_EX = new OutBounMessage("GET_TABLE_DATA_EX");
        req_GET_TABLE_DATA_EX.addHead();
        req_GET_TABLE_DATA_EX.writeAcii("");
        //req_getTableChange.print();
        App.ws.send(req_GET_TABLE_DATA_EX.getReq(), delegate (InBoundMessage res)
        {
            App.trace("RECV [GET_TABLE_DATA_EX]");

            loadStateData(res);
            loadTableData(res);
            loadPlayerMatchPoint(res);
            loadBoardData(res);

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
        command_START.action = delegate (int s) {
            App.trace("START NÈ");
        };
        stateStart.commands.Add(command_START);
        stateById.Add(stateStart.Id, stateStart);
        stateByCode.Add(stateStart.Code, stateStart);

        var state_viewTable = new State(1, "viewTable", 1);
        stateById.Add(state_viewTable.Id, state_viewTable);
        stateByCode.Add(state_viewTable.Code, state_viewTable);

        foreach (State tmpState in stateById.Values)
        {
            tmpState.commandsByPosition = new Dictionary<int, List<StateCommand>>();
            for (int i = 0; i < tmpState.commands.Count; i++)
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
                    tmpState.commandsByPosition[command.Position] = commands;
                }
                commands.Add(command);
            }
        }

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

    private Dictionary<int, Player> playerList;
    private int mySlotId = -1;
    private int currOwner = -1;
    private bool[] exitsSlotList = { false, false, false, false };
    private void loadTableData(InBoundMessage res)
    {
        playerList = new Dictionary<int, Player>();

        mySlotId = res.readByte();

        isPlaying = res.readByte() == 1;
        App.trace("MY SLOT ID = " + mySlotId + "|isPlaying = " + isPlaying);
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
                App.trace("CURR OWNER = " + currOwner);
            }

            //slotId = detecSlotIdBySvrId(slotId,isOwner);

            int tmp = slotId;
            if (slotId > -1)
            {
                Player player = new Player(detecSlotIdBySvrId(slotId), tmp, playerId, nickName, avatarId, avatar, isMale, chipBalance, starBalance, score, level, isOwner);

                //App.trace("PLAYER SLOT " + player.SlotId + "ADDED!" + player.SvSlotId + "|isOwner = " + isOwner);
                if (!playerList.ContainsKey(player.SlotId))
                {
                    exitsSlotList[player.SlotId] = true;
                    playerList.Add(player.SlotId, player);
                }
                /*
                else
                {
                    player.SlotId = detecSlotIdBySvrId(tmp);
                    exitsSlotList[player.SlotId] = true;
                }*/

                try
                {
                    //setInfo(player, avatarList[slotId], infoObjectList[slotId], balanceTextList[slotId], nickNameTextList[slotId], ownerList[slotId]);
                    App.trace("SET PLAYER INFO " + player.SlotId + "|SVID = " + player.SvSlotId);
                    setInfo(player, mauBinhImgs[player.SlotId + 15], mauBinhGojList[player.SlotId], mbTextList[player.SlotId + 13], mbTextList[player.SlotId + 17], mauBinhGojList[player.SlotId + 4]);
                    exitsSlotList[player.SlotId] = true;
                }
                catch
                {
                    App.trace("sai ID = " + slotId);
                }
            }
        }

        int currentTurnSlotId = res.readByte();

        int currTimeOut = res.readShort();
        //showCountDown(currTimeOut);
        App.trace("currentTurnSlotId = " + currentTurnSlotId + "|currTimeOut = " + currTimeOut);
        int slotRemainDuration = res.readShort();
        var currentState = res.readByte();
        enterState(stateById[currentState]);
        if(currTimeOut > 0 && currentTurnSlotId == -1)
        {

            time = currTimeOut;

        }
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

    private void changePlayersMode(bool isPlay) {
        for (int i = 0; i < playerList.Count; i++)
        {
            playerList[i].PlayMode = isPlay;
        }
    }

    private void loadBoardData(InBoundMessage res, bool isChiaBai = false)
    {


        int slotCount = res.readByte();
        App.trace("slot count = " + slotCount);
        for (int i = 0; i < slotCount; i++)
        {
            int slotId = res.readByte();
            //
            int lineCount = res.readByte();
            App.trace("slotId = " + slotId + "|BOARD DATA lineCount = " + lineCount);
            for (int j = 0; j < lineCount; j++)
            {
                int cardLineId = res.readByte() - 1;
                int cardCount = res.readByte();

                if (cardCount < 0)   //ĐÂY LÀ CÁC QUÂN BÀI ĐÃ ĐÁNH
                {

                    List<int> ids = new List<int>();
                    ids = res.readBytes();
                    //CardUtils.svrIdsToIds(ids);
                    App.trace("slotId = " + slotId + "|length = " + ids.Count + "|cardLineId = " + cardLineId);
                    if (slotId == mySlotId)
                        myCardIdList.Add(cardLineId, ids);
                }
                else
                {
                    App.trace("slotId = " + slotId + "CARD COUNT = " + cardCount);
                    /*
                    for(int k = 0; k < cardCount; k++)
                    {

                    }*/
                }
            }

        }

        if (isPlaying == false && myCardIdList.Count < 1)
        {
            return;
        }
        if(time < 5 && isChiaBai == false)    //Thời gian còn lại nhỏ hơn 5 thì méo chia bài
        {
            return;
        }
        if (isChiaBai)
        {
            divideCards(true);
        }
        else
        {
            divideCards(false);
        }

        if (time > 0)
        {
            for (int i = 1; i < 4; i++)
            {
                mauBinhImgs[19 + i].fillAmount = time / 120;
                if (exitsSlotList[i] == true)
                    mauBinhImgs[19 + i].gameObject.SetActive(true);
            }
            if(myCardIdList.Count > 0)
            {
                mauBinhImgs[19].gameObject.SetActive(true);
            }
            curr = 0;
            run = true;
        }

    }

    private Dictionary<string, Button> stateButtonByCommandCode;
    private Dictionary<int, Button> stateButtonByPosition;
    private State state;
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

        foreach (int position in state.commandsByPosition.Keys)
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
    private string currentState = "";
    private void enterState2(State state, InBoundMessage res = null)
    {
        if (state == null)
            return;
        currentState = state.Code;
        switch (state.Code)
        {
            default:
                    App.trace("ENTER STATE " + state.Code);
            break;
        }
    }

    /// <summary>
    /// Xác định vị trí theo Player luôn ở vị trí 0
    /// </summary>
    /// <param name="slotId"></param>
    /// <returns></returns>
    private int detecSlotIdBySvrId(int slotId)
    {

        int temp = slotId;
        if (mySlotId >= 0)
        {
            temp = temp - mySlotId;
            temp = temp < 0 ? (temp + 4) : temp;
        }
        return temp;
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
        foreach (Player pl in playerList.Values.ToList())
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

    private long[] chipList = { 0, 0, 0, 0, 0, 0, 0 };
    private void setInfo(Player player, Image im, GameObject infoObj, Text balanceText, Text nickNamText, GameObject ownerImg)
    {
        //im.gameObject.transform.localScale = Vector3.one;
        StartCoroutine(App.loadImg(im, App.getAvatarLink2(player.Avatar, (int)player.PlayerId),player.SvSlotId == mySlotId));
        if (infoObj != null)
            infoObj.SetActive(true);
        chipList[player.SlotId] = player.ChipBalance;
        balanceText.text = "100.0 K";
        balanceText.text = " " + (player.SlotId == 0 ? App.formatMoney(player.ChipBalance.ToString()) : App.formatMoneyAuto(player.ChipBalance));
        nickNamText.text = App.formatNickName(player.NickName, 10);
        ownerImg.SetActive(player.IsOwner);

    }

    #region //CLASS PLAYER
    public class Player
    {
        string nickName, avatar;
        int slotId, svSlotId, avatarId, level;
        bool isMale, isOwner;
        long playerId, chipBalance, starBalance, score;
        // viewing or playing this turn.
        bool playMode;

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

        public bool PlayMode
        {
            get
            {
                return playMode;
            }

            set
            {
                playMode = value;
            }
        }

        public Player(int slotId, int svSlotId, long playerId, string nickName, int avatarId, string avatar, bool isMale, long chipBalance, long starBalance, long score, int level, bool isOwner)
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
    private void divideCards(bool isStartMatch = true)
    {
        playerCardList.Clear();
        if(myCardIdList.Count > 0)
            StartCoroutine(_divideCards(isStartMatch));   //Chia bài cho mình
        for(int i = 1; i < 4; i++)
        {
            if (exitsSlotList[i])
            {
                StartCoroutine(_divideCards2(i, isStartMatch));
            }
        }
    }

    private int chiCount = 0;
    private IEnumerator _divideCards(bool isStartMatch)
    {
        if (!isStartMatch)
        {
            mauBinhImgs[1].transform.parent.parent.DOScale(1.5f, .05f);
            yield return new WaitForSeconds(.1f);
        }

        for (int i = 0; i < 3; i++)
        {
            mauBinhImgs[1 + i].transform.parent.SetAsFirstSibling();
        }
        //myCardIdList = new Dictionary<int, List<int>>();
        mauBinhGojList[17].SetActive(true);
        for (int i = 12; i > -1; i--)
        {
            int temp = i;
            /*
            RectTransform rtf = mauBinhImgs[temp / 5 + 1].GetComponent<RectTransform>();
            Vector2 vec = new Vector2(rtf.transform.position.x, rtf.transform.position.y);
            */
            RectTransform rtf = mauBinhImgs[temp / 5 + 1].GetComponent<RectTransform>();
            Vector2 vec = new Vector2(rtf.anchoredPosition.x * scX, rtf.anchoredPosition.y * scY);

            Image img = Instantiate(mauBinhImgs[0], mauBinhImgs[0].transform.parent, false);
            img.sprite = faces[52];
            img.transform.parent = mauBinhImgs[temp / 5 + 1].transform.parent;
            if(!isStartMatch)
                img.transform.localScale = Vector2.one;
            RectTransform mRtf = img.GetComponent<RectTransform>();
            if (isStartMatch)
            {
                mRtf.position = new Vector2(810 / scX, 600 / scY);
            }else
            {

                mRtf.anchoredPosition = vec;
            }
            //mRtf.anchoredPosition = new Vector2(vec.x + i * 65 , vec.y);

            //mRtf.DOScale(1.8f, .5f);
            img.gameObject.SetActive(true);


            int id = (int)Mathf.Floor(temp / 5) + 1;

            if (playerCardList.ContainsKey(id))
            {
                //App.trace("A " + id);
                playerCardList[id].Add(new PlayerCard(mRtf, img, img.transform.GetChild(1).gameObject));
            }
            else
            {
                List<PlayerCard> pl = new List<PlayerCard>();
                pl.Add(new PlayerCard(mRtf, img, img.transform.GetChild(1).gameObject));
                playerCardList.Add(id, pl);
                //App.trace("TAO CO ADD NE " + id.ToString());
            }

            DOTween.To(() => mRtf.anchoredPosition, x => mRtf.anchoredPosition = x, new Vector2(vec.x + (temp % 5) * 65, vec.y), .1f).OnComplete(() =>
            {

                /*
                mRtf.anchorMax = rtf.anchorMax;
                mRtf.anchorMin = rtf.anchorMin;
                mRtf.pivot = rtf.pivot;
                */
                /*

                 */
                mRtf.SetAsFirstSibling();
                mRtf.DORotate(new Vector3(0, 90, 0), .1f).OnComplete(() =>
                {
                    //int carId = temp;
                    int carId = UnityEngine.Random.RandomRange(0, 52);

                    int chi = (int)Mathf.Floor(temp / 5);
                    try
                    {
                        img.sprite = faces[myCardIdList[chi][temp % 5]];
                    }
                    catch
                    {
                        img.sprite = faces[52];
                        App.trace("SAI " + myCardIdList[chi][temp % 5]);
                    }
                    /*if (myCardIdList.ContainsKey(chi))
                    {
                        myCardIdList[chi].Add(carId);
                        //App.trace(cardIdList[chi].Count.ToString());
                    }
                    else
                    {
                        List<int> ls = new List<int>();
                        ls.Add(carId);
                        myCardIdList.Add(chi,ls);
                    }*/
                    mRtf.DORotate(new Vector3(0, 0, 0), .1f).OnComplete(()=> {

                    });


                });
            });
            yield return new WaitForSeconds(.05f);
        }
        mauBinhGojList[17].SetActive(false);
        yield return new WaitForSeconds(.25f);
        mauBinhImgs[1].transform.parent.parent.DOScale(1.8f, .25f).SetEase(Ease.OutBack);
        if(isWhiteWin == false)
        {
            yield return new WaitForSeconds(.25f);

            for (int i = 0; i < 3; i++)
            {
                chiScore[i] = -1;
            }

            for(int k = 0; k < 3; k++)
            {
                List<int> ls = myCardIdList[k];
                //chiCount = k;
                string rs = detectChi(k, ls);
                Debug.Log("1 => "+rs);
                if(rs.Equals("Lủng"))
                {
                    for(int i = 0; i < 3; i++)
                    {
                        mbTextList[i].text = App.listKeyText["MAUBINH_BINHLUNG"]; //"Binh lủng";
                        if (mbTextList[i].transform.parent.gameObject.activeSelf == false)
                        {
                            mbTextList[i].transform.parent.gameObject.SetActive(true);
                        }
                    }
                    break;
                }

                if (mbTextList[k].transform.parent.gameObject.activeSelf == false)
                {
                    mbTextList[k].transform.parent.gameObject.SetActive(true);
                }

            }

            stateBtns[0].gameObject.SetActive(true);
        stateBtns[1].gameObject.SetActive(false);
        stateBtns[3].gameObject.SetActive(false);
        //stateBtns[2].gameObject.SetActive(true);
        }
    }
    private float scX = 1706f / Screen.width;
    private float scY = 960f/ Screen.height;

    /// <summary>
    /// List bài của ng choi được CLONE|Dic[xy,ls] x: slot,y: chi|ls: list card của chi y
    /// </summary>
    private Dictionary<int, List<PlayerCard>> playerCardList = new Dictionary<int, List<PlayerCard>>();
    //Chia bài cho các thằng khác
    private IEnumerator _divideCards2(int slot, bool isFilp = false)
    {
        for (int i = 4; i < 13; i++)    //SET SIB lại cho các line bài
        {
            mauBinhImgs[i].transform.parent.SetAsFirstSibling();
        }
        for (int i = 12; i > -1; i--)
        {
            int temp = i;
            //RectTransform rtf = mauBinhImgs[temp / 5 + 1 + slot * 3].GetComponent<RectTransform>();
            //Vector2 vec = new Vector2(rtf.position.x, rtf.position.y);
            RectTransform rtf = mauBinhImgs[temp / 5 + 1 + slot * 3].GetComponent<RectTransform>();
            //App.trace("[" + rtf.position.x + "|" + rtf.position.y + "]");
            Vector2 vec = new Vector2(rtf.anchoredPosition.x * scX, rtf.anchoredPosition.y * scY);
            //App.trace(vec.x +"|" +vec.y);
            Image img = Instantiate(mauBinhImgs[13], mauBinhImgs[13].transform.parent, false);
            img.sprite = faces[52];
            img.transform.parent = mauBinhImgs[temp / 5 + slot * 3 + 1].transform.parent;
            RectTransform mRtf = img.GetComponent<RectTransform>();
            if (isFilp)
            {
                mRtf.position = new Vector2(810 / scX, 600 / scY);
            }
            else
            {
                mRtf.anchoredPosition = vec;
            }
            //mRtf.SetAsLastSibling();
            //mRtf.anchoredPosition = new Vector2(vec.x + i * 65 , vec.y);

            int id = slot * 10 + (temp / 5 + 1); //Add vào list bài ng chơi khác 11: slot 1, chi 1|12: slot 1, chi 2
            if (playerCardList.ContainsKey(id))
            {
                playerCardList[id].Add(new PlayerCard(mRtf, img, img.transform.GetChild(0).gameObject));
            }
            else
            {
                List<PlayerCard> pl = new List<PlayerCard>();
                pl.Add(new PlayerCard(mRtf, img, img.transform.GetChild(0).gameObject));
                //App.trace("CARD LIST ID = " + id);
                playerCardList.Add(id, pl);
            }

            img.gameObject.SetActive(true);
            DOTween.To(() => mRtf.anchoredPosition, x => mRtf.anchoredPosition = x, new Vector2(vec.x + (temp % 5) * 65, vec.y), .5f).OnComplete(() =>
            {
                //img.transform.parent = mauBinhImgs[temp / 5 + slot * 3 + 1].transform.parent;
                mRtf.SetAsFirstSibling();
            });
            yield return new WaitForSeconds(.125f);
        }
        if(slot != 0 && isFilp && isWhiteWin == false)
        {
            playerAttUpdate(slot, false);
        }
        divideCardDone = true;
    }

    public void addCardPrepare(int id, RectTransform rtf, Transform parrentRtf, int slib, Vector2 pos, GameObject bor, string name)
    {
        if(id == 0)
        {
            cardPrepareList.Clear();
        }
        CardPrepare cP = new CardPrepare(id,rtf,parrentRtf,slib, pos, bor, name);
        cardPrepareList.Add(cP);
        //App.trace("CARD " + id + " ADDED WITH x = " + cP.Pos.x + "|y = " + cP.Pos.y + "|sibId = " + slib);
        App.trace(cardPrepareList.Count + " ADD CHI" + cP.ParrentRtf.GetSiblingIndex());
    }
    /*
    [HideInInspector]
    public int clickCount = 0;
    [HideInInspector]
    public string carinteract = "";
    public void cardClicked()
    {
        carinteract = "click";
           clickCount++;
        if(clickCount == 2)
        {
            clickCount = 0;
            swapPrepareCard();
            carinteract = "";
        }
    }

    public int getCardClickCount()
    {
        return clickCount;
    }
    */

    public void removeCardPrepare()
    {
        if (cardPrepareList.Count > 1)
        {
            cardPrepareList.RemoveAt(1);
        }
    }

    public void removeLastCardPrepare()
    {
        if (cardPrepareList.Count > 0)
        {
            cardPrepareList[cardPrepareList.Count - 1].Border.SetActive(false);
            cardPrepareList.RemoveAt(cardPrepareList.Count - 1);
        }
    }

    private class CardPrepare
    {
        private int id, slib;
        private RectTransform rtf;
        private Transform parrentRtf;
        private Vector2 pos;
        private GameObject border;
        private string name;
        public CardPrepare(int id, RectTransform rtf, Transform parrentRtf, int slib, Vector2 pos, GameObject border, string name)
        {
            this.Slib = slib;
            this.Id = id;
            this.Rtf = rtf;
            this.ParrentRtf = parrentRtf;
            this.Pos = pos;
            this.Border = border;
            this.Name = name;
        }

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

        public GameObject Border
        {
            get
            {
                return border;
            }

            set
            {
                border = value;
            }
        }

        public Vector2 Pos
        {
            get
            {
                return pos;
            }

            set
            {
                pos = value;
            }
        }
        public Transform ParrentRtf
        {
            get
            {
                return parrentRtf;
            }

            set
            {
                parrentRtf = value;
            }
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

        public int Slib
        {
            get
            {
                return slib;
            }

            set
            {
                slib = value;
            }
        }
    }
    /// <summary>
    /// Bài của người chơi khác
    /// </summary>
    public class PlayerCard
    {
        private RectTransform rtf;
        private Image img;
        private GameObject trans;
        public PlayerCard(RectTransform rtf, Image img, GameObject trans)
        {
            this.Rtf = rtf;
            this.Img = img;
            this.Trans = trans;
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

        public Image Img
        {
            get
            {
                return img;
            }

            set
            {
                img = value;
            }
        }

        public GameObject Trans
        {
            get
            {
                return trans;
            }

            set
            {
                trans = value;
            }
        }
    }

    public void swapPrepareCard()
    {
        App.trace("FUC " + cardPrepareList.Count);
        if (cardPrepareList.Count < 2)
        {
            cardPrepareList[0].Rtf.position = cardPrepareList[0].Pos;
            cardPrepareList[0].Rtf.parent = cardPrepareList[0].ParrentRtf;
            cardPrepareList[0].Rtf.SetSiblingIndex(cardPrepareList[0].Slib);


        }

        else
        {
            mauBinhGojList[17].SetActive(true);
            //Đổi chi
            int chi0 = 2 - cardPrepareList[0].ParrentRtf.GetSiblingIndex(); //Chi của cây đang bị hold
            int chi1 = 2 - cardPrepareList[1].ParrentRtf.GetSiblingIndex(); //Chi của cây chuẩn bị đổi
            int slib0 = cardPrepareList[0].Slib;    //Slib của card đang hold
            int slib1 = cardPrepareList[1].Slib;    //Slib của card sắp bị đổi
            //CardUtils.detecHumanCard();
            //CardUtils.detecHumanCard(int.Parse();
            App.trace("DOI CHI " + chi0 + " voi chi " + chi1);
            int id0 = int.Parse(cardPrepareList[0].Name) - 1;   //Id card đang bị hold
            int id1 = int.Parse(cardPrepareList[1].Name) -1;    //Id của card sắp bị đổi
            for (int i = 0; i < myCardIdList[chi0].ToList().Count; i++)
            {
                if(myCardIdList[chi0][i] == id0)
                {
                    myCardIdList[chi0][i] = id1;
                    break;
                }
            }

            //App.trace("1447 " + myCardIdList[chi1].ToList().Count);

            for (int i = 0; i < myCardIdList[chi1].ToList().Count; i++)
            {
                if (myCardIdList[chi1][i] == id1)
                {
                    myCardIdList[chi1][i] = id0;
                    break;
                }
            }


            foreach (int key in myCardIdList.Keys.ToList())
            {
                CardUtils.svrIdsToIds(myCardIdList[key]);
            }

            //Đổi card
            int tmpSlib = cardPrepareList[0].Slib;
            Transform tmpTrf = cardPrepareList[0].ParrentRtf;
            Vector2 tmpVec = cardPrepareList[0].Pos;
            cardPrepareList[0].Rtf.position = cardPrepareList[1].Pos;
            cardPrepareList[0].Rtf.parent = cardPrepareList[1].ParrentRtf;

            cardPrepareList[0].Rtf.SetSiblingIndex(cardPrepareList[1].Slib);
            /*
            cardPrepareList[0].Rtf.DOMove(cardPrepareList[1].Pos, .25f).OnComplete(() =>
            {
                cardPrepareList[0].Rtf.parent = cardPrepareList[1].ParrentRtf;
                cardPrepareList[0].Rtf.SetSiblingIndex(cardPrepareList[1].Slib);
            });
            */
            cardPrepareList[1].Rtf.parent = tmpTrf;
            cardPrepareList[1].Rtf.SetSiblingIndex(tmpSlib);
            cardPrepareList[1].Border.SetActive(false);
            cardPrepareList[0].Border.SetActive(false);
            cardPrepareList[1].Rtf.DOMove(tmpVec, .25f).OnComplete(() =>
            {
                for(int i = 0; i< 3; i++)
                {
                    chiScore[i] = -1;
                }

                for (int k = 0; k < 3; k++)
                {
                    List<int> ls = myCardIdList[k];
                    string rs = detectChi(k, ls);
                    Debug.Log("rs 2 = " + rs);
                    //App.trace(rs);
                    if (rs.Equals("Lủng"))
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            mbTextList[i].text = App.listKeyText["MAUBINH_BINHLUNG"]; //"Binh lủng";
                            if (mbTextList[i].transform.parent.gameObject.activeSelf == false)
                            {
                                mbTextList[i].transform.parent.gameObject.SetActive(true);
                            }
                        }
                        break;
                    }

                    if (mbTextList[k].transform.parent.gameObject.activeSelf == false)
                    {
                        mbTextList[k].transform.parent.gameObject.SetActive(true);
                    }
                }
                mauBinhGojList[17].SetActive(false);
            });



        }
        cardPrepareList.Clear();
    }

    //Bài của mình
    private Dictionary<int, List<int>> myCardIdList = new Dictionary<int, List<int>>();
    private long[] chiScore = new long[3]; //Độ mạnh yếu của các chi
    /// <summary>
    /// Xem các chi có bộ hay k. Có thì sẽ hiển thị ra
    /// </summary>
    private string detectChi(int chi, List<int> cardIds)
    {
        for (int i = 0; i < cardIds.Count; i++)
        {
            Debug.Log("Chi " + chi+" = "+cardIds[i]);
        }
        ChiRS chiRs = CardUtils.getChiByIds(cardIds);
        string rsChi = chiRs.Rs;
        Debug.Log("rsChi = " + rsChi);
        if (chi == 2){
            if(rsChi.Equals("Sảnh"))
            {
                if(mbTextList[0].text == App.listKeyText["BAND_SANH"] && mbTextList[1].text == App.listKeyText["BAND_SANH"]/*"Sảnh"*/)
                {
                    rsChi = "Sảnh";

                }
                else
                {
                    rsChi = "Mậu thầu";
                    chiRs.Score = chiRs.Score2;
                }
            }
            else if(rsChi == "Thùng")
            {
                if (mbTextList[0].text == App.listKeyText["BAND_THUNG"]/*"Thùng"*/ && mbTextList[1].text == App.listKeyText["BAND_THUNG"]/* "Thùng"*/)
                {
                    rsChi = "Thùng";
                }
                else
                {
                    rsChi = "Mậu thầu";
                    chiRs.Score = chiRs.Score2;
                }
            }
        }

        mbTextList[chi].text = rsChi;
        chiScore[chi] = chiRs.Score;
        //App.trace(chiRs.Rs + "|" + chiScore[chi] + "|" + chi);
        if(chi > 0)
        {
            if (chiScore[chi] > chiScore[chi - 1])
            {
                App.trace("Trước " + chi + " = " + chiScore[chi] + "|Sau " + (chi - 1) + " = " + chiScore[chi - 1]);
                return "Lủng";
            }

        }

        Debug.Log("chi => " + rsChi);
        return rsChi;


        /*
        List<int> tmp = new List<int>(cardIds);
        tmp.Sort();
        for(int i = 0; i < tmp.Count; i++)
        {

        }*/

    }

    public class CardUtils
    {
        private static string[] cardTypes = { "bích", "tép", "rô", "cơ" };
        /// <summary>
        /// Chuyển Id trên server về id trong sheet bài
        /// </summary>
        public static void svrIdsToIds(List<int> arr)
        {
            int cardHigh = 0; // A 2 .. 10 J Q K
            int cardType = 0; // Chất của quân bài
            int temp = 0;
            string rs = "";
            for (int i = 0; i < arr.Count; i++)
            {
                temp = arr[i];
                cardType = (int)Mathf.Floor(temp / 13);
                cardHigh = (int)Mathf.Floor(temp % 13) + 1;

                switch (cardHigh)
                {
                    case 1:
                        //App.trace("A " + cardTypes[cardType]);
                        rs = "A" + cardTypes[cardType] + "|" + rs;
                        break;
                    case 11:
                        rs = "J" + cardTypes[cardType] + "|"  +rs;
                        break;
                    case 12:
                        rs = "Q" + cardTypes[cardType] + "|" + rs;
                        break;
                    case 13:
                        rs = "K" + cardTypes[cardType] + "|" + rs;
                        break;
                    default:
                        //App.trace(cardHigh + cardTypes[cardType]);
                        rs = cardHigh + cardTypes[cardType] + "|" + rs;
                        break;

                }
            }
            //App.trace(arr.Count + "[" + rs + "]");
        }

        public static void detecHumanCard(int id)
        {
            string rs = "";
            int cardHigh = 0; // A 2 .. 10 J Q K
            int cardType = 0; // Chất của quân bài
            cardType = (int)Mathf.Floor(id / 13);
            cardHigh = (int)Mathf.Floor(id % 13) + 1;

            switch (cardHigh)
            {
                case 1:
                    //App.trace("A " + cardTypes[cardType]);
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
                    //App.trace(cardHigh + cardTypes[cardType]);
                    rs = cardHigh + cardTypes[cardType] + "|" + rs;
                    break;

            }
            //App.trace("[" + rs + "]");
        }

        public static ChiRS getChiByIds(List<int> arr)
        {
            int cardHigh = 0; // Độ lớn [A 2 .. 10 J Q K]
            int cardType = 0; // Chất của quân bài


            //Xác định bộ 5s
            if (arr.Count == 5)
            {
                int[] ar = new int[5];
                for (int i = 0; i < 5; i++)
                {
                    ar[i] = arr[i] % 13;
                    if (ar[i] == 0)
                            ar[i] = 13;
                }
                Array.Sort(ar);
                //App.trace(ar[0] + "|" + ar[1] + "|" + ar[2] + "|" + ar[3] + "|" + ar[4]);

                #region //Trường hợp 5 lá liên tiếp đồng chất | THÙNG PHÁ SẢNH
                cardType = (int)Mathf.Floor(arr[0] / 13);
                bool isThung = true;
                for (int i = 1; i < 5; i++)
                {
                    if (cardType != (int)Mathf.Floor(arr[i] / 13))
                    {
                        isThung = false;
                        break;
                    }
                }

                bool isSanh = true;
                for (int i = 0; i < 4; i++)
                {
                    if (ar[i + 1] - ar[i] != 1)
                    {
                        isSanh = false;
                        break;
                    }

                }

                if (ar[0] == 0 && ar[4] == 12)
                    isSanh = true;

                if (isThung && isSanh)
                {
                    //App.trace("AHHI " + ar[0] + "|" + ar[4]);
                    if (ar[0] == 0 && ar[4] == 12)
                    {
                        return new ChiRS("Thùng phá sảnh lớn", 90000000000);
                    }
                    else
                    {
                        return new ChiRS("Thùng phá sảnh", 80000000000 + ar[4]);
                    }

                }

                #endregion

                #region //Trường hợp tứ | TỨ QUÝ
                if ((ar[1] == ar[2] && ar[2] == ar[3] && ar[3] == ar[4] && ar[4] == ar[1]))
                    return new ChiRS("Tứ quý", 70000000000 + ar[3] * 100000000 + ar[0]);
                if (ar[0] == ar[1] && ar[1] == ar[2] && ar[2] == ar[3] && ar[3] == ar[0])
                    return new ChiRS("Tứ quý", 70000000000 + ar[3] * 100000000 + ar[4]);
                #endregion

                #region //Trường hợp 1 bộ 3 + 1 bộ đôi | CÙ LŨ
                if (ar[0] == ar[1] && ar[1] == ar[2] && ar[2] == ar[0] && ar[3] == ar[4])
                    return new ChiRS("Cù lũ", 60000000000 + ar[0] * 100000000 + ar[3] * 1000000);
                if (ar[0] == ar[1] && ar[4] == ar[2] && ar[2] == ar[3] && ar[3] == ar[4])
                    return new ChiRS("Cù lũ", 60000000000 + ar[3] * 100000000 + ar[0] * 1000000);
                #endregion

                #region //Trường hợp 5 lá đồng chất | THÙNG

                if (isThung)
                {
                    return new ChiRS("Thùng", 50000000000 + ar[4] * 100000000 + ar[3] * 1000000 + ar[2] * 10000 + ar[1] * 100 + ar[0]);
                }
                #endregion

                #region //Trường hợp 5 lá tạo dây | SẢNH
                isSanh = true;
                for (int i = 0; i < 4; i++)
                {
                    if (ar[i + 1] - ar[i] != 1)
                    {
                        isSanh = false;
                        break;
                    }

                }
                if (isSanh)
                    return new ChiRS("Sảnh", 40000000000 + ar[4] * 100000000);
                if(ar[4] == 13 && ar[0] == 1 && ar[1] == 2 && ar[2] == 3 && ar[3] == 4)
                {
                    return new ChiRS("Sảnh", 40000000000 + ar[3] * 100000000);
                }
                #endregion

                #region //Trường hợp 1 bộ 3 | SÁM CÔ
                int tmp = 0;
                for(int i = 1; i < 4; i++)
                {
                    if(ar[i] == ar[i + 1] && ar[i - 1] == ar[i])
                    {
                        tmp = i;
                        break;
                    }
                }
                if(tmp == 1)
                {
                    return new ChiRS("Sám cô", 30000000000 + ar[1] * 100000000 + ar[4] * 10000 + ar[3]);
                } else if(tmp == 2)
                {
                    return new ChiRS("Sám cô", 30000000000 + ar[2] * 100000000 + ar[4] * 10000 + ar[0]);
                }
                else if(tmp == 3)
                {
                    return new ChiRS("Sám cô", 30000000000 + ar[3] * 100000000 + ar[1] * 10000 + ar[0]);
                }
                /*
                if ((ar[2] == ar[1] && ar[1] == ar[0]) || (ar[2] == ar[1] && ar[2] == ar[3]) || (ar[2] == ar[3] && ar[3] == ar[4]))
                    return new ChiRS("Sám cô", 30000000000 + );
                    */
                #endregion

                #region //Trường hợp 2 đôi | THÚ
                if(ar[0] == ar[1] && ar[3] == ar[4])
                {
                    return new ChiRS("Thú", 20000000000 + ar[4] * 100000000 + ar[1] * 1000000 + ar[2] * 10000);
                }else if(ar[1] == ar[2] && ar[3] == ar[4])
                {
                    return new ChiRS("Thú", 20000000000 + ar[4] * 100000000 + ar[1] * 1000000 + ar[0] * 10000);
                }else if (ar[0] == ar[1] && ar[2] == ar[3])
                {
                    return new ChiRS("Thú", 20000000000 + ar[3] * 100000000 + ar[1] * 1000000 + ar[4] * 10000);
                }
                /*
                if ((ar[0] == ar[1] && ar[3] == ar[4]) || (ar[1] == ar[2] && ar[3] == ar[4] || (ar[0] == ar[1] && ar[2] == ar[3])))
                    return "Thú";
                    */
                #endregion

                #region //Trường hợp đôi | ĐÔI
                tmp = 0;
                bool isDoi = false;
                for (int i = 0; i < 4; i++)
                {
                    if (ar[i] == ar[i + 1])
                    {
                        tmp = i;
                        isDoi = true;
                        break;
                    }
                }

                if (isDoi)
                {
                    if (tmp == 0)
                    {
                        return new ChiRS("Đôi", 10000000000 + ar[0] * 100000000 + ar[4] * 1000000 + ar[3] * 10000 + ar[2] * 100);
                    }
                    else if (tmp == 1)
                    {
                        return new ChiRS("Đôi", 10000000000 + ar[1] * 100000000 + ar[4] * 1000000 + ar[3] * 10000 + ar[0] * 100);
                    }
                    else if (tmp == 2)
                    {
                        return new ChiRS("Đôi", 10000000000 + ar[2] * 100000000 + ar[4] * 1000000 + ar[1] * 10000 + ar[0] * 100);
                    }
                    else if (tmp == 3)
                    {
                        return new ChiRS("Đôi", 10000000000 + ar[3] * 100000000 + ar[2] * 1000000 + ar[1] * 10000 + ar[0] * 100);
                    }
                }


                /*
                for(int i = 1; i < 5; i++)
                {
                    if (ar[i] == ar[i - 1])
                        return "Đôi";
                }
                */

                return new ChiRS("Mậu thầu", ar[4] * 100000000 + ar[3] * 1000000 + ar[2] * 10000 + ar[1] * 100 + ar[0]);
                #endregion
            }


            //Xác định bôj 3
            if (arr.Count == 3)
            {
                int[] ar = new int[3];
                for (int i = 0; i < 3; i++)
                {
                    ar[i] = arr[i] % 13;
                    if (ar[i] == 0)
                        ar[i] = 13;
                }
                Array.Sort(ar);
                #region //Trường hợp Thùng
                bool isThung = true;
                for (int i = 1; i < 3; i++)
                {
                    if (cardType != (int)Mathf.Floor(arr[i] / 13))
                    {
                        isThung = false;
                        break;
                    }
                }
                if (isThung)
                {
                    return new ChiRS("Thùng", 50000000000 + ar[2] * 10000 + ar[1] * 100 + ar[0], ar[2] * 100000000 + ar[1] * 1000000 + ar[0] * 10000);
                }
                #endregion

                #region //Trường hợp dãy 3 | SẢNH 3
                if (ar[2] - ar[1] == 1 && ar[1] - ar[0] == 1)
                    return new ChiRS("Sảnh", 40000000000 + ar[2], ar[2] * 100000000 + ar[1] * 1000000 + ar[0] * 10000);
                #endregion

                #region //Trường hợp bộ 3 | SÁM CÔ
                bool isBa = true;
                if (ar[0] != ar[1] || ar[0] != ar[2])
                    isBa = false;
                if (isBa)
                    return new ChiRS("Sám cô", 30000000000 + ar[0] * 100000000);
                #endregion

                #region //Trường hợp đôi
                if(ar[0] == ar[1])
                {
                    return new ChiRS("Đôi", 10000000000 + ar[0] * 100000000 + ar[2] * 1000000);
                }
                if(ar[1] == ar[2])
                {
                    return new ChiRS("Đôi", 10000000000 + ar[1] * 100000000 + ar[0] * 1000000);
                }
                #endregion
                return new ChiRS("Mậu thầu", ar[2] * 100000000 + ar[1] * 1000000 + ar[0] * 10000);
            }
            return new ChiRS("Lủng",0);
        }
    }

    public class ChiRS
    {
        private long score;
        private string rs;
        private long score2;
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

        public long Score2
        {
            get
            {
                return score2;
            }

            set
            {
                score2 = value;
            }
        }

        public string Rs
        {
            get
            {
                return rs;
            }

            set
            {
                rs = value;
            }
        }

        public ChiRS(string rs, long score, long score2 = -1)
        {
            this.Score = score;
            this.Rs = rs;
            this.Score2 = score2;
        }
    }

    public class ThreeBandCompare
    {
        private int slotId, point;
        string sub, title;

        public ThreeBandCompare(int slotId, string sub, string title, int point)
        {
            this.SlotId = slotId;
            this.Sub = sub;
            this.Title = title;
            this.Point = point;
        }

        public int Point
        {
            get
            {
                return point;
            }

            set
            {
                point = value;
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

        public string Sub
        {
            get
            {
                return sub;
            }

            set
            {
                sub = value;
            }
        }

        public string Title
        {
            get
            {
                return title;
            }

            set
            {
                title = value;
            }
        }
    }

    public class CardCompare
    {
        private int point, bandIndex, svId;
        private string bandName;

        public CardCompare(int bandIndex, string bandName, int point, int svId)
        {
            this.BandIndex = bandIndex;
            this.BandName = bandName;
            this.Point = point;
            this.SvId = svId;
        }
        public int SvId
        {
            get
            {
                return svId;
            }

            set
            {
                svId = value;
            }
        }
        public int Point
        {
            get
            {
                return point;
            }

            set
            {
                point = value;
            }
        }

        public int BandIndex
        {
            get
            {
                return bandIndex;
            }

            set
            {
                bandIndex = value;
            }
        }

        public string BandName
        {
            get
            {
                return bandName;
            }

            set
            {
                bandName = value;
            }
        }
    }

    private bool isSorted = false;
    public void sortDone(bool isReSort)
    {
        if (!isReSort)
        {
            mauBinhImgs[14].gameObject.SetActive(true);
            mbTextList[0].transform.parent.gameObject.SetActive(false);
            mbTextList[1].transform.parent.gameObject.SetActive(false);
            mbTextList[2].transform.parent.gameObject.SetActive(false);
            mauBinhImgs[1].transform.parent.parent.DOScale(1.5f, .5f).SetEase(Ease.OutBack);

            stateBtns[0].gameObject.SetActive(false);
            stateBtns[1].gameObject.SetActive(true);
            stateBtns[3].gameObject.SetActive(true);
            //stateBtns[2].gameObject.SetActive(false);
            isSorted = true;
            return;
        }
        mauBinhImgs[14].gameObject.SetActive(false);
        mbTextList[0].transform.parent.gameObject.SetActive(true);
        mbTextList[1].transform.parent.gameObject.SetActive(true);
        mbTextList[2].transform.parent.gameObject.SetActive(true);
        mauBinhImgs[1].transform.parent.parent.DOScale(1.8f, .5f).SetEase(Ease.OutBack);

        stateBtns[0].gameObject.SetActive(true);
        stateBtns[1].gameObject.SetActive(false);
        stateBtns[3].gameObject.SetActive(false);
        //stateBtns[2].gameObject.SetActive(true);
        isSorted = false;
    }

    private IEnumerator _soChi(int id, string chiName)
    {
        /*
         id = xy;
         x: slot
         y: chi
         */

        if (id < 10)
        {
            #region //Nếu là mình
            playerCardList[id][0].Rtf.parent.SetAsLastSibling();
            int tmp = 3;
            Vector3 prePos = mbTextList[tmp].transform.parent.position;
            mbTextList[tmp].text = chiName;
            int chi = id % 10;
            switch (chi)
            {
                case 1:
                    mbTextList[tmp].transform.parent.position -= new Vector3(0, 161, 0);
                    break;
                case 2:
                    mbTextList[tmp].transform.parent.position -= new Vector3(0, 52, 0);
                    break;
                case 3:
                    mbTextList[tmp].transform.parent.position += new Vector3(0, 62, 0);
                    break;
            }

            for(int i = 0; i < 3; i++)
            {
                if(i + 1 != chi)
                {
                    for (int j = 0; j < playerCardList[i + 1 + id / 10].Count; j++)
                    {
                        playerCardList[i + 1 + id / 10][j].Trans.SetActive(true);
                    }
                }
            }
            yield return new WaitForSeconds(.75f);
            mbTextList[tmp].transform.parent.gameObject.SetActive(true);
            yield return new WaitForSeconds(3.25f);
            mbTextList[tmp].transform.parent.gameObject.SetActive(false);
            mbTextList[tmp].transform.parent.position = prePos;
            playerCardList[id % 10][0].Rtf.parent.SetSiblingIndex(3 - id % 10);
            //yield return new WaitForSeconds(2f);
            for (int i = 0; i < 3; i++)
            {
                if (i + 1 != chi)
                {
                    for (int j = 0; j < playerCardList[i + 1 + id / 10].Count; j++)
                    {
                        playerCardList[i + 1 + id / 10][j].Trans.SetActive(false);
                    }
                }
            }
            #endregion
        }else
        {
            #region //Là thằng khác
            App.trace("SO " + id);
            int chi = id % 10;
            for (int i = 0; i < 3; i++)
            {
                if (i + 1 != chi)
                {
                    for (int j = 0; j < playerCardList[i + 1 + id - chi].Count; j++)
                    {
                        playerCardList[i + 1 + id - chi][j].Trans.SetActive(true);
                    }
                }
            }

            playerCardList[id][0].Rtf.parent.SetAsLastSibling();
            int count = playerCardList[id].Count;
            for (int i = 0; i < count; i++)
            {
                int temp = i;
                RectTransform rtf = playerCardList[id][temp].Rtf;
                rtf.DORotate(new Vector3(0, 90, 0), .125f).OnComplete(() =>
                {
                    playerCardList[id][temp].Rtf.SetSiblingIndex(count - temp);
                    //playerCardList[id][temp].Img.overrideSprite = faces[15];
                    try
                    {
                        playerCardList[id][temp].Img.overrideSprite = faces[svIds[id / 10][5 * (chi - 1) + temp]];
                    }
                    catch
                    {
                        App.trace("id In svIds = " + (id / 10) +"|id in ls = " + (5 * (chi - 1) + temp));
                        playerCardList[id][temp].Img.overrideSprite = faces[15];
                    }
                    rtf.DORotate(new Vector3(0, 0, 0), .125f);
                });

                yield return new WaitForSeconds(.125f);
            }
            int chiId = id % 10;
            int tmp = 3 + id / 10;  //Id trong mbTextList
            mbTextList[tmp].text = chiName;
            Vector3 prePos = mbTextList[tmp].transform.parent.position;
            switch (chiId)
            {
                case 1:
                    mbTextList[tmp].transform.parent.position -= new Vector3(0, 80, 0);
                    break;
                case 3:
                    mbTextList[tmp].transform.parent.position += new Vector3(0, 70, 0);
                    break;
            }


            mbTextList[tmp].transform.parent.gameObject.SetActive(true);
            yield return new WaitForSeconds(3f);
            //mbTextList[tmp].transform.parent.gameObject.SetActive(false);
            mbTextList[tmp].transform.parent.position = prePos;

            //yield return new WaitForSeconds(2f);

            for (int i = 0; i < 3; i++)
            {
                if (i + 1 != chi)
                {
                    for (int j = 0; j < playerCardList[i + 1 + id - chi].Count; j++)
                    {
                        playerCardList[i + 1 + id - chi][j].Trans.SetActive(false);
                    }
                }
            }
            yield return new WaitForSeconds(1f);
            playerCardList[id][0].Rtf.parent.SetSiblingIndex(3 - id % 10);
            #endregion
        }

    }

    private IEnumerator _sochiText(int id)
    {
        stateBtns[1].gameObject.SetActive(false);

        yield return new WaitForSeconds(2f);
        mbTextList[12].text = App.listKeyText["MAUBINH_THANG3CHI"].ToUpper(); //"THẮNG 3 CHI";
        mbTextList[12].gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        mbTextList[12].gameObject.SetActive(false);
    }

    private IEnumerator _showCompare()
    {
        int[] rsPoint = { 0, 0, 0, 0};  //Chứa điểm của 4 slot
        int root = -1;  //Slot gốc so sánh với các slot khác
        string[] sapChi = { "", "", "", "" };
        if(mySlotId > -1 && myCardIdList.Count > 0)
        {
            root = mySlotId;
        }
        else
        {
            root = evenLsCompareData[0].SvId;
        }

        int endCount = evenLsCompareData.Count % 2 == 0 ? (evenLsCompareData.Count / 2) : ((evenLsCompareData.Count + 1) / 2);  //Khi nào thì hiển thị rs của mình sau mỗi chi

        for (int i = 0; i < 3; i++)  //Từng chi
        {
            foreach (int key in playerCardList.Keys.ToList())   //Hiển thị lớp trans trên các quân bài
            {
                for (int t = 0; t < playerCardList[key].Count; t++)
                {
                    if(key > 10)
                        playerCardList[key][t].Trans.SetActive(true);
                }
            }



            for (int j = 0; j < 4; j++)
            {
                mbTextList[3 + j].transform.parent.gameObject.SetActive(false);
                mbTextList[7 + j].text = "";
                mbTextList[7 + j].gameObject.SetActive(false);
                if (j != 0)
                {
                    mauBinhGojList[7 + j].SetActive(false);
                }
            }
            if (myCardIdList.Count > 0)
                for (int k = 0; k < 3; k++)  //Hiện lớp trans quân bài của mình mình
                {
                    mauBinhGojList[12 + k].SetActive(true);
                }
            string tempString = App.listKeyText["MAUBINH_SOCHI"].ToUpper();
            mbTextList[11].text = tempString + " " + (i+1); //"SO CHI " + (i + 1);
            mbTextList[11].gameObject.SetActive(true);
            yield return new WaitForSeconds(.5f);
            int count = 0;
            int rootPoint = 0;
            for (int j = 0; j < evenLsCompareData.Count; j++)  //Từng ele trong ls CompareData
            {

                CompareData rootData = null;
                CompareData othersData = null;
                if (oddLsCompareData[j].SvId == root)
                {
                    rootData = oddLsCompareData[j];
                    othersData = evenLsCompareData[j];
                }
                else if(evenLsCompareData[j].SvId == root)
                {
                    rootData = evenLsCompareData[j];
                    othersData = oddLsCompareData[j];
                }

                if(rootData != null && othersData != null)
                {


                    int slotIdInTable = getSlotIdBySvrId(rootData.SvId);
                    int tmp = 3 + slotIdInTable;



                    if (count == 0)
                    {
                        //===Hiển thị tên chi của ROOT===
                        if (myCardIdList.Count > 0)
                            mauBinhGojList[12 + 2 - i].SetActive(false);
                        mbTextList[tmp].text = rootData.LsCardCompare[i].BandName;
                        //mbTextList[tmp].transform.parent.gameObject.SetActive(true);
                        if (myCardIdList.Count > 0)
                        {   /*
                            switch (i + 1)
                            {
                                case 1:
                                    mbTextList[tmp].transform.position -= new Vector3(-80, 224, 0);
                                    break;
                                case 2:
                                    mbTextList[tmp].transform.position += new Vector3(0, 112, 0);
                                    break;
                                case 3:
                                    mbTextList[tmp].transform.position += new Vector3(-80, 112, 0);
                                    break;
                            }
                            */
                        }
                        else
                        {
                            //===Xòe bài===
                            int id1 = slotIdInTable * 10 + i + 1;
                            int countTmp1 = playerCardList[id1].Count;
                            playerCardList[id1][0].Rtf.parent.SetAsLastSibling();
                            for (int k = 0; k < countTmp1; k++)
                            {
                                int temp = k;
                                RectTransform rtf = playerCardList[id1][temp].Rtf;
                                rtf.DORotate(new Vector3(0, 90, 0), .125f).OnComplete(() =>
                                {
                                    playerCardList[id1][temp].Rtf.SetSiblingIndex(countTmp1 - temp);
                                    //playerCardList[id][temp].Img.overrideSprite = faces[15];
                                    try
                                    {
                                        playerCardList[id1][temp].Img.overrideSprite = faces[svIds[rootData.SvId][5 * i + temp]];
                                    }
                                    catch
                                    {
                                        App.trace("id In svIds = " + (id1 / 10) + "|id in ls = " + (5 * i + temp));
                                        playerCardList[id1][temp].Img.overrideSprite = faces[15];
                                    }
                                    rtf.DORotate(new Vector3(0, 0, 0), .125f).OnComplete(() => {
                                        playerCardList[id1][temp].Trans.SetActive(false);

                                    });
                                });
                            }
                            yield return new WaitForSeconds(.125f);
                            //=====End Xòe bài=====
                            /*
                            switch (i + 1)
                            {
                                case 1:
                                    mbTextList[tmp].transform.position -= new Vector3(-50, 152, 0);
                                    break;
                                case 2:
                                    mbTextList[tmp].transform.position += new Vector3(0, 80, 0);
                                    break;
                                case 3:
                                    mbTextList[tmp].transform.position += new Vector3(-50, 72, 0);
                                    break;
                            }*/
                        }
                        mbTextList[tmp].transform.parent.gameObject.SetActive(true);
                        yield return new WaitForSeconds(.125f);
                        //=====END Hiển thị tên chi của ROOT=====

                        //===SET sib chi ROOT===
                        mauBinhImgs[i + 1].transform.parent.SetSiblingIndex(2);
                        //======SET sib chi ROOT=====;

                    }
                    count++;
                    #region //Với thằng còn lại
                    //===XEM SẬP CHI===
                    slotIdInTable = getSlotIdBySvrId(othersData.SvId);
                    if (othersData.Title != "" && sapChi[slotIdInTable] == "")
                    {
                        sapChi[slotIdInTable] = othersData.Title;
                        App.trace("===SẬP CHI==== slot - " + slotIdInTable + "|title = " + othersData.Title);
                    }
                    //=====XEM SẬP CHI=====

                    //===Xòe bài===
                    int id = slotIdInTable * 10 + i + 1;
                    int countTmp = playerCardList[id].Count;
                    playerCardList[id][0].Rtf.parent.SetAsLastSibling();
                    for (int k = 0; k < countTmp; k++)
                    {
                        int temp = k;
                        RectTransform rtf = playerCardList[id][temp].Rtf;
                        rtf.DORotate(new Vector3(0, 90, 0), .125f).OnComplete(() =>
                        {
                            playerCardList[id][temp].Rtf.SetSiblingIndex(countTmp - temp);
                            //playerCardList[id][temp].Img.overrideSprite = faces[15];
                            try
                            {
                                playerCardList[id][temp].Img.overrideSprite = faces[svIds[othersData.SvId][5 * i+ temp]];
                            }
                            catch
                            {
                                App.trace("id In svIds = " + (id / 10) + "|id in ls = " + (5 * i+ temp));
                                playerCardList[id][temp].Img.overrideSprite = faces[15];
                            }
                            rtf.DORotate(new Vector3(0, 0, 0), .125f).OnComplete(() => {
                                playerCardList[id][temp].Trans.SetActive(false);

                            });
                        });
                    }
                    yield return new WaitForSeconds(.125f);
                    //=====End Xòe bài=====

                    int point = othersData.LsCardCompare[i].Point;
                    if(i == 0)
                    {
                        rsPoint[getSlotIdBySvrId(othersData.SvId)] += othersData.Point;
                    }
                    /*
                    else
                    {
                        rsPoint[getSlotIdBySvrId(othersData.SvId)] += point;    //Cộng điểm vào kết quả
                    }
                    */
                    rootPoint += point;
                    //===Hiển thị điểm của chi===
                    if (point > 0)
                    {
                        mbTextList[7 + slotIdInTable].font = mbFont[0];
                        mbTextList[7 + slotIdInTable].text = "+ " + point + " Chi";
                    }
                    else if (point < 0)
                    {
                        mbTextList[7 + slotIdInTable].font = mbFont[1];
                        mbTextList[7 + slotIdInTable].text = point + " Chi";
                    }
                    else
                    {
                        mbTextList[7 + slotIdInTable].font = mbFont[2];
                        mbTextList[7 + slotIdInTable].text = App.listKeyText["GAME_DRAW"]; //"\nHòa";
                    }
                    mauBinhGojList[7 + slotIdInTable].SetActive(true);  //Hiển thị blur trên ava
                    mbTextList[7 + slotIdInTable].gameObject.SetActive(true);
                    //=====END Hiển thị điểm của chi=====

                    //===Hiển thị tên chi===
                    tmp = 3 + slotIdInTable;
                    mbTextList[tmp].text = othersData.LsCardCompare[i].BandName;

                    /*
                    switch (i + 1)
                    {
                        case 1:
                            mbTextList[tmp].transform.position -= new Vector3(-50, 152, 0);
                            break;
                        case 2:
                            mbTextList[tmp].transform.position += new Vector3(0, 80, 0);
                            break;
                        case 3:
                            mbTextList[tmp].transform.position += new Vector3(-50, 72, 0);
                            break;
                    }
                    */
                    if (!mbTextList[tmp].transform.parent.gameObject.activeSelf)
                        mbTextList[tmp].transform.parent.gameObject.SetActive(true);
                    yield return new WaitForSeconds(.5f); ///1f
                    //=====END Hiển thị tên chi=====



                    #endregion

                    #region //Với thằng ROOT

                    if (count == endCount)
                    {
                        if(myCardIdList.Count > 0)
                        {
                            slotIdInTable = 0;
                        }
                        else
                        {
                            slotIdInTable = getSlotIdBySvrId(rootData.SvId);
                            mauBinhGojList[7 + slotIdInTable].SetActive(true);
                        }
                        //App.trace("COUNT = " + count + "| endcount = " + endCount + "SLot = " + slotIdInTable);
                        point = (-1) * rootPoint;

                        //===Hiển thị điểm của chi===
                        if (point > 0)
                        {
                            mbTextList[7 + slotIdInTable].font = mbFont[0];
                            mbTextList[7 + slotIdInTable].text = "+ " + point + " Chi";
                        }
                        else if (point < 0)
                        {
                            mbTextList[7 + slotIdInTable].font = mbFont[1];
                            mbTextList[7 + slotIdInTable].text = point + " Chi";
                        }
                        else
                        {
                            mbTextList[7 + slotIdInTable].font = mbFont[2];
                            mbTextList[7 + slotIdInTable].text = App.listKeyText["GAME_DRAW"];//"\nHòa";
                        }

                        mbTextList[7 + slotIdInTable].gameObject.SetActive(true);
                        //=====END Hiển thị điểm của chi=====
                        yield return new WaitForSeconds(.5f); ///1f

                    }
                    if (i == 0)
                    {
                        rsPoint[getSlotIdBySvrId(rootData.SvId)] += rootData.Point;
                        //App.trace("XÕA " + rootData.Point);
                    }
                    #endregion
                }
                else
                {
                    int point1 = evenLsCompareData[j].LsCardCompare[i].Point;
                    int point2 = oddLsCompareData[j].LsCardCompare[i].Point;
                    if (i == 0)
                    {
                        rsPoint[getSlotIdBySvrId(evenLsCompareData[j].SvId)] += evenLsCompareData[j].Point;
                        rsPoint[getSlotIdBySvrId(oddLsCompareData[j].SvId)] += oddLsCompareData[j].Point;
                    }
                    /*
                    else
                    {
                        rsPoint[getSlotIdBySvrId(evenLsCompareData[j].SvId)] += point1;    //Cộng điểm vào kết quả
                        rsPoint[getSlotIdBySvrId(oddLsCompareData[j].SvId)] += point2;
                    }*/
                }
            }
        }
        for (int j = 0; j < 4; j++)
        {
            mbTextList[3 + j].transform.parent.gameObject.SetActive(false);
            mbTextList[7 + j].text = "";
            mbTextList[7 + j].gameObject.SetActive(false);
            if (j != 0)
            {
                mauBinhGojList[7 + j].SetActive(false);
            }
        }
        yield return new WaitForSeconds(1f);

        #region //SẬP CHI?
        int sapChiCount = 0;
        bool hasSapChi = false;
        for(int i = 1; i < 4; i++)
        {
            if(sapChi[i] == "win3band")
            {
                sapChiCount -= 3;
                mbTextList[20 + i].font = mbFont[1];
                mbTextList[20 + i].text = App.listKeyText["MAUBINH_THANG3CHI"].ToUpper(); //"THẮNG 3 CHI";
                mbTextList[20 + i].gameObject.SetActive(true);
                hasSapChi = true;
            }
            if(sapChi[i] == "lost3band")
            {
                sapChiCount += 3;
                mbTextList[20 + i].font = mbFont[0];
                mbTextList[20 + i].text = App.listKeyText["MAUBINH_SAP3CHI"].ToUpper(); //"SẬP 3 CHI";
                mbTextList[20 + i].gameObject.SetActive(true);
                hasSapChi = true;
            }
        }
        if(hasSapChi == true)
            yield return new WaitForSeconds(1f);
        /*
        int slot = -1;
        if(myCardIdList.Count > 0)
        {
            if(sapChiCount > 0)
            {
                mbTextList[7].font = mbFont[0];
                mbTextList[7].text = "THẮNG\n" + sapChiCount + " CHI";
                mbTextList[7].gameObject.SetActive(true);
            }
            else
            {
                mbTextList[7].font = mbFont[1];
                mbTextList[7].text = "THUA\n" + ((-1)*sapChiCount) + " CHI";
                mbTextList[7].gameObject.SetActive(true);
            }

        }
        else
        {
            slot = getSlotIdBySvrId(evenLsCompareData[0].SvId);
            if(sapChiCount > 0)
            {
                mbTextList[20 + slot].font = mbFont[0];
                mbTextList[20 + slot].text = "THẮNG " + sapChiCount + " CHI";
                mbTextList[20 + slot].gameObject.SetActive(true);
            }
            else
            {
                mbTextList[20 + slot].font = mbFont[0];
                mbTextList[20 + slot].text = "THUA " + ((-1) * sapChiCount) + " CHI";
                mbTextList[20 + slot].gameObject.SetActive(true);
            }
        }
        yield return new WaitForSeconds(1f);
        */
        mbTextList[21].gameObject.SetActive(false); //ẨN SẬP CHI NẾU CÓ
        mbTextList[22].gameObject.SetActive(false);
        mbTextList[23].gameObject.SetActive(false);
        #endregion

        foreach (int key in playerCardList.Keys.ToList())   //Ẩn lớp trans trên các quân bài
        {
            for (int t = 0; t < playerCardList[key].Count; t++)
            {
                if (key > 10)
                    playerCardList[key][t].Trans.SetActive(false);
            }
        }

        mauBinhGojList[12].SetActive(false);
        mauBinhGojList[13].SetActive(false);
        mauBinhGojList[14].SetActive(false);

        for (int i = 1; i < 13; i++)    //SET SIB lại cho các line bài
        {
            mauBinhImgs[i].transform.parent.SetAsFirstSibling();
        }
        mbTextList[11].text = App.listKeyText["SUMMARY"].ToUpper(); //"TỔNG KẾT";
        yield return new WaitForSeconds(.5f);
        #region  //Tổng kết điểm
        for (int i = 0; i < 4; i++)
        {
            if (exitsSlotList[i] == false)
                continue;
            int point = rsPoint[i];
            if (point > 0)
            {
                mbTextList[7 + i].font = mbFont[0];
                   mbTextList[7 + i].text = "+ " + point + " Chi";
            }
            else if (point < 0)
            {
                mbTextList[7 + i].font = mbFont[1];
                mbTextList[7 + i].text = point + " Chi";
            }
            else if(playerList[i] != null)
            {
                if (playerList[i].PlayMode)
                {
                    mbTextList[7 + i].font = mbFont[2];
                    mbTextList[7 + i].text = App.listKeyText["GAME_DRAW"]; //"\nHòa";
                }
            }
            if(i!= 0)
            {
                mauBinhGojList[7 + i].SetActive(true);  //Hiển thị blur trên ava
                mbTextList[7 + i].gameObject.SetActive(true);
            }
            else {
                if (myCardIdList.Count > 0)
                    mbTextList[7].gameObject.SetActive(true);
            }

        }

        #endregion


        //isPlaying = false;
        //yield return new WaitForSeconds(1f);





        /*
        #region //TỔNG KẾT TIỀN
        for (int i = 0; i < lsGameRs.Count; i++)
        {
            int slot = getSlotIdBySvrId(lsGameRs[i].SvId);
            string t = lsGameRs[i].Title + "\n";
            if(lsGameRs[i].EarnValue > 0)
            {
                t = t + "+" + lsGameRs[i].EarnValue;
            }
            else if(lsGameRs[i].EarnValue  < 0)
            {
                t = t + lsGameRs[i].EarnValue;
            }
            else
            {
                t = "HÒA";
            }
            mbTextList[25 + slot].text = t;
            mbTextList[25 + slot].gameObject.SetActive(true);
            mbTextList[25 + slot].transform.DOScale(1.2f, .5f).SetLoops(10, LoopType.Yoyo);
        }
        #endregion


        showTotal = true;

        */
        showTotal = true;
    }


    public void playerAttUpdate(int slotId, bool isDone = false)
    {
        //slotId = getSlotIdBySvrId(slotId);
        if(slotId != 0)
        {
            if (isDone)
            {
                mbTextList[20 + slotId].font = mbFont[0];
                mbTextList[20 + slotId].text = App.listKeyText["ARRANGE_DONE"]; //"Xếp Xong";
                mbTextList[20 + slotId].transform.localScale = Vector2.one * .5f;
                mbTextList[20 + slotId].gameObject.SetActive(true);
                mbTextList[20 + slotId].transform.DOScale(1f, .5f).SetEase(Ease.OutBounce);
            }
            else
            {
                mbTextList[20 + slotId].font = mbFont[1];
                mbTextList[20 + slotId].text = App.listKeyText["CARD_ARRANGING"]; // "Đang Xếp...";
                mbTextList[20 + slotId].transform.localScale = Vector2.one * .5f;
                mbTextList[20 + slotId].gameObject.SetActive(true);
                mbTextList[20 + slotId].transform.DOScale(1f, .5f).SetEase(Ease.OutBounce);
            }
        }

    }

    public void subMidCard()
    {
        if (mbTextList[0].text == App.listKeyText["MAUBINH_BINHLUNG"] /*"Binh lủng"*/)
        {

            //App.showErr("Bài lủng. Vui lòng xếp lại bài");
            App.showErr(App.listKeyText["WARN_MAUBINH_BAILUNG"]);
            return;
        }
        submited = true;
        var req = new OutBounMessage("SUBMIT");
        req.addHead();
        req.writeByte(3);
        //List<int> ls = new List<int>();
        foreach (int key in myCardIdList.Keys.ToList())
        {

            req.writeBytes(myCardIdList[key]);
        }

        App.ws.send(req.getReq(), delegate(InBoundMessage res) {
            App.trace("SUBMITTED");
            stateBtns[3].gameObject.SetActive(false);
            stateBtns[1].gameObject.SetActive(false);
        },true,0);

        /*
        foreach (int key in myCardIdList.Keys.ToList())
        {
            CardUtils.svrIdsToIds(myCardIdList[key]);
        }
        */
    }


    public class CompareData
    {
        private int svId;
        private string title;
        private int point;
        private ThreeBandCompare threeBandCompare;
        List<CardCompare> lsCardCompare;

        public int SvId
        {
            get
            {
                return svId;
            }

            set
            {
                svId = value;
            }
        }
        public string Title
        {
            get
            {
                return title;
            }

            set
            {
                title = value;
            }
        }
        public int Point
        {
            get
            {
                return point;
            }

            set
            {
                point = value;
            }
        }
        public ThreeBandCompare ThreeBandCompare
        {
            get
            {
                return threeBandCompare;
            }

            set
            {
                threeBandCompare = value;
            }
        }

        public List<CardCompare> LsCardCompare
        {
            get
            {
                return lsCardCompare;
            }

            set
            {
                lsCardCompare = value;
            }
        }

        public CompareData(int svId, ThreeBandCompare threeBandCompare, List<CardCompare> lsCardCompare, int point, string title)
        {
            this.SvId = svId;
            this.ThreeBandCompare = threeBandCompare;
            this.LsCardCompare = lsCardCompare;
            this.Point = point;
            this.Title = title;
        }
    }

    #region //ĐĂNG KÝ RỜI BÀN
    private bool isPlaying = false, isKicked = false;
    public bool regQuit = false;
    private string statusKick = "";
    public GameObject notiText;
    public GameObject btnBackImg;
    public void showNoti()
    {
        if (!isPlaying || myCardIdList.Count == 0 || playerList.Count < 2)
        {
            LoadingControl.instance.delCoins();
            backToTableList();
            return;
        }
        this.regQuit = !regQuit;
        notiText.GetComponentInChildren<Text>().text = !regQuit ? App.listKeyText["GAME_BOARD_EXIT_CANCEL"] : App.listKeyText["GAME_BOARD_EXIT"]; //"Bạn đã hủy đăng ký rời bàn." : "Bạn đã đăng ký rời bàn.";
        btnBackImg.transform.localScale = new Vector2(regQuit ? -1 : 1, 1);
        //btnBackImg.GetComponent<RectTransform>().anchoredPosition = new Vector2(regQuit ? 115f : 7.5f, -7.5f);
        notiText.SetActive(true);
        StopCoroutine("_showNoti");
        StartCoroutine("_showNoti");
    }
    IEnumerator _showNoti()
    {

        yield return new WaitForSeconds(2f);
        notiText.SetActive(false);
        yield break;
    }

    public void backToTableList()
    {
        if (isPlaying == false || myCardIdList.Count == 0 || playerList.Count < 2 || this.regQuit)
        {
            //LoadingControl.instance.blackPanel.SetActive(true);
            DOTween.PauseAll();
            run = false;
            delAllHandle();
            StopAllCoroutines();
            if (isKicked == false)
            {
                EnterParentPlace(delegate() {
                    if (LoadingControl.instance.chatBox.activeSelf)
                    {
                        LoadingControl.instance.chatBox.SetActive(true);

                    }
                    //StopAllCoroutines();
                    //StartCoroutine(LoadingControl.instance._start());
                    StartCoroutine(openTable());
                });
                return;
            }
            if (LoadingControl.instance.chatBox.activeSelf)
            {
                LoadingControl.instance.chatBox.SetActive(false);

            }
            StartCoroutine(openTable());

        }
    }

    IEnumerator openTable()
    {
        exited = true;
        CPlayer.preScene = isKicked ? "MauBinhK" : "MauBinh";
        /*==========THAY=============
        yield return new WaitForSeconds(0.5f);

        SceneManager.LoadScene("TableList");

        //LobbyControll.instance.LobbyScene.SetActive(true);
        slideSceneAnim.Play("TableAnimation");
        //yield return async;

        Destroy(gameObject, 1f);

        yield return new WaitForSeconds(0.5f);
        LoadingControl.instance.
        .SetActive(false);
        if (isKicked == true)
            App.showErr(statusKick);
        //yield break;
        ======THAY============*/
        //LoadingControl.instance.loadingScene.SetActive(true);
        LoadingUIPanel.Show();
        SceneManager.LoadScene("TableList");
        yield return new WaitForSeconds(0.05f);
    }

    private void EnterParentPlace(Action callback)
    {
        CPlayer.clientCurrentMode = 0; // modeview
        CPlayer.clientTargetMode = 0;//mode view
        //LoadingControl.instance.loadingScene.SetActive(true);
        LoadingUIPanel.Show();
        var req = new OutBounMessage("ENTER_PARENT_PLACE");
        req.addHead();
        req.writeString("");
        req.writeByte(0);
        App.ws.send(req.getReq(), delegate (InBoundMessage res)
        {
            if (callback != null)
                callback();
        });
    }
    private List<string> handelerCommand = new List<string>();
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

    #region //CHAT
    [Header("=====CHAT=====")]
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

    #region //HELP
    public Toggle[] helpTogs;
    public void showHelp(bool isShow)
    {
        mauBinhGojList[11].SetActive(isShow);
        changeHelpTog(10);
    }
    //private int currHelpTog = -1;
    public void changeHelpTog(int id)
    {
        if(id == 0 && helpTogs[0].isOn == true)
        {

            mauBinhGojList[15].SetActive(true);
            mauBinhGojList[16].SetActive(false);
            helpTogs[0].interactable = false;
            helpTogs[1].interactable = true;
        }
        if(id == 1 && helpTogs[1].isOn == true)
        {
            mauBinhGojList[15].SetActive(false);
            mauBinhGojList[16].SetActive(true);
            helpTogs[0].interactable = true;
            helpTogs[1].interactable = false;
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
            LoadingControl.instance.showPlayerInfo(CPlayer.nickName, (long)CPlayer.chipBalance, (long)CPlayer.manBalance, CPlayer.id, true, mauBinhImgs[slotIdToShow + 15].overrideSprite, "me");
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
                    LoadingControl.instance.showPlayerInfo(pl.NickName, pl.ChipBalance, pl.StarBalance, pl.PlayerId, isCPlayerFriend, mauBinhImgs[slotIdToShow + 15].overrideSprite, typeShowInfo);
                });
                return;
            }
        }

    }
    #endregion

    public void balanceChanged(int slotId, long chipBalance, long starBalance)
    {
        int mSl = getSlotIdBySvrId(slotId);
        if (mSl > -1)
        {
            //StartCoroutine(_balanceChanged(chipList[mSl], chipBalance, mbTextList[13 + mSl]));
            chipList[mSl] = chipBalance;
            playerList[mSl].ChipBalance = chipBalance;
            playerList[mSl].StarBalance = starBalance;
            mbTextList[13 + mSl].text = mSl == 0 ? App.formatMoney(chipBalance.ToString()) : App.formatMoneyAuto(chipBalance);
        }

    }
    private IEnumerator _balanceChanged(long start, long end, Text txt, bool isMe = false)
    {
        if (start == end)
            yield break;

        if (start < end)
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

    /// <summary>
    /// KQUẢ GAME
    /// </summary>
    public class GameRS
    {
        private int svId;
        long earnValue;
        string title;

        public int SvId
        {
            get
            {
                return svId;
            }

            set
            {
                svId = value;
            }
        }

        public long EarnValue
        {
            get
            {
                return earnValue;
            }

            set
            {
                earnValue = value;
            }
        }

        public string Title
        {
            get
            {
                return title;
            }

            set
            {
                title = value;
            }
        }
        public GameRS(int svId, string title, long earnValue)
        {
            this.SvId = svId;
            this.Title = title;
            this.EarnValue = earnValue;
        }
    }


    #region //TIME LEAP
    private bool run = false;
    private float curr = 0;
    public float time = 1;
    private bool submited = false;
    void Update()
    {

        if (run)
        {
            curr += Time.deltaTime;
            for (int i = 0; i < 4; i++)
            {
                if (exitsSlotList[i] == true)
                {
                    mauBinhImgs[19 + i].fillAmount = (time - curr) / 120;
                }
            }
            if(curr > 115 && submited == false)
            {
                submited = true;
                sortDone(false);
                subMidCard();

            }
        }

    }
    #endregion
    private Coroutine preCoroutine = null;
    private IEnumerator _showCountDOwn(int timeOut)
    {
        /*
        if (regQuit)
        {
            backToTableList();
            yield break;
        }*/
        float mTime = timeOut - 2;
        mbTextList[11].gameObject.SetActive(false);
        mbTextList[29].text = App.listKeyText["GAME_PREPARE"].ToUpper(); //"CHUẨN BỊ";
        mbTextList[29].gameObject.SetActive(true);
        yield return new WaitForSeconds(.5f);
        mbTextList[29].gameObject.SetActive(false);
        yield return new WaitForSeconds(.5f);
        while (mTime > 0f)
        {
            mbTextList[29].text = mTime.ToString();
            mbTextList[29].gameObject.SetActive(true);
            yield return new WaitForSeconds(.5f);
            mbTextList[29].gameObject.SetActive(false);
            yield return new WaitForSeconds(.5f);
            mTime -= 1f;
            if(mTime == 4)
            {
                mbTextList[31].transform.parent.gameObject.SetActive(false);
            }
        }
        isPlaying = true;
        mbTextList[29].text = App.listKeyText["GAME_START"].ToUpper(); //"BẮT ĐẦU";
        mbTextList[29].gameObject.SetActive(true);
        yield return new WaitForSeconds(.5f);
        mbTextList[29].gameObject.SetActive(false);
        yield break;
    }

    private bool exited = false;
    private bool coinFlyed = true;
    private IEnumerator showTotalMoney()
    {
        List<int> winLs = new List<int>(), loseLs = new List<int>();
        if (showTotal == false)
            yield return new WaitForSeconds(1f);
        else
            yield return new WaitForSeconds(1f);
        for (int i = 0; i < 4; i++)
        {
            mbTextList[7 + i].gameObject.SetActive(false);
            if (i != 0)
            {
                //mauBinhGojList[7 + i].SetActive(false);
            }
        }
        #region //TỔNG KẾT TIỀN
        for (int i = 0; i < lsGameRs.Count; i++)
        {
            int slot = -1;
            if (playerList.Count > 1)
                slot = getSlotIdBySvrId(lsGameRs[i].SvId);
            else
            {
                slot = detecSlotIdBySvrId(lsGameRs[i].SvId);
            }


            string t = lsGameRs[i].Title + "\n";
            if (lsGameRs[i].EarnValue > 0)
            {
                mbTextList[25 + slot].font = mbFont[0];
                t = t + "+" + lsGameRs[i].EarnValue;
                winLs.Add(slot);
            }
            else if (lsGameRs[i].EarnValue < 0)
            {
                mbTextList[25 + slot].font = mbFont[1];
                t = t + lsGameRs[i].EarnValue;
                loseLs.Add(slot);
            }
            else
            {
                mbTextList[25 + slot].font = mbFont[2];
                t = App.listKeyText["GAME_DRAW"]; //"HÒA";
            }
            mbTextList[25 + slot].text = t;
            mbTextList[25 + slot].gameObject.SetActive(true);
            mbTextList[25 + slot].transform.DOScale(1.2f, .5f).SetLoops(10, LoopType.Yoyo);

            if (slot == 0)
            {
                if (lsGameRs[i].EarnValue > 0)
                {
                    SoundManager.instance.PlayEffectSound(SoundFX.CARD_WIN);
                    // SoundManager.instance.PlayUISound(SoundFX.CARD_WIN);
                }
                else
                {
                    SoundManager.instance.PlayEffectSound(SoundFX.CARD_LOSE);
                    // SoundManager.instance.PlayUISound(SoundFX.CARD_LOSE);
                }
            }
        }
        lsGameRs.Clear();
        #endregion

        #region //BAY TIỀN
        if(regQuit == false && isKicked == false && exited == false && CPlayer.preScene.Contains("TableList"))
        {

            for (int i = 0; i < winLs.Count; i++)
            {
                float x = 1706 * (mauBinhImgs[15 + winLs[i]].transform.position.x) / Screen.width;
                float y = 960 * (mauBinhImgs[15 + winLs[i]].transform.position.y) / Screen.height;
                Vector2 end = new Vector2(x, y);
                //App.trace("end X = " + x + "|Y = " + y);
                for (int j = 0; j < loseLs.Count; j++)
                {
                    float xx = 1706 * (mauBinhImgs[15 + loseLs[j]].transform.position.x) / Screen.width;
                    float yy = 960 * (mauBinhImgs[15 + loseLs[j]].transform.position.y) / Screen.height;
                    Vector2 start = new Vector2(xx, yy);
                    //App.trace("start X = " + xx + "|Y = " + yy);

                    LoadingControl.instance.flyCoins("line", 10, start, end);
                }
            }
        }
        #endregion
        coinFlyed = true;
        yield return new WaitForSeconds(.5f);
        SoundManager.instance.PlayUISound(SoundFX.CARD_TAI_XIU_BET);
        if ((!isPlaying && regQuit) || (isKicked && !isPlaying))
        {
            backToTableList();
        }
    }

    public void openSettingPanel()
    {
        LoadingControl.instance.openSettingPanel();
    }

    private string getWhiteWinTitle(string t)
    {
        switch (t)
        {
            case "FullStraight":
                return App.listKeyText["BAND_SANH_RONG"];//"SẢNH RỒNG";
            case "FullStraightFlush":
                return App.listKeyText["BAND_SANH_RONG_DONG_HOA"]; //"SẢNH RỒNG ĐỒNG HOA";
            case "SixPair":
                return App.listKeyText["BAND_LUC_PHE_BON"];//"LỤC PHÉ BÔN";
            case "FivePairAndATriple":
                return App.listKeyText["BAND_5DOI_1SAMCO"]; //"5 ĐÔI & 1 SÁM CÔ";
            case "AllFlush":
                return App.listKeyText["BAND_3THUNG"]; //"3 CÁI THÙNG";
            case "AllStraight":
                return App.listKeyText["BAND_3SANH"]; //"3 CÁI SẢNH";
        }
        return App.listKeyText["WHITE_WIN"].ToUpper();
    }

    private bool checkWhiteWin(string t) {
        if(t.Equals("FullStraight") || t.Equals("FullStraightFlush") || t.Equals("SixPair") || t.Equals("white_win") || t.Equals("FivePairAndATriple") ||
        t.Equals("AllFlush") || t.Equals("AllStraight")) {
            return true;
        }
        return false;
    }

    public void test()
    {
        myCardIdList.Add(0, new List<int>() { 1, 2, 3,6,7 });
        myCardIdList.Add(1, new List<int>() { 11, 21, 32,35,9 });
        myCardIdList.Add(2, new List<int>() { 1, 2, 3 });
        StartCoroutine(_divideCards(false));   //Chia bài cho mình)
    }
}
