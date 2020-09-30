using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;
using System.Linq;
using System;
using Core.Server.Api;

public partial class BoardManager : MonoBehaviour {
    public Transform[] cardLine, showPlayerCardList;
    public Transform cardLine0;
    public GameObject[] cardTiles;
    public Image[] cardImages;
    public Sprite[] faces;
    public Sprite back;
    public GameObject cardToInstantiate;
    public GameObject[] timeLeapList;
    public Button[] playerButtonList; // Chứa các nút bấm của ng chơi: Sắp bài, Bỏ lượt, Đánh
    public GameObject btnBackImg;
    private static Cards cards;
    public GameObject boardHolder;
    [Header("===ANIM===")]
    public Animator slideSceneAnim;
    [Header("========Text=====")]
    public Text[] moneyEarnedText;
    public Text countDownText, tableTitleText;
    [Header("======= Others ======")]
    public GameObject notiText, cardFx;
    /// <summary>
    /// 0: Trắng|1: Vàng
    /// </summary>
    public Font[] fontList;
    public Color[] winLoseColor;
    private RectTransform mTransform; //Cai nay cua cac la bai
    private bool isPlaying = false;
    private Vector2 tempToFade = new Vector2(0, 90);
    /// <summary>
    /// Di chuyển các lá bài khi người chơi đánh bài
    /// </summary>
    /// <param name="cardIds"></param>
    /// <param name="sourceSlotId"></param>
    /// <param name="sourceLineId"></param>
    /// <param name="targetSlotId"></param>
    /// <param name="targetLineId"></param>
    public void MoveCard(List<int> cardIds, int sourceSlotId, int sourceLineId, int targetSlotId, int targetLineId)
    {
        sourceSlotId = detecSlotIdBySvrId(sourceSlotId);

        List<GameObject> tempList = new List<GameObject>();
        //boardHolder = new GameObject("Board").transform;
        for (int i = 0; i < cardIds.Count; i++)
        {
            //var face = Cards.faces[i];
            GameObject temp = Instantiate(cardToInstantiate, cardLine[0].transform, false) as GameObject;
            //App.trace("FUCK U BITCH: " + cardIds[i]);
            temp.GetComponent<Image>().overrideSprite = faces[cardIds[i]];
            mTransform = temp.GetComponent<RectTransform>();
            mTransform.pivot = Vector2.zero;
            mTransform.anchorMin = Vector2.zero;
            mTransform.anchorMax = Vector2.zero;
            mTransform.localScale = new Vector3(1.2f, 1.2f, 1);
            //mTransform.anchoredPosition = Vector2.zero;
            mTransform.Rotate(new Vector3(0, 0, UnityEngine.Random.Range(-10, 10)));

            //mTransform.anchoredPosition = new Vector2(200 + i * 50, 300);
            //Destroy(temp, 1f);
            tempList.Add(temp);
        }

        /*
        foreach (Transform goj in cardLine[1])
        {

            Vector2 posToFade = goj.gameObject.GetComponent<RectTransform>().anchoredPosition + tempToFade;
            RectTransform gojTransform = goj.GetComponent<RectTransform>();
            goj.gameObject.transform.SetParent(cardLine[2].transform);
            //mTransform.SetParent(holder.transform);
            //gojTransform.SetSiblingIndex();
            DOTween.To(() => gojTransform.anchoredPosition, x => gojTransform.anchoredPosition = x, posToFade, 1f).SetEase(Ease.OutCirc).OnComplete(() => {
                //goj.gameObject.transform.SetParent(cardLine[2].transform);
            });
            gojTransform.DOScale(new Vector2(0.64f, 0.65f), 1f).SetEase(Ease.OutCirc);
        }
        */

        foreach (Transform goj in cardLine0.gameObject.transform)
        {
            /*
            goj.transform.SetParent(cardLine[2].transform);
            CardControl cardControl =  goj.gameObject.GetComponent<CardControl>();
            cardControl.DoFade();
            */
            //TweenParams tParms = new TweenParams();
            Vector2 posToFade = goj.GetComponent<RectTransform>().anchoredPosition + tempToFade;
            RectTransform gojTransform = goj.GetComponent<RectTransform>();
            //goj.SetParent(cardLine[1].transform);

            DOTween.To(() => gojTransform.anchoredPosition, x => gojTransform.anchoredPosition = x, posToFade, .75f).OnComplete(() => {
                gojTransform.SetParent(cardLine[1]);

            });
            /*
            DOTween.To(() => gojTransform.anchoredPosition, x => gojTransform.anchoredPosition = x, posToFade, 1f).OnComplete(() => {
                goj.SetParent(cardLine[1].transform);
                //goj.SetAsLastSibling();
            });
            */
            gojTransform.DOScale(new Vector2(0.8f, 0.8f), 1f);
        }

        mTransform = cardLine[0].transform.parent.GetComponent<RectTransform>();
        mTransform.anchoredPosition = moveFrom(sourceSlotId);
        cardLine[0].SetAsLastSibling();
        //Vector2 mPos = new Vector2(Random.Range(780, 820) - 50 * cardIds.Count, Random.Range(350, 390));
        Vector2 mPos = new Vector2(UnityEngine.Random.Range(0, 40) - 50 * cardIds.Count, UnityEngine.Random.Range(-100, -50));
        DOTween.To(() => mTransform.anchoredPosition, x => mTransform.anchoredPosition = x, mPos, .4f).SetEase(Ease.OutCirc).OnComplete(() => {
            foreach (GameObject temp in tempList)
            {
                if (!cardFx.activeSelf)
                {
                    SoundManager.instance.PlayUISound(SoundFX.CARD_DROP);
                    cardFx.SetActive(true);
                    Image img = cardFx.GetComponent<Image>();
                    img.DOFade(0, .5f).SetEase(Ease.InQuint).OnComplete(() => {
                        cardFx.SetActive(false);
                        img.color = Color.white;
                    });
                }
                temp.transform.SetParent(cardLine0.transform);
                temp.transform.DOScale(new Vector3(1f, 1f, 1f), .1f).OnComplete(()=> {

                });
            }

        });
        //mTransform.DOScale(new Vector2(0.8f, 0.8f),1.5f).SetEase(Ease.OutBack);

        //mTransform.DORotate(new Vector3(0, 0, Random.Range(-10, 10)), 1f);
    }

    private int crrH = 960 / 2;
    private int crrW = 1706 / 2;
    private Vector2 moveFrom(int sourceSlotId)
    {
        /*
        if (mySlotId >= 0)
        {
            sourceSlotId = sourceSlotId - mySlotId;
            sourceSlotId = sourceSlotId < 0 ? (sourceSlotId + 4) : sourceSlotId;
        }
        */
        switch (sourceSlotId)
        {
            case 0:
                return new Vector2(-773, -435);
            case 1:
                return new Vector2(676, -70);
            case 2:
                return new Vector2(-125, 241);
            case 3:
                return new Vector2(-787, -62);
        }
        return Vector2.zero;
    }

    public static BoardManager instance;
    public GameObject Table;
    void Awake()
    {
        Application.runInBackground = true;
        getInstance();
        Table.SetActive(true);
        //Assign enemies to a new List of Enemy objects.
        cardControlList = new List<CardControl>();
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
        //int[] ids = { 11, 15, 17, 19, 21, 50, 48, 18, 24, 36, 32, 31, 28};
        //flipCards(new List<int>(ids));

        StartCoroutine(LoadingControl.instance._start());
        listPlayerAtt = new string[4];
        loadData();


        //MoveCard();
        //cardLine0.GetComponent<RectTransform>().anchoredPosition = new Vector2(800, 600);
        //cardLine[1].GetComponent<RectTransform>().anchoredPosition = new Vector2(800, 700);

    }

    public void loadData()
    {
        //requestEnterPlace("Lobby." + CPlayer.gameName + "." + CPlayer.roomId + "." + CPlayer.tableToGoId);


        if (CPlayer.betAmtOfTableToGo.Contains('-'))
        {
            int amtId = (-1) * int.Parse(CPlayer.betAmtOfTableToGo);

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
                        tableTitleText.text = "TLMN - " + CPlayer.betAmtOfTableToGo + " Gold";
                       // Debug.Log("1======================================================================" + CPlayer.betAmtOfTableToGo);
                        break;
                    }


                }



                //App.trace("GET BET AMT DONE! BETAMT COUNT = " + count);
            });
        }
        else
        {
            tableTitleText.text = "TLMN - " + App.formatMoney(CPlayer.betAmtOfTableToGo) + " Gold";
            //Debug.Log("======================================================================" + CPlayer.betAmtOfTableToGo);
        }
        getTableDataEx();
        registerHandler();
        //requestEnterPlace("Lobby.1.0.12727");
    }

    private int mode = 1; // 0: Vào xem | 1: Vào chơi
    public void requestEnterPlace(string path)
    {
        CPlayer.clientTargetMode = mode;
        //roomId + "." + tableId
        var req_enterChessRoom = new OutBounMessage("ENTER_PLACE");
        req_enterChessRoom.addHead();

        req_enterChessRoom.writeAcii(path);
        req_enterChessRoom.writeString("125"); //Mật khẩu của phòng chơi
        req_enterChessRoom.writeByte(mode); // 0: Vào xem | 1: Vào chơi
                                            //req_enterChessRoom.print();


        App.ws.send(req_enterChessRoom.getReq(), delegate (InBoundMessage res)
        {
            //App.trace("ENTERED TABLE!");
            getTableDataEx();
            registerHandler();
        });


    }

    public GameObject[] player0CardList;//List card của ngời chơi có slotId = 0
    public Text playerAtt;
    public Sprite addPlayerIcon; //Icon thêm người vào bàn chơi
    private Dictionary<int, State> stateById;
    private bool isKicked = false;
    private string statusKick = "";
    private int currOwnerId = -1;
    private string[] listPlayerAtt;
    public void registerHandler()
    {
        #region //PHAN HANDLE DUNG CHUNG

        var req_KICK_PLAYER = new OutBounMessage("KICK_PLAYER");    //KÍCH NG CHƠI
        req_KICK_PLAYER.addHead();
        handelerCommand.Add("KICK_PLAYER");
        App.ws.sendHandler(req_KICK_PLAYER.getReq(), delegate (InBoundMessage res)
        {
            App.trace("KICK_PLAYER");
            int status = res.readByte();
            string content = res.readString();
            App.trace("status = " + status + "|content = " + content);
            if(status == -1)
            {
                App.showErr(content);
                return;
            }
            if(status== 2)
            {
                isKicked = true;
                isPlaying = false;
                statusKick = content;
                backToTableList();

            }

        });

        var req_ENTER_STATE = new OutBounMessage("ENTER_STATE");
        req_ENTER_STATE.addHead();
        handelerCommand.Add("ENTER_STATE");
        App.ws.sendHandler(req_ENTER_STATE.getReq(), delegate (InBoundMessage res)
        {
            App.trace("RCV [ENTER_STATE]");
            int stateId = res.readByte();
            //enterState(stateById[stateId],res);

        });

        var req_SET_TURN = new OutBounMessage("SET_TURN");
        req_SET_TURN.addHead();
        handelerCommand.Add("SET_TURN");
        App.ws.sendHandler(req_SET_TURN.getReq(), delegate (InBoundMessage res)
        {

            if (preTimeLeapImage != null)
                preTimeLeapImage.gameObject.SetActive(false);

            App.trace("RCV [SET_TURN]" + mySlotId);
            int slotId = res.readByte();
            int turnTimeOut = res.readShort();

            if (slotId == -2)
            {
                if (turnTimeOut > 0) //Bắt đầu ván mới
                {
                    showCountDown(turnTimeOut);
                } else //Hiện nút START!
                {

                }
                return;
            }

            int remainDuration = res.readShort();

            for(int i = 1; i < 3; i++)
            {
                playerButtonList[i].gameObject.SetActive((mySlotId > -1 && slotId == mySlotId) ? true : false);
            }
            if(slotId == mySlotId)
            {

                SoundManager.instance.PlayUISound(SoundFX.CARD_SET_TURN);
            }

            if (slotId >= 0)
            {
                if (mySlotId >= 0)
                {
                    slotId = slotId - mySlotId;
                    slotId = slotId < 0 ? (slotId + 4) : slotId;
                }

                run = true;
                preTimeLeapId = slotId;
                //time = remainDuration;
                time = turnTimeOut;
                curr = remainDuration;
                timeLeapList[slotId].SetActive(true);
                preTimeLeapImage = timeLeapList[slotId].GetComponent<Image>();
                preTimeLeapImage.fillAmount = 1;
                //App.trace("WAIT " + slotId + "|time = " + turnTimeOut + "dur = " + remainDuration);
            }
        });

        var req_SLOT_IN_TABLE_CHANGED = new OutBounMessage("SLOT_IN_TABLE_CHANGED");
        req_SLOT_IN_TABLE_CHANGED.addHead();
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
                currOwnerId = slotId;
            }

            Player player = new Player(detecSlotIdBySvrId(slotId), slotId, playerId, nickName, avatarId, avatar, isMale, chipBalance, starBalance, score, level, isOwner);

            slotId = getSlotIdBySvrId(slotId);

            if(nickName.Length == 0)    //Có thằng thoát khỏi bàn chơi
            {
                SoundManager.instance.PlayUISound(SoundFX.CARD_EXIT_TABLE);
                playerList.Remove(slotId);
                infoObjectList[slotId].SetActive(false);
                playerAvatarList[slotId].overrideSprite = addPlayerIcon;
                playerAvatarList[slotId].material = null;
                //playerAvatarList[slotId].transform.localScale = new Vector3(.5f, .5f, 1);
                exitsSlotList[slotId] = false;
                if (slotId != 0)
                {
                    backCards[slotId - 1].SetActive(false);
                    ownerList[slotId].SetActive(false);
                }
                if (playerList.Count < 2)
                {
                    StopCoroutine(preCoroutine);
                    countDownText.gameObject.SetActive(false);
                }
                return;
            }
            SoundManager.instance.PlayUISound(SoundFX.CARD_JOIN_TABLE);
            if (playerList.ContainsKey(slotId)) //Có thằng thay thế
            {
                playerList[slotId] =  player;
                PlayerManager.instance.setInfo(player, playerAvatarList[slotId], infoObjectList[slotId], balanceTextList[slotId], nickNameTextList[slotId], ownerList[slotId], player.SvSlotId == mySlotId);
                //playerAvatarList[slotId].transform.localScale = new Vector3(1f, 1f, 1);
                exitsSlotList[slotId] = true;
                return;
            }

            //Thêm bình thường
            playerList.Add(slotId, player);
            PlayerManager.instance.setInfo(player, playerAvatarList[slotId], infoObjectList[slotId], balanceTextList[slotId], nickNameTextList[slotId], ownerList[slotId], player.SvSlotId == mySlotId);
            //playerAvatarList[slotId].transform.localScale = new Vector3(1f, 1f, 1);
            exitsSlotList[slotId] = true;

            /*
            if (exitsSlotList[slotId] == true)
            {
                foreach (Player pl in playerList.Values.ToList())
                {
                    if (pl.SlotId == player.SlotId)
                    {
                        playerList.Remove(p);


                        playerList.Add(player);
                        PlayerManager.instance.setInfo(player, playerAvatarList[slotId], infoObjectList[slotId], balanceTextList[slotId], nickNameTextList[slotId], ownerList[slotId]);
                        //playerAvatarList[slotId].transform.localScale = new Vector3(1f, 1f, 1);
                        exitsSlotList[slotId] = true;
                        //backCards[slotId].SetActive(true);

                        break;
                    }

                }
                //exitsSlotList[slotId] = false;

            }
            else
            {
                playerList.Add(player);
                PlayerManager.instance.setInfo(player, playerAvatarList[slotId], infoObjectList[slotId], balanceTextList[slotId], nickNameTextList[slotId], ownerList[slotId]);
                //playerAvatarList[slotId].transform.localScale = new Vector3(1f, 1f, 1);
                exitsSlotList[slotId] = true;
                //backCards[slotId].SetActive(true);
            }
            */
            App.trace("PLAYER LIST COUNT = " + playerList.Count);

        });

        var req_OWNER_CHANGED = new OutBounMessage("OWNER_CHANGED");
        req_OWNER_CHANGED.addHead();
        handelerCommand.Add("OWNER_CHANGED");
        App.ws.sendHandler(req_OWNER_CHANGED.getReq(), delegate (InBoundMessage res)
        {
            int slotId = res.readByte();
            currOwnerId = slotId;
            slotId = detecSlotIdBySvrId(slotId);

            foreach(GameObject goj in ownerList)
            {
                if (goj.activeSelf)
                {
                    goj.SetActive(false);
                    return;
                }
            }
            if(slotId > -1)
            {
                ownerList[slotId].SetActive(true);
            }
        });

        var req_START_MATCH = new OutBounMessage("START_MATCH");
        req_START_MATCH.addHead();
        handelerCommand.Add("START_MATCH");
        App.ws.sendHandler(req_START_MATCH.getReq(), delegate (InBoundMessage res)
        {
            /*
             * TO DO:
             * 1. Set điểm
             * 2. Load lại Board Data
             * 3. So sánh mySlotId và 0( Lớn hơn 0 thì bđầu chơi| Nhỏ hơn 0 thì là view)
             * */
            isPlaying = true;
            App.trace("RCV [START_MATCH]");
            LoadingControl.instance.delCoins();
            /*
            foreach (Text temp in cardCountList)
            {
                temp.text = 13.ToString();
            }
            */
            SoundManager.instance.PlayUISound(SoundFX.CARD_START);
            if (mySlotId < 0)
            {
                foreach (GameObject obj in player0CardList)
                {
                    obj.SetActive(true);
                }
            }

            foreach (Transform g in cardLine0)
            {
                Destroy(g.gameObject, 0.2f);
            }

            foreach (Transform g in cardLine[1])
            {
                Destroy(g.gameObject, 0.2f);
            }
            foreach (Transform g in cardLine[2])
            {
                Destroy(g.gameObject, 0.2f);
            }

            foreach (Transform cardToShowList in showPlayerCardList)
            {
                foreach (Transform g in cardToShowList)
                {
                    if (g != null)
                        Destroy(g.gameObject, 0.2f);
                }
            }

            for (int i = 0; i < 4; i++)
            {
                listPlayerAtt[i] = "";
            }
            for(int i = 0; i < 13; i++)
            {
                myCardList[i].GetComponent<CardControl>().realSelected = false;
            }
            loadPlayerMatchPoint(res);

            loadBoardData(res,true);

            #region //3. So sánh mySlotId và 0( Lớn hơn 0 thì bđầu | Nhỏ hơn 0 thì là view)
            if (mySlotId >= 0)
            {

            }
            #endregion

            foreach (Text mText in moneyEarnedText)
            {
                mText.gameObject.SetActive(false);
            }
        });

        var req_GAMEOVER = new OutBounMessage("GAMEOVER");
        req_GAMEOVER.addHead();
        handelerCommand.Add("GAMEOVER");
        App.ws.sendHandler(req_GAMEOVER.getReq(), delegate (InBoundMessage res)
        {
            isPlaying = false;
            App.trace("GAMEOVER");
            List<int> winLs = new List<int>(), loseLs = new List<int>();
            myCardId.Clear();

            int count = res.readByte();
            for (int i = 0; i < count; i++)
            {
                int slotId = res.readByte();
                int grade = res.readByte();
                long earnValue = res.readLong();
                //App.trace("SLOT " + slotId + "|GRADE: " + grade + "|EARN: " + earnValue);




                if (slotId == mySlotId)
                {
                    if (earnValue > 0)
                    {
                        SoundManager.instance.PlayUISound(SoundFX.CARD_WIN);
                    }
                    else
                    {
                        SoundManager.instance.PlayUISound(SoundFX.CARD_LOSE);
                    }
                }

                slotId = detecSlotIdBySvrId(slotId);
                string t = App.listKeyText["GAME_DRAW"];//"Hòa\n";
                if (earnValue > 0)
                {
                    winLs.Add(slotId);
                    t = App.listKeyText["GAME_WIN"] + "/n"; //"Thắng\n";
                }
                else if(earnValue < 0)
                {
                    loseLs.Add(slotId);
                    t = App.listKeyText["GAME_LOSE"] + "/n";//"Thua\n";
                }

                moneyEarnedText[slotId].font = earnValue < 1 ? fontList[0] : fontList[1];
                moneyEarnedText[slotId].gameObject.SetActive(true);

                if (listPlayerAtt[slotId]!= null)
                {
                    if(listPlayerAtt[slotId].Length > 0)
                        t = listPlayerAtt[slotId] + "\n";
                }
                moneyEarnedText[slotId].text = (t) + (earnValue > 0 ? ("+" + earnValue) : earnValue.ToString());
                moneyEarnedText[slotId].transform.DOKill();
                if (grade == 1)
                    moneyEarnedText[slotId].transform.DOScale(new Vector3(1.2f, 1.2f, 1f), 1f).SetLoops(-1, LoopType.Yoyo);


            }
            string matchResult = res.readStrings();
            //App.trace("KET QUA = " + matchResult);
            //this.matchFinished(message, slotResults, matchResult, M.get('MATCH_RESULT'), form, buttons);
            #region //BAY TIỀN
            if (regQuit == false && isKicked == false && exited == false && CPlayer.preScene.Contains("TableList"))
            {
                SoundManager.instance.PlayUISound(SoundFX.CARD_TAI_XIU_BET);
                for (int i = 0; i < winLs.Count; i++)
                {
                    float x = 1706 * (playerAvatarList[winLs[i]].transform.position.x) / Screen.width;
                    float y = 960 * (playerAvatarList[winLs[i]].transform.position.y) / Screen.height;
                    Vector2 end = new Vector2(x, y);
                    //App.trace("end X = " + x + "|Y = " + y);
                    for (int j = 0; j < loseLs.Count; j++)
                    {
                        float xx = 1706 * (playerAvatarList[loseLs[j]].transform.position.x) / Screen.width;
                        float yy = 960 * (playerAvatarList[loseLs[j]].transform.position.y) / Screen.height;
                        Vector2 start = new Vector2(xx, yy);
                        //App.trace("start X = " + xx + "|Y = " + yy);

                        LoadingControl.instance.flyCoins("line", 10, start, end);
                    }
                }
            }

            #endregion

        });

        #endregion
        #region //PHAN HANDER CHO GAME BAI
        var req_SET_PLAYER_ATTR = new OutBounMessage("SET_PLAYER_ATTR");
        req_SET_PLAYER_ATTR.addHead();
        handelerCommand.Add("SET_PLAYER_ATTR");
        App.ws.sendHandler(req_SET_PLAYER_ATTR.getReq(), delegate (InBoundMessage res)
        {
            App.trace("SET_PLAYER_ATTR");
            int slotId = res.readByte();
            string icon = res.readAscii();
            string content = res.readAscii();
            int action = res.readByte();
            App.trace("????? icon = " + icon + "|action = " + action + "|content" + content);

            slotId = detecSlotIdBySvrId(slotId);
            if (icon == "pass_turn")    //Gắn với action = 0 | 1
                return;
            if (action == -2)
            {
                if (icon == "bet" || icon =="")
                    return;
                if (icon == "freeze" || icon == "white_win")
                    listPlayerAtt[slotId] = getAttByIcon(icon);
                else
                {
                    SoundManager.instance.PlayUISound(SoundFX.CARD_START_MATCH);
                    playerAtt.text = getAttByIcon(icon);
                    playerAtt.gameObject.SetActive(true);
                    playerAtt.transform.SetAsLastSibling();
                    StartCoroutine(hideBitmapFont());
                }
                return;
            }
            if(action == -1)    //Bo luot
            {
                return;
            }
            //Còn lại action = 0
            if(icon == "freeze" || icon == "white_win")
            {
                listPlayerAtt[slotId] = getAttByIcon(icon);
            }
            else
            {
                SoundManager.instance.PlayUISound(SoundFX.CARD_START_MATCH);
                playerAtt.text = getAttByIcon(icon);
                playerAtt.gameObject.SetActive(true);
                playerAtt.transform.SetAsLastSibling();
                StartCoroutine(hideBitmapFont());
            }


            /*
            if(icon == "freeze" || icon == "white_win")
            {
                listPlayerAtt[slotId] = getAttByIcon(icon);
                App.trace(slotId + " bị cóng");
                return;
            }


            if (action == -2 || action == 0)
            {
                playerAtt.text = getAttByIcon(icon);
                playerAtt.gameObject.SetActive(true);
                playerAtt.transform.SetAsLastSibling();
                StartCoroutine(hideBitmapFont());
                return;
            }
            //slotId = detecSlotIdBySvrId(slotId);

            */

        });

        var req_SHOW_PLAYER_CARD = new OutBounMessage("SHOW_PLAYER_CARD");
        req_SHOW_PLAYER_CARD.addHead();
        handelerCommand.Add("SHOW_PLAYER_CARD");
        App.ws.sendHandler(req_SHOW_PLAYER_CARD.getReq(), delegate (InBoundMessage res)
        {
            //playerButtonList[0].gameObject.SetActive(false);
            int slotId = res.readByte();
            List<int> ids = new List<int>();
            ids = res.readBytes();
            slotId = detecSlotIdBySvrId(slotId);
            App.trace("SHOW CARDS OF " + slotId);
            foreach (GameObject g in player0CardList)
            {
                g.SetActive(false);
            }
            foreach (GameObject g in myCardList)
            {
                g.SetActive(false);
            }
            foreach (GameObject g in backCards)
            {
                g.SetActive(false);
            }
            int halfLength = ids.Count / 2;
            Vector2 mPos = Vector2.zero;
            if (slotId == 0)
            {
                mPos = new Vector2(-60, 0);
            }
            else if (slotId == 2)
            {
                mPos = new Vector2(-60, 0);
            } else
            {
                mPos = new Vector2(0, 60);
            }


            for (int i = ids.Count - 1; i >= 0; i--)
            {
                //var face = Cards.faces[i];
                GameObject temp = Instantiate(cardToInstantiate, showPlayerCardList[slotId], false) as GameObject;
                //App.trace("FUCK U BITCH: " + cardIds[i]);
                Image mImage = temp.GetComponent<Image>();
                mImage.overrideSprite = faces[ids[i]];
                mImage.color = winLoseColor[1];
                RectTransform myTransform = temp.GetComponent<RectTransform>();
                //mTransform.localPosition = Vector3.zero;
                myTransform.localScale = slotId == 0 ? new Vector3(.9f, .9f, .9f) : new Vector3(.8f, .8f, .8f);
                myTransform.pivot = showPlayerCardList[slotId].GetComponent<RectTransform>().pivot;

                myTransform.anchoredPosition = mPos * (-halfLength);
                //myTransform.SetAsLastSibling();
                //, .5f
                DOTween.To(() => myTransform.anchoredPosition, x => myTransform.anchoredPosition = x, mPos * (i - halfLength), .5f);

            }

            // App.trace("SHOW_PLAYER_CARD id = " + slotId.ToString());

        });
        var req_CLEAR_CARDS = new OutBounMessage("CLEAR_CARDS");
        req_CLEAR_CARDS.addHead();
        handelerCommand.Add("CLEAR_CARDS");
        App.ws.sendHandler(req_CLEAR_CARDS.getReq(), delegate (InBoundMessage res)
        {
            App.trace("CLEAR_CARDS");
            try
            {
                foreach (Transform g in cardLine0)
                {
                    if (g != null)
                        Destroy(g.gameObject, 0.2f);
                }

                foreach (Transform g in cardLine[1])
                {
                    if (g != null)
                        Destroy(g.gameObject, 0.2f);
                }
                foreach (Transform g in cardLine[2])
                {
                    if (g != null)
                        Destroy(g.gameObject, 0.2f);
                }
            }
            catch
            {

            }

        });
        var req_SET_CARDS = new OutBounMessage("SET_CARDS");
        req_SET_CARDS.addHead();
        handelerCommand.Add("SET_CARDS");
        App.ws.sendHandler(req_SET_CARDS.getReq(), delegate (InBoundMessage res)
        {


            App.trace("SET_CARDS");
            int slotId = res.readByte();
            int lineid = res.readByte();
            int cardCount = res.readByte();

            List<int> ids = res.readBytes();
            //CardUtils.svrIdsToIds(ids);

            if (isSorted == true)
            {
                StartCoroutine(_resort(ids));
            }
            isSorted = false;


        });
        var req_SELECT_CARDS = new OutBounMessage("SELECT_CARDS");
        req_SELECT_CARDS.addHead();
        handelerCommand.Add("SELECT_CARDS");
        App.ws.sendHandler(req_SELECT_CARDS.getReq(), delegate (InBoundMessage res)
        {
            App.trace("SELECT_CARDS");
        });
        var req_MOVE = new OutBounMessage("MOVE");
        req_MOVE.addHead();
        handelerCommand.Add("MOVE");
        App.ws.sendHandler(req_MOVE.getReq(), delegate (InBoundMessage res)
        {
            List<int> ids = new List<int>();
            ids = res.readBytes();
            //CardUtils.svrIdsToIds(ids);

            int sourceSlotId = res.readByte();
            int sourceLineId = res.readByte() - 1;
            int targetSlotId = res.readByte();
            int targetLineId = res.readByte() - 1;
            int targetIndex = res.readByte();
            App.trace("- sourceSlotId = " + sourceSlotId + "|targetIndex = " + targetIndex);
            //App.trace(string.Format("MOVE = {0} | sourceSlotId = {1} | sourceLineId = {2}| targetSlotId = {3}| targetLineId = {4} | targetIndex = {5}| CON DATA =\n",
            //ids.Count, sourceSlotId, sourceLineId, targetSlotId, targetLineId, targetIndex));

            MoveCard(ids, sourceSlotId, sourceLineId, targetSlotId, targetLineId);
            if (sourceSlotId == mySlotId)
            {
                for (int i = 0; i < ids.Count; i++)
                {
                    for (int j = 0; j < myCardId.Count; j++)
                    {
                        if (ids[i] == myCardId[j])
                        {
                            myCardList[j].transform.DOScale(new Vector3(0f, 1f, 1f), .2f).OnComplete(() => {
                                myCardList[j].SetActive(false);
                                myCardList[j].GetComponent<CardControl>().realSelected = false;
                            });
                            //myCardList[j].GetComponent<Image>().overrideSprite = back;
                            break;
                        }
                    }
                }
                return;
            }
            sourceSlotId = detecSlotIdBySvrId(sourceSlotId);
            if (sourceSlotId != 0)
                cardCountList[sourceSlotId - 1].text = (int.Parse(cardCountList[sourceSlotId - 1].text) - ids.Count).ToString();
            else
            {

                for (int i = 0; i < player0CardList.Length; i++)
                {
                    if (player0CardList[i].activeSelf == true)
                    {
                        for (int j = 0; j < ids.Count; j++)
                        {
                            player0CardList[i + j].SetActive(false);
                        }
                        break;
                    }

                }

            }

            run = false;
            timeLeapList[sourceSlotId].SetActive(false);


        });
        #endregion
        /*
        var reqBroadCast = new OutBounMessage("BROADCAST");
        reqBroadCast.addHead();
        App.ws.sendHandler(reqBroadCast.getReq(), delegate (InBoundMessage resBroadCast)
        {
            string nickName = resBroadCast.readAscii();
            string content = resBroadCast.readString();
            string emoticon = resBroadCast.readAscii();
            int messageType = resBroadCast.readByte();
            App.trace("BROADCAST content = " + content);
        });*/
    }

    IEnumerator hideBitmapFont()
    {
        yield return new WaitForSeconds(3f);
        playerAtt.gameObject.SetActive(false);
    }

    private Image preTimeLeapImage;
    private int preTimeLeapId;
    public float time = 1;
    private bool run = false;
    private float curr = 0;
    void Update()
    {
        curr += Time.deltaTime;
        if (run)
            preTimeLeapImage.fillAmount = (time - curr) / time;
    }

    #region //LOAD TABLE DATA
    public void getTableDataEx()
    {
        if (CPlayer.clientTargetMode != CPlayer.clientCurrentMode)
        {

            changeClientMode(CPlayer.clientTargetMode);
        }

        App.trace("===========\nSTART getTableDataEx");
        var req_GET_TABLE_DATA_EX = new OutBounMessage("GET_TABLE_DATA_EX");
        req_GET_TABLE_DATA_EX.addHead();
        req_GET_TABLE_DATA_EX.writeAcii("");
        //req_getTableChange.print();
        App.ws.send(req_GET_TABLE_DATA_EX.getReq(), delegate (InBoundMessage res)
        {
            App.trace("GET_TABLE_DATA_EX...");

            loadStateData(res);
            loadTableData(res);
            loadPlayerMatchPoint(res);
            loadBoardData(res);

        });


    }

    public void changeClientMode(int mode)
    {
        OutBounMessage req = new OutBounMessage("SET_CLIENT_MODE");
        req.addHead();
        req.writeByte(mode);
        App.ws.send(req.getReq(), delegate (InBoundMessage res)
        {
            CPlayer.clientCurrentMode = mode;
            getTableDataEx();
        });
    }

    /// <summary>
    /// Trạng thái bàn chơi
    /// </summary>
    /// <param name="res"></param>
    public void loadStateData(InBoundMessage res)
    {
        int count = res.readByte();
        for (int i = 0; i < count; i++)
        {
            res.readByte();
            res.readAscii();
            res.readByte();

            int commandCount = res.readByte();
            for (int j = 0; j < commandCount; j++)
            {
                res.readByte();
                res.readAscii();
                res.readString();

                res.readByte();
                res.readByte();
            }
        }
        res.readByte();
    }
    private Dictionary<int,Player> playerList;
    private int mySlotId = -1;
    public Image[] playerAvatarList;
    public GameObject[] ownerList;
    public GameObject[] infoObjectList;
    public Text[] nickNameTextList, balanceTextList, cardCountList;
    public GameObject[] backCards; //Cac quan bai` up cua 3 ng choi con lai
    public RectTransform[] playerListTransform; //Vi tri cua nguoi choi trong ban;
    private bool[] exitsSlotList = { false, false, false, false };//Cac vi tri da co ng ngoi`
    public void loadTableData(InBoundMessage res)
    {
        playerList = new Dictionary<int, Player>();

        mySlotId = res.readByte();
        App.trace("MY SLOT ID = " + mySlotId);
        isPlaying = res.readByte() == 1;

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
                currOwnerId = slotId;
            }
            Player player = new Player(detecSlotIdBySvrId(slotId), slotId, playerId, nickName, avatarId, avatar, isMale, chipBalance, starBalance, score, level, isOwner);

            slotId = detecSlotIdBySvrId(slotId);


            playerList.Add(slotId, player);

            PlayerManager.instance.setInfo(player, playerAvatarList[slotId], infoObjectList[slotId], balanceTextList[slotId], nickNameTextList[slotId], ownerList[slotId], player.SvSlotId == mySlotId);
            exitsSlotList[slotId] = true;

        }
        //App.trace("So slot " + count);
        int currentTurnSlotId = res.readByte();
        if (mySlotId > -1)
        {
            if (currentTurnSlotId == mySlotId)
            {
                for(int i = 1; i < 3; i++ )
                {

                    playerButtonList[i].gameObject.SetActive(true);
                }
            }
            /*
            else if(isPlaying)
            {
                playerButtonList[0].gameObject.SetActive(true);
            }
            */
        }

        int currentTime = res.readShort();
        //currentDurationTime =

        int slotRemainDuration = res.readShort();
        var currentState = res.readByte();
        if (currentTurnSlotId < 0)
        {
            return;
        }
        currentTurnSlotId = detecSlotIdBySvrId(currentTurnSlotId);
        run = true;
        preTimeLeapId = currentTurnSlotId;
        //time = remainDuration;
        time = currentTime;
        curr = slotRemainDuration;
        timeLeapList[currentTurnSlotId].SetActive(true);
        preTimeLeapImage = timeLeapList[currentTurnSlotId].GetComponent<Image>();
        preTimeLeapImage.fillAmount = (float)slotRemainDuration / time;

    }

    public void loadPlayerMatchPoint(InBoundMessage res)
    {
        int count = res.readByte();
        for (int i = 0; i < count; i++)
        {
            int slotId = res.readByte();
            int point = res.readInt();
            //App.trace("slotId = " + slotId + ", point = " + point);
        }
        //App.trace("this count = " + count);
    }
    public void loadBoardData(InBoundMessage res, bool chiaBai = false)
    {
        int slotCount = res.readByte();
        //App.trace("slot count = " + slotCount);
        for (int i = 0; i < slotCount; i++)
        {
            int slotId = res.readByte();
            //App.trace("+ BOARD DATA slotID = " + slotId);
            int lineCount = res.readByte();
            for (int j = 0; j < lineCount; j++)
            {
                int cardLineId = res.readByte();
                int cardCount = res.readByte();

                if (cardCount < 0)   //ĐÂY LÀ CÁC QUÂN BÀI CỦA MÌNH
                {
                    List<int> ids = new List<int>();
                    ids = res.readBytes();
                    CardUtils.svrIdsToIds(ids);
                    if (mySlotId >= 0 && slotId == mySlotId && isPlaying)
                    {
                        //isPlaying = true;
                        if (chiaBai == false)
                        {
                            setMyCard(ids);
                        }
                        else
                        {

                            flipCards(ids);
                        }

                    }

                    continue;
                }
                if (slotId == 0 && mySlotId < 0)  //Các quân bài của thằng slotId = 0
                {
                    for (int k = 0; k < player0CardList.Length; k++)
                    {
                        player0CardList[k].SetActive(true);
                    }
                    continue;
                }
                slotId = detecSlotIdBySvrId(slotId);

                if (slotId > 0 && cardCount > 0 && chiaBai == false)
                {
                    //App.trace("--- cardCount = " + cardCount);

                    backCards[slotId - 1].SetActive(true);
                    cardCountList[slotId - 1].text = cardCount.ToString();
                    continue;
                }



                //App.trace(cardLineId.ToString() + "|so bai = " + cardCount + "|cardLineId = " + cardLineId + "|name = " + backCards[j].transform.GetChild(0).name);
            }
        }
    }
    #endregion
    public List<GameObject> myCardList;
    private static List<int> myCardId = new List<int>();
    public void setMyCard(List<int> arr)
    {
        myCardId.Clear();
        for (int i = 0; i < arr.Count; i++)
        {
            myCardList[i].SetActive(true);
            myCardList[i].transform.localScale = Vector3.one;
            myCardList[i].GetComponent<Image>().overrideSprite = faces[arr[i]];
            //myCardId[i] = arr[i];
            myCardId.Add(arr[i]);
        }
        if (arr.Count < 13)
        {
            for (int i = arr.Count; i < 13; i++)
            {
                myCardList[i].SetActive(false);
            }
        }
    }

    public List<CardControl> cardControlList;

    public void FireCard()
    {
        List<int> tmpList = new List<int>();
        for (int i = 0; i < myCardList.Count; i++)
        {
            if (myCardList[i].GetComponent<CardControl>().realSelected)
            {
                tmpList.Add(myCardId[i]);
                //myCardList[i].GetComponent<CardControl>().isSelected = false;
                //RectTransform rtf = myCardList[i].GetComponent<RectTransform>();
                //rtf.anchoredPosition = new Vector2(rtf.anchoredPosition.x,rtf.anchoredPosition.y - 50);
            }
        }

        //MoveCard(tmpList, 0,-1,-1,-1);

        OutBounMessage req = new OutBounMessage("FIRE_CARD");
        req.addHead();
        req.writeBytes(tmpList);
        App.ws.send(req.getReq(), delegate (InBoundMessage res) {
            App.trace("CARD FIRED!");
        });
        //req.writeBytes(srvCardIds);
    }
    public void PassTurn()
    {

        OutBounMessage req = new OutBounMessage("PASS_TURN");
        req.addHead();
        App.ws.send(req.getReq(), delegate (InBoundMessage res) {
            App.trace("PASS TURN!");
        });
    }
    #region //CLASS PLAYER
    public class Player
    {
        string nickName, avatar;
        int slotId, avatarId, level, svSlotId;
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
            this.SvSlotId = svSlotId;
            this.SlotId = slotId;
            this.PlayerId = playerId;
            this.NickName = nickName;
            this.AvatarId = avatarId;
            this.IsMale = isMale;
            this.ChipBalance = chipBalance;
            this.StarBalance = starBalance;
            this.Score = score;
            this.Level = level;
            this.IsOwner = isOwner;
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

    private Dictionary<string, int> stateButtonByCommandCode;
    private Dictionary<string, int> stateButtonByPosition;
    private State state;
    private void enterState(State state, InBoundMessage res)
    {
        stateButtonByCommandCode = new Dictionary<string, int>();
        stateButtonByPosition = new Dictionary<string, int>();
        if (state == null)
            return;
        this.state = state;
        foreach (int position in state.commandsByPosition.Keys)
        {
            List<StateCommand> commands = state.commandsByPosition[position];
            stateButtonByPosition.Add(position.ToString(), createStateButton(commands));

        }

        /*
         * if (stateButtonByCommandCode != null)
		{
			for (button in stateButtonByCommandCode.iterator())
				button.free();
		}

		if (stateButtonByPosition != null)
		{
			for (button in stateButtonByPosition.iterator())
				button.free();
		}
         * */
    }

    private int createStateButton(List<StateCommand> commands)
    {
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
        return detecSlotIdBySvrId(slotId);
    }

    private int detecSlotIdBySvrId(int slotId)
    {
        int temp = slotId;
        if (mySlotId > -1)
        {
            temp = temp - mySlotId;
            temp = temp < 0 ? (temp + 4) : temp;
        }
        return temp;
    }
    private int detecSvrBySlotId(int slotId)
    {
        int temp = slotId;
        if (mySlotId < 0)
        {
            temp = temp + mySlotId;
            temp = temp > 4 ? (temp - 4) : temp;
        }
        return temp;
    }
    //Khi người chơi sắp xếp lại bài;
    public static void changeCardInMyCardList(int preId, int newId)
    {
        if (preId < myCardId.Count && preId > -1)
        {
            int temp = myCardId[preId];
            myCardId.RemoveAt(preId);
            myCardId.Insert(newId, temp);
            App.trace("NEW ID = " + newId + "preId = " + preId);
            instance.changeSelectedCard(preId, newId);
        }

    }

    public void changeSelectedCard(int preID, int newID)
    {
        if(preID != newID)
        {
            GameObject goj = myCardList[preID];
            myCardList.RemoveAt(preID);
            myCardList.Insert(newID, goj);
        }

        for (int i = 0; i < myCardList.Count; i++)
        {
            if(myCardList[i].GetComponent<CardControl>().realSelected == true)
                myCardList[i].GetComponent<CardControl>().pushCardUp();

        }
        // myCardList[1].GetComponent<RectTransform>().anchoredPosition += new Vector2(0, 100);
    }

    public void _fire()
    {
        int[] arr = { 1, 2, 4 };
        int temp = -1;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < myCardList.Count; j++) {
                if (myCardList[j].GetComponent<CardControl>().realSelected == true)
                {
                    temp++;
                    GameObject goj = Instantiate(myCardList[i], myCardList[i].transform, false) as GameObject;
                    RectTransform mTransform = goj.GetComponent<RectTransform>();
                    goj.GetComponent<Image>().overrideSprite = faces[i];
                    mTransform.sizeDelta = new Vector2(128, 176);
                    mTransform.SetParent(boardHolder.transform);
                    DOTween.To(() => mTransform.anchoredPosition, x => mTransform.anchoredPosition = x, new Vector2(UnityEngine.Random.Range(720, 750) + 100 * temp, UnityEngine.Random.Range(600, 620)), .5f).SetEase(Ease.OutCirc).OnComplete(() => {

                    });
                    mTransform.DORotate(new Vector3(1, 1, UnityEngine.Random.Range(-5f, 5f)), .5f);
                    //break;
                }
            }
        }

    }
    private string getAttByIcon(string icon) {
        switch (icon)
        {
            case "freeze":
                return App.listKeyText["BAND_CONG"];//"CÓNG";
            case "3_sequence_pair":
                return App.listKeyText["BAND_3DOITHONG"];//"3 đôi thông";
            case "4_sequence_pair":
                return App.listKeyText["BAND_4DOITHONG"]; //"4 đôi thông";
            case "finish_by_3_of_spade_block":
                return App.listKeyText["GAME_BLOCK_3B_FINISH"]; //"Chặn kết 3 bích";
            case "finish_by_3_of_spade_success":
                return App.listKeyText["GAME_3BICH_FINISH"]; //"Kết 3 bích";
            case "quad_pair":
                return App.listKeyText["BAND_TU_QUY"];//"Tứ quý";
            case "white_win":
                return App.listKeyText["WHITE_WIN"]; //"THẮNG TRẮNG";
        }
        return "";
    }

    private Coroutine preCoroutine = null;
    private void showCountDown(int timeOut)
    {

        preCoroutine = StartCoroutine(_showCountDOwn(timeOut));
    }

    IEnumerator _showCountDOwn(int timeOut)
    {
        if (regQuit)
        {
            backToTableList();
            yield break;
        }
        float mTime = timeOut - 2;
        countDownText.text = App.listKeyText["GAME_PREPARE"]; //"CHUẨN BỊ";
        countDownText.gameObject.SetActive(true);
        yield return new WaitForSeconds(.5f);
        countDownText.gameObject.SetActive(false);
        yield return new WaitForSeconds(.5f);
        while (mTime > 0f)
        {
            countDownText.text = mTime.ToString();
            countDownText.gameObject.SetActive(true);
            yield return new WaitForSeconds(.5f);
            countDownText.gameObject.SetActive(false);
            yield return new WaitForSeconds(.5f);
            mTime -= 1f;
        }

        countDownText.text = App.listKeyText["GAME_START"];//"BẮT ĐẦU";
        countDownText.gameObject.SetActive(true);
        yield return new WaitForSeconds(.5f);
        countDownText.gameObject.SetActive(false);
        //yield break;
    }
    private bool isSorted = false;
    public void sortCard()
    {
        OutBounMessage req = new OutBounMessage("SORT_CARD");
        req.addHead();
        App.ws.send(req.getReq(), delegate (InBoundMessage res) {
            App.trace("CARD SORTED!");
            isSorted = true;
        });
    }

    public void backToTableList()
    {
        if (!isPlaying || myCardId.Count < 1)
        {
            DOTween.PauseAll();
            run = false;
            delAllHandle();
            if (isKicked == false){
                EnterParentPlace(delegate() {
                    if (LoadingControl.instance.chatBox.activeSelf)
                    {
                        LoadingControl.instance.chatBox.SetActive(false);

                    }
                    StartCoroutine(openTable());
                });
            }
            else
            {
                if (LoadingControl.instance.chatBox.activeSelf)
                {
                    LoadingControl.instance.chatBox.SetActive(false);
                }
                StartCoroutine(openTable());
            }

        }
    }

    private bool exited = false;
    IEnumerator openTable()
    {
        /*==========THAY===================
        exited = true;
        CPlayer.preScene = isKicked ? "TLMNK" : "TLMN";

        yield return new WaitForSeconds(0.5f);

        SceneManager.LoadScene("TableList");

        //LobbyControll.instance.LobbyScene.SetActive(true);
        slideSceneAnim.Play("TableAnimation");
        //yield return async;

        Destroy(gameObject,1f);

        yield return new WaitForSeconds(0.5f);
        LoadingControl.instance.loadingScene.SetActive(false);
        if (isKicked == true)
            App.showErr(statusKick);
        //yield break;
        =============THAY======*/

        exited = true;
        CPlayer.preScene = isKicked ? "TLMNK" : "TLMN";
        LoadingUIPanel.Show();
        SceneManager.LoadScene("TableList");
        yield return new WaitForSeconds(0.05f);
    }

    private void EnterParentPlace(Action callBack)
    {
        CPlayer.clientCurrentMode = 0; // modeview
        CPlayer.clientTargetMode = 0;//mode view
        //LoadingControl.instance.loadingScene.SetActive(true);
        LoadingUIPanel.Show();
        var req = new OutBounMessage("ENTER_PARENT_PLACE");
        req.addHead();
        req.writeString("");
        req.writeByte(CPlayer.clientCurrentMode);
        App.ws.send(req.getReq(), delegate (InBoundMessage res)
        {
            if (callBack != null)
                callBack();
        });
    }
    private List<string> handelerCommand = new List<string>();
    private void delAllHandle()
    {

        foreach(string t in handelerCommand)
        {
            //App.trace(t);
            var req = new OutBounMessage(t);
            req.addHead();
            App.ws.delHandler(req.getReq());
        }
    }

    public bool regQuit =false;
    public void showNoti()
    {
        if (!isPlaying || (isDividingCards == false &&  myCardId.Count < 1))
        {
            LoadingControl.instance.delCoins();
            backToTableList();
            return;
        }
        this.regQuit = !regQuit;
        notiText.GetComponentInChildren<Text>().text = !regQuit ? App.listKeyText["GAME_BOARD_EXIT_CANCEL"] : App.listKeyText["GAME_BOARD_EXIT"];//"Bạn đã hủy đăng ký rời bàn." : "Bạn đã đăng ký rời bàn.";
        btnBackImg.transform.localScale = new Vector2(regQuit ? -1 : 1,1);
        notiText.SetActive(true);
        StartCoroutine(_showNoti());
    }
    IEnumerator _showNoti()
    {

        yield return new WaitForSeconds(2f);
        notiText.SetActive(false);
        yield break;
    }

    public void showPlayerInfo(int slotIdToShow)
    {
        //slotIdToShow = detecSlotIdBySvrId(slotIdToShow);
        if (exitsSlotList[slotIdToShow] == false)
        {
            LoadingControl.instance.sendInvite();
            return;
        }
        Player pl = null;
        string typeShowInfo = "";
        if (slotIdToShow == 0)
        {
            LoadingControl.instance.showPlayerInfo(CPlayer.nickName, (long)CPlayer.chipBalance, (long)CPlayer.manBalance, CPlayer.id, true, playerAvatarList[slotIdToShow].overrideSprite, "me");
            return;
        }
        if (mySlotId == currOwnerId && slotIdToShow != 0)
            typeShowInfo = "kick";
        foreach (Player mpl in playerList.Values.ToList())
        {
            if(mpl.SlotId == slotIdToShow)
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
                    LoadingControl.instance.showPlayerInfo(pl.NickName, pl.ChipBalance, pl.StarBalance, pl.PlayerId, isCPlayerFriend, playerAvatarList[slotIdToShow].overrideSprite, typeShowInfo);
                });
                return;
            }
        }

    }

    private void _kick()
    {
        var reqEnterChild = new OutBounMessage("ENTER_CHILD_PLACE");
        reqEnterChild.addHead();
        reqEnterChild.writeAcii("0");    //Id game
        reqEnterChild.writeString(""); //Pass
        reqEnterChild.writeByte(0);
        App.ws.send(reqEnterChild.getReq(), delegate (InBoundMessage resEnterChild) {
            resEnterChild.readByte();
            backToTableList();
        });
        var reqZone = new OutBounMessage("LIST_ZONE_ROOM");
        reqZone.addHead();
        App.ws.send(reqZone.getReq(), delegate (InBoundMessage res)
        {
            int count = res.readByte();
            App.trace("[[[[[[[[[[[[[[[[[[[[[[ = " + count);
        });
        //backToTableList();
    }


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
        foreach(Player pl in playerList.Values.ToList())
        {
            if(pl.NickName == sender && pl.SlotId > -1)
            {
                if(emoSprite == null) {
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
                    chatImoIco[pl.SlotId].transform.DOScale(1.2f, 4f).SetEase(Ease.OutBounce).OnComplete(()=> {
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

    #region //MOI NG CHOI
    [Header("===MỜI NG CHƠI")]
    public GameObject invitePanel;
    public void openInvitePanel(bool toOpen)
    {
        RectTransform rtf = invitePanel.GetComponent<RectTransform>();

        if (!toOpen)
        {
            DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, new Vector2(0, -960), .35f).OnComplete(() => {
                invitePanel.SetActive(false);
            });
            return;
        }

        var req = new OutBounMessage("LIST_ZONE_PLAYER");
        req.addHead();
        App.ws.send(req.getReq(), delegate (InBoundMessage res)
        {
            int count = res.readByte();
            for (int i = 0; i < count; i++)
            {
                long playerId = res.readLong();
                string nickName = res.readAscii();
                bool isMale = res.readByte() == 1;
                long score = res.readLong();
                var level = res.readShort();
                var gradeStatus = res.readByte(); // 1: tăng, 0: giữ. -1: giảm
                var online = res.readByte() == 1;
                var avatar = res.readAscii();


            }
        });
    }
    #endregion

    public void balanceChanged(int slotId, long chip, long man)
    {
        slotId = detecSlotIdBySvrId(slotId);
        balanceTextList[slotId].text = slotId == 0 ? App.formatMoney(chip.ToString()):App.formatMoneyAuto(chip);
    }

    [Header("======COINS=====")]
    public GameObject coin;
    public void flyCoins(int type)
    {
        if(type == 1)
        {
            int coinNum = 20;
            Vector2 startPos = new Vector2(500, 500);
            Vector2 posToFly = Vector2.zero;
            int radius = 500;


            for (int i = 0; i < 20; i++)
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

                DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, startPos + posToFly, .5f + i * .1f).SetEase(Ease.OutCubic).OnComplete(() => {
                    rtf.DOScale(.25f, .1f).OnComplete(() =>
                    {
                        Destroy(mCoind);
                    });
                });
            }

        }
        else
        {
            StartCoroutine(_flyCoins());
        }





    }

    private IEnumerator _flyCoins()
    {
        for (int i = 0; i < 20; i++)
        {
            GameObject mCoind = Instantiate(coin, coin.transform.parent, false);
            mCoind.SetActive(true);
            RectTransform rtf = mCoind.GetComponent<RectTransform>();
            Vector2 pos = new Vector2(UnityEngine.Random.RandomRange(-50, 50), UnityEngine.Random.RandomRange(-50, 50));
            rtf.anchoredPosition = new Vector2(500, 500);

            Vector2 endPos = new Vector2(UnityEngine.Random.RandomRange(100, 900), UnityEngine.Random.RandomRange(100, 400));

            rtf.DOJump(endPos, 400, 1,1f).SetEase(Ease.OutCubic).OnComplete(() => {
                rtf.DOScale(.25f, .1f).OnComplete(() =>
                {
                    Destroy(mCoind);
                });
            }); ;
            yield return new WaitForSeconds(.05f);
        }
    }

    private bool isDividingCards = false;
    public void flipCards(List<int> ids)
    {
        /*
        RectTransform rtf = myCardList[stt].GetComponent<RectTransform>();
        rtf.DORotate(new Vector3(0, 90, 0), .25f).OnComplete(() =>
        {
            myCardList[stt].GetComponent<Image>().overrideSprite = faces[cardId];
            rtf.DORotate(new Vector3(0, 0, 0), .25f);
        });
        */

        isDividingCards = true;
        for (int i = 1; i < 4; i++)
        {
            if(exitsSlotList[i] == true)
                StartCoroutine(_divideCards(i));

        }

        StartCoroutine(_flipCards(ids));


    }
    private IEnumerator _flipCards(List<int> ids)
    {
        myCardId.Clear();
        List<Vector2> ls = new List<Vector2>();
        for(int i = 0; i < 13; i++)
        {
            myCardList[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(myCardList[i].GetComponent<RectTransform>().anchoredPosition.x, -88);
            ls.Add(myCardList[i].GetComponent<RectTransform>().anchoredPosition);
            myCardList[i].transform.localScale = new Vector2(0, 1);
            myCardList[i].SetActive(true);
            myCardList[i].GetComponent<Image>().overrideSprite = back;
        }

        yield return new WaitForSeconds(.1f);
        for (int i = 0; i < 13; i++)
        {
            int tmp = i;
            //myCardList[tmp].SetActive(true);
            RectTransform rtf = myCardList[tmp].GetComponent<RectTransform>();
            Vector2 pos = ls[tmp];
            //myCardList[tmp].SetActive(true);
            //App.trace("- x = " + pos.x);
            rtf.anchoredPosition = new Vector2(624,363);
            rtf.localScale = Vector2.one;
            yield return new WaitForSeconds(.1f);
            DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, pos, .125f).OnComplete(() => {

            });
            //yield return new WaitForSeconds(.1f);
        }
        for(int i = 0; i < 13; i++)
        {
            int tmp = i;
            RectTransform rtf = myCardList[tmp].GetComponent<RectTransform>();
            Vector2 pos = ls[tmp];
            rtf.DORotate(new Vector3(0, 90, 0), .125f).OnComplete(() =>
            {
                myCardList[tmp].GetComponent<Image>().overrideSprite = faces[ids[i]];
                myCardId.Add(ids[i]);
                rtf.DORotate(new Vector3(0, 0, 0), .125f);
            });
            yield return new WaitForSeconds(.125f);
        }
        yield return new WaitForSeconds(1.5f);
        //playerButtonList[0].gameObject.SetActive(true);
        isDividingCards = false;
    }

    /// <summary>
    /// Chia bài về cho từng thắngf
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    private IEnumerator _divideCards(int i)
    {
        List<Vector2> vecList = new List<Vector2>();
        vecList.Add(new Vector2(1706 - 215 - 102, 960 / 2 + 102 - 71));
        vecList.Add(new Vector2(1706 / 2 + 72 - 51, 960 - 9 - 142));
        vecList.Add(new Vector2(205, 960 / 2 + 104 - 71));
        for (int j = 0; j < 13; j++)
        {
            GameObject card = Instantiate(cardToInstantiate, cardToInstantiate.transform.parent, false);
            RectTransform rtf = card.GetComponent<RectTransform>();
            rtf.pivot = Vector2.zero;
            rtf.anchorMin = Vector2.zero;
            rtf.anchorMax = Vector2.zero;
            //rtf.localScale = new Vector2(, .835f);
            rtf.DOScaleX(.8125f, .25f);
            rtf.DOScaleY(.835f, .25f);
            rtf.anchoredPosition = new Vector2(1706 / 2, 960 / 2);
            card.SetActive(true);
            DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, vecList[i - 1],.25f).OnComplete(()=> {
                Destroy(card,.25f);
            });
            yield return new WaitForSeconds(.125f);
        }
        for(int j = 0; j < 3; j++)
        {
            if (exitsSlotList[j + 1] == true)
            {
                cardCountList[j].text = 13.ToString();
                backCards[j].SetActive(true);
            }
        }

    }

    private IEnumerator _resort(List<int> ids)
    {
        for (int i = 0; i < myCardList.Count; i++)
        {
            myCardList[i].GetComponent<CardControl>().realSelected = false;
        }
        myCardId = new List<int>(ids);
        Vector2 rtf0 = new Vector2(64, -88);
        List<Vector2> ls = new List<Vector2>();
        for (int i = 0; i < 13; i++) //Thu gọn bài về
        {
            int tmp = i;
            myCardList[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(myCardList[i].GetComponent<RectTransform>().anchoredPosition.x, -88);
            ls.Add(myCardList[i].GetComponent<RectTransform>().anchoredPosition);

            RectTransform rtf = myCardList[tmp].GetComponent<RectTransform>();
            DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, rtf0, .1f).OnComplete(() => {

                if(tmp < ids.Count)
                {

                    //App.trace("=========" + tmp + "|=== " + ids[tmp]);
                    myCardList[tmp].transform.SetSiblingIndex(tmp);
                    myCardList[tmp].GetComponent<Image>().overrideSprite = faces[ids[tmp]];
                }

            });
        }
        yield return new WaitForSeconds(.1f);

        App.trace("IDS COUNT = " + ids.Count);
        for (int i = 12; i > -1; i--)
        {
            int tmp = i;
            if (tmp < ids.Count)
            {
                //myCardId.Add(ids[tmp]);
                RectTransform rtf = myCardList[tmp].GetComponent<RectTransform>();
                Vector2 pos = new Vector2(64 + 108 * (tmp), -88);
                myCardList[tmp].transform.localScale = Vector2.one;
                if (myCardList[tmp].activeSelf == false)
                {
                    myCardList[tmp].SetActive(true);
                }
                yield return new WaitForSeconds(.1f);
                DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, pos, .1f);
                //yield return new WaitForSeconds(.1f);
            }
            else
            {
                myCardList[tmp].SetActive(false);
            }
        }
        yield break;
    }

    public void openSettingPanel()
    {
        LoadingControl.instance.openSettingPanel();
    }

    public GameObject helpPanel;
    public void openHelpPanel(bool isOpen)
    {
        if (isOpen)
        {
            helpPanel.SetActive(true);
            return;
        }
        helpPanel.SetActive(false);
    }
}
