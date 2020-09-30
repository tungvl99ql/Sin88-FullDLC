using DG.Tweening;
using Core.Server.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;

public class PhomController : MonoBehaviour
{

    private UnityEvent discardPlayerActionCallback = new UnityEvent();
    private bool _hadDrawcard = false;
    private bool _isAutoDiscard = true;
    private bool _isAnimationFinish = true;
    private bool IsPlayActionFinished
    {
        get
        {
            return _isAnimationFinish && _hadDrawcard;
        }
    }

    public bool IsAutoDiscard
    {
        get
        {
            return _isAutoDiscard;
        }

        set
        {
            _isAutoDiscard = value;
        }
    }

    /// <summary>
    /// 0-3: userName|4-7: balance|8: tableName|9: countDown|10-13: earnText|14-17: points|18: att
    /// </summary>
    public Text[] phomTextList;
    /// <summary>
    /// 0-3: avar|4: BackBtn|5-8: HandCard-showCard|9-12: eatanCard-bộ|13:16: firedCard
    /// </summary>
    public Image[] phomImageList;
    /// <summary>
    /// 0-3: Info|4-7: owner|8-10:backCard|11: Noti|12-15: Timeleap|
    /// 16: HandCardToClone|17: Trans|18: backCard0|19: Trán0|20: help
    /// </summary>
    public GameObject[] phomObjList;
    //public Sprite addPlayerIcon;
    public Sprite addPlayerIcon;
    //public Sprite[] cardFaces;
    public Sprite[] cardFaces;


    /// <summary>
    /// 0: Trang|1: Vang
    /// </summary>
    public Font[] phomFontList;
    /// <summary>
    /// 0: Ăn|1: Bốc|2: Đánh
    /// </summary>
    public Button[] stateBtns;
    [HideInInspector]
    public static PhomController instance;

    public Sprite[] tableBackground;

    [HideInInspector]
    public bool regQuit = false;
    private bool isPlaying = false, isKicked = false;
    private float scX = 1, scY = 1;
    private float xxx = Screen.width, yyy = Screen.height;
    private string statusKick = "";
    private List<GameObject> firedCardList = new List<GameObject>();
    private List<int> myCardIdList = new List<int>();
    private string[] rsList = { "", "", "", "" };
    private bool isFullBand = false, isNoBand = false;
    /// <summary>
    /// 0-3: tọa độ bay xu|4-6: tọa độ backCard|7-10: tọa độ cuối khi nhả|11-14:tọa độ cuối khi ăn hoặc hạ bài
    /// |15: Bốc|16: my card
    /// </summary>
    private Vector2[] coordinatesList;
    private Vector3[] oriCoordinatesYList = new Vector3[4];

    [Header("Vua bai dep")]
    private bool isBeautiful = false; //co dc bai dep hay ko

    void Awake()
    {
        //App.trace("SS" + )
        //scY = 960 / yyy;
        Input.multiTouchEnabled = false;
        Application.runInBackground = true;
        Vector2[] v = { new Vector2(90, 180), new Vector2(1576, 542), new Vector2(793, 818), new Vector2(28, 547)
    , new Vector2(1460, 540), new Vector2(xxx/2 + 97, yyy - 146), new Vector2(160, yyy/2 + 60)
    , new Vector2(1243, 403), new Vector2(1240, 590), new Vector2(325, 590), new Vector2(323, 403)
    , new Vector2(325, 266), new Vector2(1359, 542), new Vector2(934, 706), new Vector2(243, 545)
    , new Vector2(814, 514), new Vector2(399, 110) };
        coordinatesList = v;
        for (int i = 0; i < 4; i++)
        {
            oriCoordinatesYList[i] = phomTextList[14 + i].transform.localPosition;
        }
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
    void Start()
    {
        CPlayer.baiDep = "PHOM.BAIDEP";

        //StartCoroutine(divideCards(new List<int>() { 8,21,34,10,24,37,4,5,6},false));
        //return;
        /*
        for(int i = 1; i < 4; i++)
        {
            StartCoroutine(divideCards2(i));
        }
        return;
        */
        /*
        Image img = Instantiate(phomImageList[16], phomImageList[16].transform.parent.parent, false);
        RectTransform rtf = img.gameObject.GetComponent<RectTransform>();
        rtf.pivot = Vector2.zero;
        rtf.anchoredPosition = new Vector2(853 + phomObjList[10].transform.localPosition.x, 480 + phomObjList[10].transform.localPosition.y);
        App.trace(phomObjList[10].transform.localPosition.x + "|" + phomObjList[10].transform.localPosition.y);
        img.overrideSprite = cardFaces[52];
        img.gameObject.SetActive(true);
        firedCardList.Add(img.gameObject);
        preCardRtf = rtf;
        preCardImg = img;
        //rtf.SetParent(phomImageList[16].transform.parent);

        phomObjList[10].SetActive(true);
        return;
        */

        //set background cua ban choi theo tung muc cuoc
        int bgIndex = UnityEngine.Random.Range(0, tableBackground.Length - 1);

        //if (CPlayer.betAmtId > 3 && CPlayer.betAmtId < 8)
        //{
        //    bgIndex = 1;
        //}
        //else if (CPlayer.betAmtId >= 8)
        //{
        //    bgIndex = 2;
        //}

        phomImageList[18].sprite = tableBackground[bgIndex];

        StartCoroutine(LoadingControl.instance._start());
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
                    if (i == amtId)
                    {
                        CPlayer.betAmtOfTableToGo = App.formatMoney(a.ToString());
                        string tempString = /*"PHOM";*/App.listKeyText["PHOM_NAME"].ToUpper();
                        phomTextList[8].text = tempString + " - " + CPlayer.betAmtOfTableToGo + " " + App.listKeyText["CURRENCY"]; //"PHỎM - " + CPlayer.betAmtOfTableToGo + " Gold";
                        break;
                    }


                }
            });
        }
        else
        {
            string tempString = /*"PHOM";*/ App.listKeyText["PHOM_NAME"].ToUpper();
            phomTextList[8].text = tempString +  " - " + App.formatMoney(CPlayer.betAmtOfTableToGo) + " " + App.listKeyText["CURRENCY"];//"PHỎM - " + App.formatMoney(CPlayer.betAmtOfTableToGo) + " Gold";
        }

        registerHandler();
        getTableDataEx();
    }

    private List<string> handelerCommand = new List<string>();  //Lưu các handler đã đăng ký
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
                //isPlaying = false;
                CPlayer.statusKick = content;
                //App.showErr(content);
                backToTableList();

            }

        });
        #endregion

        #region //SLOT_IN_TABLE_CHANGED
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

            slotId = detecSlotIdBySvrId(slotId);

            if (nickName.Length == 0)    //Có thằng thoát khỏi bàn chơi
            {
                // comment out
                SoundManager.instance.PlayUISound(SoundFX.CARD_EXIT_TABLE);
                playerList.Remove(slotId);
                phomObjList[slotId].SetActive(false);
                phomImageList[slotId].sprite = addPlayerIcon;
                phomImageList[slotId].overrideSprite = addPlayerIcon;
                //phomImageList[slotId].material = null;
                exitsSlotList[slotId] = false;
                if (slotId != 0)
                {
                    phomObjList[slotId + 4].SetActive(false);   //Xóa owner của thằng thoát
                    phomObjList[7 + slotId].SetActive(false);   //Ẩn backCard
                }
                if (playerList.Count < 2)   //Khi bàn chỉ còn 1 thằng thì k đếm ngược nữa
                {
                    if (preCoroutine != null)
                    {
                        phomTextList[9].gameObject.SetActive(false);
                        StopCoroutine(preCoroutine);

                    }

                }
                return;
            }
            //Comment out
            SoundManager.instance.PlayUISound(SoundFX.CARD_JOIN_TABLE);
            if (playerList.ContainsKey(slotId)) //Có thằng thay thế
            {
                playerList[slotId] = player;
                exitsSlotList[slotId] = true;
                setInfo(player, phomImageList[player.SlotId], phomObjList[player.SlotId], phomTextList[player.SlotId + 4], phomTextList[player.SlotId], phomObjList[player.SlotId + 4]);
                return;
            }
            //Thêm bình thường
            playerList.Add(slotId, player);
            exitsSlotList[slotId] = true;
            setInfo(player, phomImageList[player.SlotId], phomObjList[player.SlotId], phomTextList[player.SlotId + 4], phomTextList[player.SlotId], phomObjList[player.SlotId + 4]);

        });
        #endregion

        #region [SET_TURN]
        var req_SET_TURN = new OutBounMessage("SET_TURN");
        req_SET_TURN.addHead();
        handelerCommand.Add("SET_TURN");
        App.ws.sendHandler(req_SET_TURN.getReq(), delegate (InBoundMessage res_SET_TURN)
        {
            int slotId = res_SET_TURN.readByte();
            int turnTimeOut = res_SET_TURN.readShort(); //Thời gian chờ
            int playerRemainDuration = res_SET_TURN.readShort();    //Thời gian chờ còn lại

            App.trace("RECV [SET_TURN]");
            App.trace("slotId = " + slotId + "| turnTimeOut = " + turnTimeOut + "|playerRemainDuration  = " + playerRemainDuration);
            if (slotId == -2)    //Khi bắt đầu ván mới
            {
                if (turnTimeOut > 0)
                {

                    preCoroutine = _showCountDOwn(turnTimeOut);
                    StartCoroutine(preCoroutine);
                }
                else
                {
                    StopCoroutine(preCoroutine);
                }

                return;
            }


            if (turnTimeOut > 0)
            {
                phomObjList[preTimeLeapId + 12].SetActive(false);
                slotId = getSlotIdBySvrId(slotId);
                preTimeLeapId = slotId;
                time = turnTimeOut;
                curr = playerRemainDuration;
                phomObjList[slotId + 12].SetActive(true);
                preTimeLeapImage = phomObjList[slotId + 12].GetComponent<Image>();
                preTimeLeapImage.fillAmount = 1;
                run = true;
            }

        });
        #endregion

        #region [MOVE]
        var req_MOVE = new OutBounMessage("MOVE");
        req_MOVE.addHead();
        handelerCommand.Add("MOVE");
        App.ws.sendHandler(req_MOVE.getReq(), delegate (InBoundMessage res)
        {
            List<int> ids = new List<int>();
            ids = res.readBytes();
            //CardUtils.svrIdsToIds(ids);
            //comment out
            SoundManager.instance.PlayUISound(SoundFX.CARD_DROP);
            int sourceSlotId = res.readByte();
            int sourceLineId = res.readByte() - 1;
            int targetSlotId = res.readByte();
            int targetLineId = res.readByte() - 1;
            int targetIndex = res.readByte();

            App.trace(string.Format("RECV [MOVE] IdsCount = {0} | sourceSlotId = {1} | sourceLineId = {2}| targetSlotId = {3}| targetLineId = {4} | targetIndex = {5}| CON DATA =\n",
            ids.Count, sourceSlotId, sourceLineId, targetSlotId, targetLineId, targetIndex));

            #region //Chuyển bài đã nhả khi chênh lệch
            if (sourceLineId == 3 && targetLineId == 3)
            {
                int slotId = getSlotIdBySvrId(sourceSlotId);
                int mCount = phomImageList[slotId + 13].transform.parent.childCount - 1;
                App.trace("MCOUNT = " + mCount);
                if (mCount < 1)
                {
                    return;
                }
                //RECV [MOVE] IdsCount = 1 | sourceSlotId = 1 | sourceLineId = 3| targetSlotId = 0| targetLineId = 3 | targetIndex = -1| CON DATA =
                RectTransform rtf = null;
                if (slotId < 2)
                {
                    rtf = phomImageList[slotId + 13].transform.parent.GetChild(0).gameObject.GetComponent<RectTransform>();
                }
                else
                {
                    rtf = phomImageList[slotId + 13].transform.parent.GetChild(mCount).gameObject.GetComponent<RectTransform>();
                }
                slotId = getSlotIdBySvrId(targetSlotId);
                Vector2 vec = new Vector2(65, 0);
                Vector2 mPos = Vector2.zero;

                mPos = vec * (phomImageList[13 + slotId].transform.parent.childCount - 2);
                if (slotId == 0 || slotId == 1)
                {
                    mPos = new Vector2(260, 0) - mPos;
                }
                rtf.SetParent(phomImageList[slotId + 13].transform.parent);
                DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, mPos, .25f).OnComplete(() => {
                    if (slotId > 1)
                    {
                        rtf.SetAsLastSibling();
                    }
                    else
                    {
                        rtf.SetAsFirstSibling();
                    }
                    /*
                    int count = phomImageList[slotId + 13].transform.parent.childCount;
                    for(int i = 0; i < count; i++)
                    {
                        Rect
                        DOTween.To(() => mRtf.anchoredPosition, x => mRtf.anchoredPosition = x, new Vector2(90 * (mRtf.GetSiblingIndex() - 1), 0), .25f);
                    }*/
                });
                return;
            }
            #endregion

            if (targetSlotId == mySlotId && targetLineId == 0)   //Mình Bốc
            {
                myCard(ids, "take", 0, targetIndex);
                return;
            }

            if (targetSlotId == mySlotId && sourceSlotId == mySlotId && targetLineId == 1)  //Mình hạ bài
            {
                Debug.LogError("1111111 ====================>");
                Debug.LogError("IsAutoDiscard = " + IsAutoDiscard);
                Debug.LogError("cardPrepareList.Count = " + cardPrepareList.Count);
                Debug.LogError("isDragging = " + isDragging);
                Debug.LogError("_isAnimationFinish = " + _isAnimationFinish);
                Debug.LogError("<==================== 222222222");
                if (IsAutoDiscard)
                {
                    discardPlayerActionCallback.RemoveAllListeners();
                    discardPlayerActionCallback.AddListener(
                        () =>
                        {
                            myCard(ids, "final");
                        }
                    );
                    ForceFinishPlayerAction(discardPlayerActionCallback);
                }
                else
                {
                    myCard(ids, "final");
                }
                //myCard(ids, "final");
                return;
            }

            if (sourceSlotId == mySlotId && targetLineId == 3 && sourceLineId == 0)   //Mình Nhả
            {
                //App.trace("MÌNH NHẢ " + ids[0]);
                //myCard(ids, "remove");
                if (myCardIdList.Count > 9)
                {
                    _hadDrawcard = true;
                }
                Debug.LogError("1111111 ====================>");
                Debug.LogError("IsAutoDiscard = " + IsAutoDiscard);
                Debug.LogError("cardPrepareList.Count = " + cardPrepareList.Count);
                Debug.LogError("isDragging = " + isDragging);
                Debug.LogError("_isAnimationFinish = " + _isAnimationFinish);
                Debug.LogError("<==================== 222222222");

                if (IsAutoDiscard)
                {
                    // TODO:
                    //1 Finish in progressing action
                    //2 Discard
                    discardPlayerActionCallback.RemoveAllListeners();
                    discardPlayerActionCallback.AddListener(
                        () =>
                        {
                            myCard(ids, "remove");
                        }
                    );
                    ForceFinishPlayerAction(discardPlayerActionCallback);

                }
                else
                {
                    myCard(ids, "remove");
                }

                return;
            }

            if (targetSlotId == mySlotId && targetLineId == 1)   //Mình ăn bài
            {
                myCard(ids, "eat");
                return;
            }
            if (sourceSlotId == mySlotId && targetLineId == 1 && sourceLineId == 0)   //Mình gửi bài
            {
                myCard(ids, "send", getSlotIdBySvrId(targetSlotId));
                return;
            }
            moveCard(ids, sourceSlotId, targetSlotId, sourceLineId, targetLineId);
        });
        #endregion

        #region [GAMEOVER]
        var req_GAMEOVER = new OutBounMessage("GAMEOVER");
        req_GAMEOVER.addHead();
        handelerCommand.Add("GAMEOVER");
        App.ws.sendHandler(req_GAMEOVER.getReq(), delegate (InBoundMessage res) {
            App.trace("RECV [GAMEOVER]");
            Debug.LogError("======================== GAME_OVER =======================================");
            _hadDrawcard = true;
            run = false;    //K chạy timeLeap nữa
            preTimeLeapImage.gameObject.SetActive(false);   //Ẩn time leap

            List<int> winLs = new List<int>(), loseLs = new List<int>();

            // if(isBeautiful) {

            //    StartCoroutine(_showTheLe());

            // }

            int count = res.readByte();
            for (int i = 0; i < count; i++)
            {
                int slotId = res.readByte();
                int grade = res.readByte();
                long earnValue = res.readLong();
                App.trace("slotId = " + slotId + "|grade =" + grade + "|earvalue = " + earnValue);
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
                    case 5:
                        title = App.listKeyText["PHOM_U"];//"Ù";
                        /*
                        for(int j = 0; j < 4; j++)
                        {
                            phomTextList[14 + j].gameObject.SetActive(false);
                        }*/
                        break;
                    case 0:
                        title = App.listKeyText["PHOM_NO_BAND"];//"MÓM";
                        break;
                }

                if (slotId == mySlotId)
                {
                    if (earnValue > 0)
                    {
                        SoundManager.instance.PlayEffectSound(SoundFX.CARD_PHOM_WIN);
                    }
                    else
                    {

                        SoundManager.instance.PlayEffectSound(SoundFX.CARD_LOSE);
                    }
                }
                slotId = getSlotIdBySvrId(slotId);
                if (grade == 1 || grade == 5)
                    phomTextList[10 + slotId].font = phomFontList[1];
                else
                    phomTextList[10 + slotId].font = phomFontList[0];
                phomTextList[10 + slotId].text = title;
                if (grade == 5)  //Ù
                {
                    Text goj = Instantiate(phomTextList[10 + slotId], phomTextList[10 + slotId].transform.parent, false);
                    goj.text = "Ù";
                    RectTransform rtf = goj.gameObject.GetComponent<RectTransform>();
                    Vector2 mPiVotEnd = rtf.pivot;
                    rtf.DOMove(phomObjList[18].transform.position, .01f);
                    rtf.localScale = Vector2.one * 10f;
                    rtf.pivot = Vector2.one * .5f;

                    Vector2 mPos = phomTextList[10 + slotId].gameObject.GetComponent<RectTransform>().anchoredPosition;

                    rtf.DOScale(3f, 1.5f).SetEase(Ease.OutBounce).OnComplete(() => {
                        rtf.DOScale(1f, 1.25f).OnComplete(() => {
                            Destroy(goj.gameObject);
                        });
                        DOTween.To(() => rtf.pivot, x => rtf.pivot = x, mPiVotEnd, 1f);
                        DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, mPos, 1f);
                    });
                }
                string rs = "";
                if (earnValue > -1)
                {
                    winLs.Add(slotId);
                    rs = "+" + App.formatMoney(earnValue.ToString());
                }
                else if (earnValue < 0)
                {
                    loseLs.Add(slotId);
                    rs = "-" + App.formatMoney(Math.Abs(earnValue).ToString());
                }

                /*
                if (earnValue > -1)
                {
                    winLs.Add(slotId);
                    rs = title + "\n+" + App.formatMoney(earnValue.ToString());
                }
                else if (earnValue < 0)
                {
                    loseLs.Add(slotId);
                    rs = title + "\n-" + App.formatMoney(Math.Abs(earnValue).ToString());
                }


                phomTextList[slotId + 10].text = rs;
                phomTextList[slotId + 10].font = earnValue < 1 ? phomFontList[0] : phomFontList[1];
                phomTextList[slotId + 10].gameObject.SetActive(true);
                */
                rsList[slotId] = rs;
            }
            string matchResult = res.readStrings();

            StartCoroutine(_quit(winLs, loseLs));
            discardPlayerActionCallback.RemoveAllListeners();
            discardPlayerActionCallback.AddListener(() => {
                StartCoroutine(_ClearCardWhenGameOver());
            });
            ForceFinishPlayerAction(discardPlayerActionCallback);
        });
        #endregion

        #region [START_MATCH]
        var req_START_MATCH = new OutBounMessage("START_MATCH");
        req_START_MATCH.addHead();
        handelerCommand.Add("START_MATCH");
        App.ws.sendHandler(req_START_MATCH.getReq(), delegate (InBoundMessage res_START_MATCH)
        {
            Debug.LogError("======================== START_GAME 1 =======================================");
            isDragging = false;
            SoundManager.instance.PlayUISound(SoundFX.CARD_START);
            isFullBand = false;
            isNoBand = false;
            foreach (GameObject goj in firedCardList)
            {
                try
                {
                    Destroy(goj);
                }
                catch
                {

                }

            }
            firedCardList.Clear();
            for (int i = 0; i < playerCardList.Count; i++)   //Xóa các quân bài của mình
            {
                try
                {
                    Destroy(playerCardList[i].Rtf.gameObject);
                }
                catch
                {

                }
            }

            myCardIdList.Clear();   //Xóa danh sách id bài của mình
            phomObjList[18].SetActive(true);    //Hiện bài giữa bàn chơi
            coinFlyed = false;

            LoadingControl.instance.delCoins();
            for (int i = 0; i < 4; i++)
            {
                if (rsList[i] != "")
                {
                    phomTextList[14 + i].gameObject.SetActive(false);
                    phomTextList[14 + i].transform.localScale = Vector3.one;

                    phomTextList[14 + i].transform.localScale = Vector2.one;
                    //phomTextList[14 + i].transform.localPosition -= new Vector3(0, 100, 0);
                    phomTextList[14 + i].transform.localPosition = oriCoordinatesYList[i];
                    phomTextList[10 + i].gameObject.SetActive(false);
                    rsList[i] = "";
                }
            }

            for (int i = 1; i < 4; i++)
            {
                if (exitsSlotList[i] == true)
                {
                    StartCoroutine(divideCards2(i));
                }
            }
            isPlaying = true;
            loadPlayerMatchPoint(res_START_MATCH);
            loadBoardData(res_START_MATCH, false);
        });
        #endregion

        #region [SHOW_PLAYER_CARD]
        var req_SHOW_PLAYER_CARD = new OutBounMessage("SHOW_PLAYER_CARD");
        req_SHOW_PLAYER_CARD.addHead();
        handelerCommand.Add("SHOW_PLAYER_CARD");
        App.ws.sendHandler(req_SHOW_PLAYER_CARD.getReq(), delegate (InBoundMessage res_SHOW_PLAYER_CARD)
        {
            int slotId = res_SHOW_PLAYER_CARD.readByte();
            List<int> ids = res_SHOW_PLAYER_CARD.readBytes();
            int slotIdClone = slotId;
            slotId = getSlotIdBySvrId(slotId);
            App.trace("RECV [SHOW_PLAYER_CARD] of slot = " + slotId + "|" + ids.Count);

            phomObjList[18].SetActive(false);    //Ẩn bài giữa bàn

            //=====BAY ĐIỂM============
            phomTextList[14 + slotId].font = phomFontList[1];
            phomTextList[14 + slotId].text = idsToPoint(ids);
            phomTextList[14 + slotId].gameObject.SetActive(true);
            Vector3 vec3 = phomTextList[14 + slotId].transform.localPosition;
            Transform rtf1 = phomTextList[14 + slotId].transform;
            //if(phomTextList[14 + slotId].text != "")
            rtf1.DOLocalMoveY(vec3.y + 100, 1f);
            //====END BAY ĐIỂM==========

            //========SHOW BÀI==========
            if (slotId == 0)    //Bài của mình
            {
                discardPlayerActionCallback.RemoveAllListeners();
                discardPlayerActionCallback.AddListener(() => {
                    //for (int i = 0; i < ids.Count; i++)
                    //{
                    //    for (int j = 0; j < playerCardList.Count; j++)
                    //    {
                    //        if (int.Parse(playerCardList[j].Img.overrideSprite.name) - 1 == ids[i])
                    //        {
                    //            try
                    //            {
                    //                Destroy(playerCardList[j].Rtf.gameObject.GetComponent<PhomCardCtrl>());
                    //            }
                    //            catch
                    //            {

                    //            }
                    //            break;
                    //        }
                    //    }
                    //}
                    //return;
                });

                ForceFinishPlayerAction(discardPlayerActionCallback);

                //for (int i = 0; i < playerCardList.Count; i++)
                //{
                //    playerCardList[i].Rtf.GetComponent<TLMNCardCtrl>().EndProcess();
                //}
                //isDragging = false;

                //for (int i = 0; i < ids.Count; i++)
                //{
                //    for(int j = 0; j < playerCardList.Count; j++)
                //    {
                //        if(int.Parse(playerCardList[j].Img.overrideSprite.name) - 1 == ids[i])
                //        {
                //            try
                //            {
                //                Destroy(playerCardList[j].Rtf.gameObject.GetComponent<PhomCardCtrl>());
                //            }
                //            catch
                //            {

                //            }
                //            break;
                //        }
                //    }
                //}
                return;
            }
            #region "//SHOW BÀI ĐÔI THỦ [RE DONE]"
            phomObjList[slotId + 7].SetActive(false);
            moveCard(ids, slotIdClone, slotIdClone, 0, -9);
            return;
            #endregion
            //==========END SHOW BÀI==========
        });
        #endregion

        #region [ENTER_STATE]
        var req_ENTER_STATE = new OutBounMessage("ENTER_STATE");
        req_ENTER_STATE.addHead();
        handelerCommand.Add("ENTER_STATE");
        App.ws.sendHandler(req_ENTER_STATE.getReq(), delegate (InBoundMessage res_ENTER_STATE)
        {
            int stateId = res_ENTER_STATE.readByte();
            enterState(stateById[stateId]);
            enterState2(stateById[stateId]);
        });
        #endregion

        #region [SET_CARDS]
        var req_SET_CARDS = new OutBounMessage("SET_CARDS");
        req_SET_CARDS.addHead();
        handelerCommand.Add("SET_CARDS");
        App.ws.sendHandler(req_SET_CARDS.getReq(), delegate (InBoundMessage res_SET_CARDS)
        {
            App.trace("RECV [SET_CARDS]");
            var slotId = res_SET_CARDS.readByte();
            var lineId = res_SET_CARDS.readByte() - 1;
            var cardCount = res_SET_CARDS.readByte();
            List<int> ids = null;
            if (cardCount < 0)
            {
                ids = res_SET_CARDS.readBytes();
                StartCoroutine(divideCards(ids));
            }


        });
        #endregion

        #region [SET_PLAYER_ATTR]
        var req_SET_PLAYER_ATTR = new OutBounMessage("SET_PLAYER_ATTR");
        req_SET_PLAYER_ATTR.addHead();
        handelerCommand.Add("SET_PLAYER_ATTR");
        App.ws.sendHandler(req_SET_PLAYER_ATTR.getReq(), delegate (InBoundMessage res)
        {
            App.trace("RECV [SET_PLAYER_ATTR]");
            int slotId = res.readByte();
            string icon = res.readAscii();
            string content = res.readAscii();
            int action = res.readByte();
            App.trace("????? icon = " + icon + "|action = " + action + "|content" + content);
            //????? icon = full_band|action = 0|content
            //isFullBand = true;

            // if(icon.Equals("full_band") && slotId == mySlotId) {
            //     isBeautiful = true;
            //     VuaBaiDepController.instance.canSent = true;
            //     VuaBaiDepController.instance.PlayCanSentAnim();

            // }

            if (icon == "full_band")
                isFullBand = true;
            if (icon == "no_band")
                isNoBand = true;

        });
        #endregion
    }


    private void ForceFinishPlayerAction(UnityEvent onFinish = null)
    {
        // Not has any card is dragging.
        if (!isDragging)
        {
            if (onFinish != null)
            {
                //onFinish.Invoke();
                Debug.LogError("GOI ĐÂY 1 ======= ");
                StartCoroutine(ASysInvoke(onFinish));


            }
        }
        else
        {
            Debug.LogError("GOI ĐÂY 2 ======= ");
            for (int i = 0; i < playerCardList.Count; i++)
            {
                playerCardList[i].Rtf.GetComponent<PhomCardCtrl>().EndProcess();
            }
        }

    }

    IEnumerator ASysInvoke(UnityEvent a)
    {

        Debug.LogError("***** IsPlayActionFinished ****** " + IsPlayActionFinished);

        while (!IsPlayActionFinished)
        {
            Debug.LogError("***** IsPlayActionFinished ****** " + IsPlayActionFinished);
            yield return null;

        }
        Debug.LogError("888888888 IsPlayActionFinished 888888888 " + IsPlayActionFinished);
        Debug.LogError("99999999999999  IsPlayActionFinished 999999999999999  " + IsPlayActionFinished);
        a.Invoke();
    }


    // IEnumerator _showTheLe() {
    //     yield return new WaitForSeconds(5f);
    //     VuaBaiDepController.instance.canSent = false;
    //     isBeautiful = false;
    //     VuaBaiDepController.instance.PlayCanSentAnim();
    // }

    //sau khi gameover thi clear het bai di, binh thuong la bat dau van moi thi moi clear
    IEnumerator _ClearCardWhenGameOver()
    {
        Debug.LogError("======================== GAME OVER 2 =======================================");
        discardPlayerActionCallback.RemoveAllListeners();
        if (isPlaying)
            yield return null;
        yield return new WaitForSeconds(5f);
        isFullBand = false;
        isNoBand = false;
        foreach (GameObject goj in firedCardList)
        {
            try
            {
                Destroy(goj);
            }
            catch
            {

            }

        }
        Debug.LogError("DELETE ME NOW.........................");
        for (int i = 0; i < playerCardList.Count; i++)   //Xóa các quân bài của mình
        {
            try
            {
                Destroy(playerCardList[i].Rtf.gameObject);
            }
            catch
            {

            }
        }

        playerCardList.Clear();


        myCardIdList.Clear();   //Xóa danh sách id bài của mình
        phomObjList[18].SetActive(false);    //Xoa icon bài giữa bàn chơi
        coinFlyed = false;

        LoadingControl.instance.delCoins();

        for (int i = 0; i < 4; i++)
        {
            if (rsList[i] != "")
            {
                phomTextList[14 + i].gameObject.SetActive(false);
                phomTextList[14 + i].transform.localScale = Vector3.one;

                phomTextList[14 + i].transform.localScale = Vector2.one;
                //phomTextList[14 + i].transform.localPosition -= new Vector3(0, 100, 0);
                phomTextList[14 + i].transform.localPosition = oriCoordinatesYList[i];
                phomTextList[10 + i].gameObject.SetActive(false);
                rsList[i] = "";
            }
        }

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
    #region //STATE CMD
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

    #endregion

    #region TABLE DATA

    private Dictionary<int, Player> playerList;
    private int mySlotId = -1;
    private int currOwnerId = -1;
    private bool[] exitsSlotList = { false, false, false, false };
    private void loadTableData(InBoundMessage res)
    {
        playerList = new Dictionary<int, Player>();

        mySlotId = res.readByte();

        isPlaying = res.readByte() == 1;
        App.trace("MY SLOT ID = " + mySlotId + "|isPlaying = " + isPlaying);
        if (isPlaying == true)
        {
            phomObjList[18].SetActive(true);
        }
        else
        {
            phomObjList[18].SetActive(false);
        }
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
                App.trace("CURR OWNER = " + currOwnerId);
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
                try
                {
                    //App.trace("SET PLAYER INFO " + player.SlotId + "|SVID = " + player.SvSlotId);
                    setInfo(player, phomImageList[player.SlotId], phomObjList[player.SlotId], phomTextList[player.SlotId + 4], phomTextList[player.SlotId], phomObjList[player.SlotId + 4]);
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

        int slotRemainDuration = res.readShort();
        var currentState = res.readByte();

        App.trace("currentTurnSlotId = " + currentTurnSlotId + "|currTimeOut = " + currTimeOut + "|slotRemainDuration = " + slotRemainDuration + "|");

        enterState(stateById[currentState]);

        if (currTimeOut > 0 && currentTurnSlotId > -1)
        {
            preTimeLeapId = getSlotIdBySvrId(currentTurnSlotId);
            time = currTimeOut;
            curr = slotRemainDuration;
            phomObjList[preTimeLeapId + 12].SetActive(true);
            preTimeLeapImage = phomObjList[preTimeLeapId + 12].GetComponent<Image>();
            preTimeLeapImage.fillAmount = 1;
            run = true;
        }
        if (mySlotId > -1)
            enterState2(stateById[currentState]);
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

    private long[] chipList = { 0, 0, 0, 0 };
    private void setInfo(Player player, Image im, GameObject infoObj, Text balanceText, Text nickNamText, GameObject ownerImg)
    {
        //im.gameObject.transform.localScale = Vector3.one;
        StartCoroutine(App.loadImg(im, App.getAvatarLink2(player.Avatar, (int)player.PlayerId), player.SvSlotId == mySlotId));
        if (infoObj != null)
            infoObj.SetActive(true);
        chipList[player.SlotId] = player.ChipBalance;
        balanceText.text = "100.0 K";
        balanceText.text = " " + (player.SlotId == 0 ? App.formatMoney(player.ChipBalance.ToString()) : App.formatMoneyAuto(player.ChipBalance));
        nickNamText.text = App.formatNickName(player.NickName, 10);
        ownerImg.SetActive(player.IsOwner);

    }

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

        foreach (Button btn in stateBtns)
        {
            btn.gameObject.SetActive(false);
        }

        App.trace("ENTER STATE = " + state.Code);
        switch (state.Code)
        {
            case "take":    //Bốc bài
                stateBtns[1].gameObject.SetActive(true);
                break;
            case "eat": //Ăn
                stateBtns[0].gameObject.SetActive(true);
                stateBtns[1].gameObject.SetActive(true);
                break;
            case "remove":   //NHẢ
                stateBtns[2].gameObject.SetActive(true);
                break;
            case "drop":    //Hạ bài
                stateBtns[3].gameObject.SetActive(true);
                stateBtns[4].gameObject.SetActive(true);
                stateBtns[5].gameObject.SetActive(true);
                break;
        }
    }
    #endregion
    private void loadPlayerMatchPoint(InBoundMessage res)
    {
        int count = res.readByte();
        for (int i = 0; i < count; i++)
        {
            int slotId = res.readByte();
            int point = res.readInt();
        }
    }
    private void loadBoardData(InBoundMessage res, bool isReconnect = true)
    {
        if (isPlaying == false)
            return;
        int slotCount = res.readByte();
        //App.trace("slot count = " + slotCount);
        for (int i = 0; i < slotCount; i++)
        {
            int slotId = res.readByte();
            slotId = getSlotIdBySvrId(slotId);
            int lineCount = res.readByte();
            App.trace("slotId = " + slotId + "|BOARD DATA lineCount = " + lineCount);
            for (int j = 0; j < lineCount; j++)
            {
                int cardLineId = res.readByte() - 1;
                int cardCount = res.readByte();

                if (cardCount < 0)   //
                {

                    List<int> ids = new List<int>();
                    ids = res.readBytes();

                    //CardUtils.svrIdsToIds(ids);
                    //if (slotId == mySlotId)
                    //myCardIdList.Add(cardLineId, ids);
                    if (slotId == 0 && cardLineId == 0)
                    {
                        StartCoroutine(divideCards(ids, isReconnect));
                        continue;
                    }

                    if (slotId != 0)
                    {
                        phomObjList[slotId + 7].SetActive(true);    //Hiện bài úp của thằng khác
                    }
                    if (cardLineId == 1 && ids.Count > 0)    //Bài ăn
                    {
                        Vector3 vec = new Vector3(65, 0, 0);
                        if (slotId == 1 || slotId == 3)
                        {
                            vec = new Vector3(0, 65, 0);

                        }


                        for (int k = 0; k < ids.Count; k++)
                        {
                            Image img = Instantiate(phomImageList[slotId + 9], phomImageList[slotId + 9].transform.parent, false);

                            if (slotId == 0 || slotId == 2)
                            {
                                img.transform.SetAsLastSibling();
                                img.transform.localPosition = img.transform.localPosition + vec * k;

                            }
                            else
                            {
                                img.transform.SetAsFirstSibling();
                                img.transform.localPosition = img.transform.localPosition + vec * k;
                                img.transform.localPosition += new Vector3(0, -65 * (int)Mathf.Ceil(ids.Count / 2), 0);
                            }
                            img.overrideSprite = cardFaces[ids[k]];
                            img.gameObject.SetActive(true);
                            firedCardList.Add(img.gameObject);
                        }
                        continue;
                    }
                    if (cardLineId == 3 && ids.Count > 0)   //Bài nhả
                    {
                        App.trace(slotId + "====NHẢ====" + ids.Count);
                        CardUtils.svrIdsToIds(ids);
                        Vector3 vec = new Vector3(65, 0, 0);
                        for (int k = 0; k < ids.Count; k++)
                        {
                            Image img = Instantiate(phomImageList[slotId + 13], phomImageList[slotId + 13].transform.parent, false);

                            if (slotId > 1)
                            {
                                img.transform.SetAsLastSibling();
                                img.transform.localPosition = img.transform.localPosition + vec * k;
                            }
                            else
                            {
                                img.transform.SetAsFirstSibling();
                                img.transform.localPosition = img.transform.localPosition - vec * k;
                            }
                            img.overrideSprite = cardFaces[ids[k]];
                            img.gameObject.SetActive(true);
                            firedCardList.Add(img.gameObject);
                        }
                    }
                    App.trace("[[[[[[[[[[[mySlotId = " + mySlotId + "slotId = " + slotId + "|length = " + ids.Count + "|cardLineId = " + cardLineId);
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
    }

    public void openSetting()
    {
        LoadingControl.instance.openSettingPanel();
    }

    public void openChat()
    {

    }
    private float time = 1, curr = 0;
    private bool run = false;
    private int preTimeLeapId = -1;
    private Image preTimeLeapImage;
    // Update is called once per frame
    void Update()
    {
        curr += Time.deltaTime;
        if (run)
        {
            preTimeLeapImage.fillAmount = (time - curr) / time;
            if (time - curr < 1 && currentState == "take")
            {
                run = false;
                preTimeLeapImage.gameObject.SetActive(false);
                stateBtnActions("take");
            }
        }


    }

    private IEnumerator preCoroutine = null;
    private IEnumerator _showCountDOwn(int timeOut)
    {
        /*
        if (regQuit)
        {
            backToTableList();
            yield break;
        }*/
        float mTime = timeOut - 2;

        phomTextList[9].text = App.listKeyText["GAME_PREPARE"].ToUpper();//"CHUẨN BỊ";
        phomTextList[9].gameObject.SetActive(true);
        yield return new WaitForSeconds(.5f);
        phomTextList[9].gameObject.SetActive(false);
        yield return new WaitForSeconds(.5f);
        while (mTime > 0f)
        {
            phomTextList[9].text = mTime.ToString();
            phomTextList[9].gameObject.SetActive(true);
            yield return new WaitForSeconds(.5f);
            phomTextList[9].gameObject.SetActive(false);
            yield return new WaitForSeconds(.5f);
            mTime -= 1f;
        }

        phomTextList[9].text = App.listKeyText["GAME_START"].ToUpper(); //"BẮT ĐẦU";
        phomTextList[9].gameObject.SetActive(true);
        yield return new WaitForSeconds(.5f);
        phomTextList[9].gameObject.SetActive(false);
    }

    private bool coinFlyed = false;
    public void showNoti()
    {
        if (playerList.Count < 2 || myCardIdList.Count < 1 || coinFlyed == true || isPlaying == false)
        {
            LoadingControl.instance.delCoins();
            backToTableList();
            return;
        }
        this.regQuit = !regQuit;
        phomObjList[11].GetComponentInChildren<Text>().text = !regQuit ? App.listKeyText["GAME_BOARD_EXIT_CANCEL"] : App.listKeyText["GAME_BOARD_EXIT"]; //"Bạn đã hủy đăng ký rời bàn." : "Bạn đã đăng ký rời bàn.";
        phomImageList[4].transform.localScale = new Vector2(regQuit ? -1 : 1, 1);
        phomObjList[11].SetActive(true);
        StopCoroutine("_showNoti");
        StartCoroutine("_showNoti");
    }

    private IEnumerator _showNoti()
    {

        yield return new WaitForSeconds(2f);
        phomObjList[11].SetActive(false);
    }

    public void backToTableList()
    {
        if (isKicked)
        {
            DOTween.PauseAll();
            LoadingControl.instance.delCoins();
            delAllHandle();
            StartCoroutine(openTable());
            return;
        }
        if (isPlaying == false || playerList.Count < 2 || regQuit || myCardIdList.Count < 1)
        {
            //LoadingControl.instance.blackPanel.SetActive(true);
            DOTween.PauseAll();
            LoadingControl.instance.delCoins();
            delAllHandle();

            EnterParentPlace(delegate () {
                if (LoadingControl.instance.chatBox.activeSelf)
                {
                    LoadingControl.instance.chatBox.SetActive(true);

                }
                StartCoroutine(openTable());
            });
        }
    }

    private bool exited = false;
    IEnumerator openTable()
    {
        exited = true;
        CPlayer.preScene = "Phom";
        if (isKicked)
            CPlayer.preScene = "PhomK";
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
            LoadingControl.instance.showPlayerInfo(CPlayer.nickName, (long)CPlayer.chipBalance, (long)CPlayer.manBalance, CPlayer.id, true, phomImageList[slotIdToShow].overrideSprite, "me");
            //ProfileController.instance.Show();
            return;
        }

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
                    LoadingControl.instance.showPlayerInfo(pl.NickName, pl.ChipBalance, pl.StarBalance, pl.PlayerId, isCPlayerFriend, phomImageList[slotIdToShow].overrideSprite, typeShowInfo);
                });
                return;
            }
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
            for (int i = 0; i < arr.Count; i++)
            {
                temp = arr[i];
                cardType = (int)Mathf.Floor((float)temp / 13);
                cardHigh = temp % 13 + 1;

                switch (cardHigh)
                {
                    case 1:
                        App.trace("A " + cardTypes[cardType]);
                        break;
                    case 11:
                        App.trace("J " + cardTypes[cardType]);
                        break;
                    case 12:
                        App.trace("Q " + cardTypes[cardType]);
                        break;
                    case 13:
                        App.trace("K " + cardTypes[cardType]);
                        break;
                    default:
                        //App.trace(cardHigh + cardTypes[cardType]);
                        break;

                }
            }
        }
    }

    private RectTransform preCardRtf = null;  //Quân bài đã nhả trước đó
    private Image preCardImg;   //Img quân bài đã nhả trước đó
    private void moveCard(List<int> ids, int sourceSlotId, int targetSlotId, int sourceLineId, int targetLineId)
    {

        #region //SHOW BÀI
        if (sourceSlotId == targetSlotId && targetLineId == -9)
        {
            App.trace("SHOW");
            int slotId = getSlotIdBySvrId(sourceSlotId);
            for (int k = 0; k < ids.Count; k++)
            {
                int tmp = k;
                Image img = Instantiate(phomImageList[5 + slotId], phomImageList[5 + slotId].transform.parent, false);
                RectTransform rtf = img.gameObject.GetComponent<RectTransform>();
                firedCardList.Add(img.gameObject);
                img.gameObject.SetActive(true);
                if (slotId == 1 || slotId == 3)
                {
                    rtf.SetAsFirstSibling();
                    DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, new Vector2(rtf.anchoredPosition.x, 65 * (tmp + 2) - 65 * (int)Mathf.Ceil(ids.Count / 2)), .25f).OnComplete(() => {
                        rtf.DORotate(new Vector3(0, 90, 0), .125f).OnComplete(() =>
                        {
                            try
                            {
                                img.overrideSprite = cardFaces[ids[tmp]];
                            }
                            catch
                            {
                                //App.trace("FACK DIU = " + ids[tmp]);
                            }
                            rtf.DORotate(new Vector3(0, 0, 0), .125f);
                        });
                    });

                }
                else
                {
                    rtf.SetAsLastSibling();
                    DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, new Vector2(rtf.anchoredPosition.x + 65 * tmp, rtf.anchoredPosition.y), .25f).OnComplete(() => {
                        rtf.DORotate(new Vector3(0, 90, 0), .125f).OnComplete(() =>
                        {
                            try
                            {
                                img.overrideSprite = cardFaces[ids[tmp]];
                            }
                            catch
                            {
                                //App.trace("FACK DIU = " + ids[tmp]);
                            }
                            rtf.DORotate(new Vector3(0, 0, 0), .125f);
                        });
                    }); ;
                }

            }
            return;
        }
        #endregion

        #region //HẠ + NHẢ [RE DONE]
        if (sourceSlotId == targetSlotId && sourceSlotId != mySlotId)
        {
            int mIdEnd = 0;
            int mIdStart = 0;

            switch (targetLineId)
            {
                case 3: //Nhả
                    App.trace("[OTHERS PLAYER Đánh bài ???!!!", Color.blue.ToString());
                    mIdEnd = 7;
                    mIdStart = 13;
                    break;
                case 1: //Hạ
                    App.trace("[OTHERS PLAYER Hạ bài ???!!!", Color.blue.ToString());
                    Transform tfm = phomImageList[9 + getSlotIdBySvrId(targetSlotId)].transform.parent;
                    int count = tfm.childCount;
                    App.trace("==================" + count);
                    for (int i = count - 1; i > -1; i--)
                    {
                        //tfm.GetChild(i).gameObject.SetActive(false);
                        if (tfm.GetChild(i).gameObject.name.Contains("Clone"))
                            DestroyImmediate(tfm.GetChild(i).gameObject);
                    }
                    mIdEnd = 11;
                    mIdStart = 9;
                    break;
                default:    //Show bài
                    App.trace("[OTHERS PLAYER Show hết bài ???!!!", Color.blue.ToString());
                    mIdEnd = 11;
                    mIdStart = 5;
                    break;
            }
            int slotId = getSlotIdBySvrId(sourceSlotId);
            Vector2 vec = new Vector2(65, 0);
            if ((targetLineId == 1) && (slotId == 3 || slotId == 1))
                vec = new Vector2(0, 65);
            Vector2 mPivotStart = phomObjList[7 + slotId].GetComponent<RectTransform>().pivot;
            Vector2 mPivotEnd = phomImageList[slotId + mIdStart].GetComponent<RectTransform>().pivot;
            for (int k = 0; k < ids.Count; k++)
            {
                int tmp = k;
                Image img = Instantiate(phomImageList[slotId + mIdStart], phomImageList[slotId + mIdStart].transform.parent.parent, false);
                RectTransform rtf = img.gameObject.GetComponent<RectTransform>();
                rtf.pivot = mPivotStart;
                img.transform.localPosition = phomObjList[slotId + 7].transform.localPosition;


                img.overrideSprite = cardFaces[52];
                img.gameObject.SetActive(true);
                firedCardList.Add(img.gameObject);

                Vector2 mPos = vec * (phomImageList[slotId + mIdStart].transform.parent.childCount - 1);
                if (vec.y > 0 && (slotId == 1 || slotId == 3))
                {
                    mPos -= new Vector2(0, 65 * (int)Mathf.Ceil(ids.Count / 2));
                }
                if (vec.x > 0 && slotId == 1)
                {
                    mPos = new Vector2(260, 0) - mPos;
                }
                if (targetLineId != 1)
                {
                    preCardRtf = rtf;
                    preCardImg = img;
                }

                rtf.SetParent(phomImageList[slotId + mIdStart].transform.parent);
                DOTween.To(() => rtf.pivot, x => rtf.pivot = x, mPivotEnd, .25f);
                DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, mPos, .25f).OnComplete(() =>
                {
                    if (vec.x > 0)
                    {
                        if (slotId > 1)
                        {
                            img.transform.SetAsLastSibling();
                        }
                        else
                        {
                            img.transform.SetAsFirstSibling();
                        }
                    }
                    else
                    {
                        img.transform.SetAsFirstSibling();
                    }
                    rtf.DORotate(new Vector3(0, 90, 0), .125f).OnComplete(() =>
                    {
                        try
                        {
                            img.overrideSprite = cardFaces[ids[tmp]];
                        }
                        catch
                        {
                            //App.trace("FACK DIU = " + ids[tmp]);
                        }
                        rtf.DORotate(new Vector3(0, 0, 0), .125f);
                    });
                });
            }
            return;
        }
        #endregion

        #region //BỐC [RE DONE]
        if (sourceSlotId == -1 && targetSlotId != mySlotId)
        {
            /*
            Image img = Instantiate(phomImageList[14], phomImageList[14].transform.parent.parent, false);
            RectTransform rtf = img.gameObject.GetComponent<RectTransform>();
            rtf.anchoredPosition = new Vector2(coordinatesList[15].x * scX, coordinatesList[15].y * scY);
            img.overrideSprite = cardFaces[52];
            img.gameObject.SetActive(true);
            Vector2 endPos = new Vector2(coordinatesList[getSlotIdBySvrId(targetSlotId) + 3].x * scX, coordinatesList[getSlotIdBySvrId(targetSlotId) + 3].y * scY);
            DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, endPos, .25f).OnComplete(()=> {
                Destroy(img.gameObject);
            });


             */
            int slotId = getSlotIdBySvrId(targetSlotId);
            Vector2 mPivot = phomObjList[slotId + 7].GetComponent<RectTransform>().pivot;
            Vector3 endPos = phomObjList[slotId + 7].transform.position;
            GameObject img = Instantiate(phomObjList[18], phomObjList[18].transform.parent, false);
            RectTransform rtf = img.gameObject.GetComponent<RectTransform>();

            SoundManager.instance.PlayUISound(SoundFX.CARD_DROP);
            DOTween.To(() => rtf.pivot, x => rtf.pivot = x, mPivot, .25f);
            img.transform.DOMove(endPos, .25f).OnComplete(() => {
                Destroy(img);
            });

            /*
           Image img = Instantiate(phomImageList[14], phomImageList[14].transform.parent.parent, false);
           RectTransform rtf = img.gameObject.GetComponent<RectTransform>();
           rtf.anchoredPosition = new Vector2(853 + phomObjList[18].transform.localPosition.x - 40, 480 + phomObjList[18].transform.localPosition.y - 56);
           img.overrideSprite = cardFaces[52];
           img.gameObject.SetActive(true);
           Vector2 endPos = new Vector2(853 + phomObjList[getSlotIdBySvrId(targetSlotId) + 7].transform.localPosition.x, 480 + phomObjList[getSlotIdBySvrId(targetSlotId) + 7].transform.localPosition.y - 56);
           DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, endPos, .25f).OnComplete(() => {
               Destroy(img.gameObject);
           });*/
            return;
        }
        #endregion

        #region //ĂN [RE DONE]
        if (sourceSlotId != targetSlotId && sourceLineId == 3 && targetLineId == 1)
        {

            int slotId = getSlotIdBySvrId(targetSlotId);
            phomTextList[10 + slotId].font = phomFontList[1];
            phomTextList[10 + slotId].text = App.listKeyText["CARD_EAT"].ToUpper();//"ĂN";
            phomTextList[10 + slotId].gameObject.SetActive(true);
            phomTextList[10 + slotId].transform.DOScale(1.2f, 2f).SetEase(Ease.OutBounce).OnComplete(() =>
            {
                phomTextList[10 + slotId].gameObject.SetActive(false);
                phomTextList[10 + slotId].transform.localScale = Vector2.one;
            });

            Vector2 vec = new Vector2(65, 0);
            if (targetLineId == 1 && (slotId == 3 || slotId == 1))
                vec = new Vector2(0, 65);

            if (preCardRtf == null)
            {
                for (int i = slotId - 1; i > -1; i--)
                {
                    if (exitsSlotList[i] == true)
                    {
                        int count = phomImageList[13 + i].transform.parent.childCount;
                        if (i == 2 || i == 3)
                        {
                            preCardRtf = phomImageList[13 + i].transform.parent.GetChild(count - 1).GetComponent<RectTransform>();
                            //Destroy(phomImageList[13 + i].transform.parent.GetChild(count - 2).gameObject);
                        }
                        else
                        {
                            preCardRtf = phomImageList[13 + i].transform.parent.GetChild(0).GetComponent<RectTransform>();
                            //Destroy(phomImageList[13 + i].transform.parent.GetChild(0).gameObject);
                        }
                        App.trace("D M CÓ");
                        break;
                    }
                }
            }
            Vector2 mPos = (slotId > 1 ? 1 : -1) * vec * (phomImageList[slotId + 9].transform.parent.childCount - 1);
            preCardRtf.SetParent(phomImageList[slotId + 9].transform.parent);
            DOTween.To(() => preCardRtf.anchoredPosition, x => preCardRtf.anchoredPosition = x, mPos, .25f).OnComplete(() =>
            {
                if (vec.x > 0)
                {
                    if (slotId > 1)
                    {
                        preCardRtf.SetAsLastSibling();
                    }
                    else
                    {
                        preCardRtf.SetAsFirstSibling();
                    }
                }
                else
                {
                    if (slotId == 1)
                    {
                        preCardRtf.SetAsLastSibling();
                    }
                    if (slotId == 3)
                    {
                        preCardRtf.SetAsFirstSibling();
                    }
                }
            });


            return;
        }
        #endregion

        #region [TU HÚ]
        if (sourceSlotId != targetSlotId && sourceLineId == 0 && targetLineId == 1)
        {
            App.trace("============= Tu hú gửi đồ ================", Color.magenta.ToString());
            int startSlot = getSlotIdBySvrId(sourceSlotId);
            int slotId = getSlotIdBySvrId(targetSlotId);

            //Todo:
            // create send card
            // make animation

            for (int k = 0; k < ids.Count; k++)
            {

                Image img = Instantiate(phomImageList[5 + startSlot], phomImageList[5 + startSlot].transform.parent.parent, false);
                img.overrideSprite = cardFaces[ids[k]];
                RectTransform rtf = img.GetComponent<RectTransform>();

                firedCardList.Add(rtf.gameObject);

                img.gameObject.SetActive(true);

                var containerChilds = phomImageList[slotId + 9].transform.parent.childCount - 1;
                Vector2 vec = Vector2.zero;
                Vector2 mPos = Vector2.zero;
                if ((slotId == 3 || slotId == 1))
                {
                    vec = new Vector2(0, 65);
                    if (containerChilds % 2 == 0)
                    {
                        mPos = (vec * containerChilds) / 2;
                    }
                    else
                    {
                        mPos = (vec * (containerChilds / 2)) + vec;
                    }

                }
                else
                {
                    vec = new Vector2(65, 0);
                    mPos = vec * containerChilds;
                }

                rtf.SetParent(phomImageList[slotId + 9].transform.parent);

                DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, mPos, .25f).OnComplete(() =>
                {
                    if (vec.x > 0)
                    {
                        if (slotId > 1)
                        {
                            rtf.SetAsLastSibling();
                        }
                        else
                        {
                            rtf.SetAsFirstSibling();
                        }
                    }
                    else
                    {
                        if (slotId == 1 || slotId == 3)
                        {
                            rtf.SetAsFirstSibling();
                        }
                    }
                });

            }

            return;

        }




        #endregion

    }

    private string idsToPoint(List<int> ids)
    {
        int point = 0;
        for (int i = 0; i < ids.Count; i++)
        {
            point += ids[i] % 13 + 1;
        }
        return ids.Count == 9 ? "" : (point.ToString() + " điểm");
    }

    #region CHIA BÀI


    private List<PlayerCard> playerCardList = new List<PlayerCard>();
    private IEnumerator divideCards(List<int> ids, bool isReconnect = true)
    {

        phomObjList[17].SetActive(true);
        if (playerCardList.Count > 0)
        {
            for (int i = 0; i < playerCardList.Count; i++)
            {
                DestroyImmediate(playerCardList[i].Rtf.gameObject);
            }
        }
        myCardIdList.Clear();
        playerCardList.Clear();

        if (isReconnect)
        {
            for (int i = 0; i < ids.Count; i++)
            {
                Image img = Instantiate(phomImageList[5], phomImageList[5].transform.parent, false);
                img.overrideSprite = cardFaces[ids[i]];
                RectTransform rtf = img.GetComponent<RectTransform>();
                Vector2 vec = new Vector2(90, 0);
                Vector2 mPos = vec * (i);
                rtf.anchoredPosition = mPos;
                playerCardList.Add(new PlayerCard(rtf, img, img.transform.GetChild(0).gameObject));
                myCardIdList.Add(int.Parse(cardFaces[ids[i]].name));
                img.gameObject.SetActive(true);
            }
            yield return new WaitForSeconds(.5f);
            phomObjList[17].SetActive(false);
        }
        else
        {
            for (int i = 0; i < ids.Count; i++)
            {
                Image img = Instantiate(phomImageList[5], phomImageList[5].transform.parent.parent, false);
                img.overrideSprite = cardFaces[52];
                RectTransform rtf = img.GetComponent<RectTransform>();
                rtf.anchoredPosition = coordinatesList[15];
                img.gameObject.SetActive(true);
                Vector2 vec = new Vector2(90, 0);
                Vector2 mPos = vec * (i);
                int tmp = i;
                playerCardList.Add(new PlayerCard(rtf, img, img.transform.GetChild(0).gameObject));
                myCardIdList.Add(int.Parse(cardFaces[ids[tmp]].name));
                rtf.parent = phomImageList[5].transform.parent;
                rtf.SetAsLastSibling();
                DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, mPos, .5f + tmp * .05f).OnComplete(() => {

                    rtf.DORotate(new Vector3(0, 90, 0), .125f + tmp * .01f).OnComplete(() =>
                    {
                        try
                        {
                            img.overrideSprite = cardFaces[ids[tmp]];

                        }
                        catch
                        {
                            //App.trace("FACK DIU = " + ids[tmp]);
                        }
                        rtf.DORotate(new Vector3(0, 0, 0), .125f).OnComplete(() => {
                            if (tmp == ids.Count - 1)
                            {
                                phomObjList[17].SetActive(false);
                            }
                        });

                    });
                });
                yield return new WaitForSeconds(.125f);
            }
        }



    }
    private IEnumerator divideCards2(int slotId)
    {
        Vector2 mPivot = phomObjList[slotId + 7].GetComponent<RectTransform>().pivot;
        Vector3 endPos = phomObjList[slotId + 7].transform.position;
        App.trace(mPivot.x + "|" + mPivot.y);
        for (int i = 0; i < 9; i++)
        {
            /*
            Image img = Instantiate(phomImageList[14], phomImageList[14].transform.parent.parent, false);
            RectTransform rtf = img.gameObject.GetComponent<RectTransform>();
            Vector2 mPivot = phomObjList[slotId + 7].GetComponent<RectTransform>().pivot;
            rtf.anchoredPosition = new Vector2(853 + phomObjList[18].transform.localPosition.x - 40, 480 + phomObjList[18].transform.localPosition.y - 56);

            img.overrideSprite = cardFaces[52];
            img.gameObject.SetActive(true);

            Vector2 endPos = new Vector2(853 + phomObjList[slotId + 7].transform.localPosition.x - (mPivot.x ) * 80, 480 + phomObjList[7 + slotId].transform.localPosition.y - (mPivot.y * 112));

            DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, endPos, .5f).OnComplete(() => {
                Destroy(img.gameObject);
            });
            */
            GameObject img = Instantiate(phomObjList[18], phomObjList[18].transform.parent, false);
            RectTransform rtf = img.gameObject.GetComponent<RectTransform>();
            DOTween.To(() => rtf.pivot, x => rtf.pivot = x, mPivot, .25f);
            img.transform.DOMove(endPos, .25f, false).OnComplete(() => {
                Destroy(img);
            });
            yield return new WaitForSeconds(.125f);
        }
    }
    #endregion

    #region //HANDLER CONTROLLER BÊN CARD
    [HideInInspector]
    public bool isDragging = false;
    private List<CardPrepare> cardPrepareList = new List<CardPrepare>();
    private class CardPrepare
    {
        private int id, slib;
        private RectTransform rtf;
        private Transform parrentRtf;
        private Vector2 pos;
        private GameObject border;
        private string name;
        private bool isClicked;
        public CardPrepare(int id, RectTransform rtf, Transform parrentRtf, int slib, Vector2 pos, GameObject border, string name, bool isClicked)
        {
            this.Slib = slib;
            this.Id = id;
            this.Rtf = rtf;
            this.ParrentRtf = parrentRtf;
            this.Pos = pos;
            this.Border = border;
            this.Name = name;
            this.IsClicked = isClicked;
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
        public bool IsClicked
        {
            get
            {
                return isClicked;
            }

            set
            {
                isClicked = value;
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
    public void removeCardPrepare()
    {
        if (cardPrepareList.Count > 1)
        {
            cardPrepareList.RemoveAt(1);
        }
    }
    public void addCardPrepare(int id, RectTransform rtf, Transform parrentRtf, int slib, Vector2 pos, GameObject bor, string name, bool isClicked)
    {
        //phomObjList[19].SetActive(true);

        if (id == 0)
        {
            cardPrepareList.Clear();
        }
        CardPrepare cP = new CardPrepare(id, rtf, parrentRtf, slib, pos, bor, name, isClicked);
        cardPrepareList.Add(cP);
        //App.trace("CARD " + id + " ADDED WITH x = " + cP.Pos.x + "|y = " + cP.Pos.y + "|sibId = " + slib);
    }
    public void swapPrepareCard()
    {
        Debug.LogError("BBBBBBBBBBBBBBBBBB ========= >");
        _isAnimationFinish = false;
        if (cardPrepareList.Count < 2)
        {
            //App.trace("MUHA " + cardPrepareList[0].Pos.x + "|" + cardPrepareList[0].Pos.y);
            cardPrepareList[0].Rtf.parent = cardPrepareList[0].ParrentRtf;
            cardPrepareList[0].Rtf.anchoredPosition = cardPrepareList[0].Pos;
            cardPrepareList[0].Rtf.SetSiblingIndex(cardPrepareList[0].Slib);
            cardPrepareList.Clear();
            phomObjList[19].SetActive(false);

            _isAnimationFinish = true;
            if (discardPlayerActionCallback != null)
            {
                Debug.Log("CALL IN CALLBACK");
                discardPlayerActionCallback.Invoke();
            }

            return;
        }
        phomObjList[17].SetActive(true);
        int a = int.Parse(cardPrepareList[0].Name);
        int b = int.Parse(cardPrepareList[1].Name);
        int idTmpA = myCardIdList.IndexOf(a);
        int idTmpB = myCardIdList.IndexOf(b);
        myCardIdList[idTmpA] = b;
        myCardIdList[idTmpB] = a;

        PlayerCard mPlayerCard = playerCardList[idTmpA];
        playerCardList[idTmpA] = playerCardList[idTmpB];
        playerCardList[idTmpB] = mPlayerCard;
        cardPrepareList[0].Rtf.parent = cardPrepareList[1].ParrentRtf;
        cardPrepareList[0].Rtf.SetSiblingIndex(cardPrepareList[1].Slib);
        DOTween.To(() => cardPrepareList[0].Rtf.anchoredPosition, x => cardPrepareList[0].Rtf.anchoredPosition = x, cardPrepareList[1].Pos, .25f).OnComplete(() => {
            cardPrepareList[0].Border.SetActive(false);
            if (cardPrepareList[0].IsClicked)
                cardPrepareList[0].Rtf.anchoredPosition = new Vector2(cardPrepareList[0].Rtf.anchoredPosition.x, 65);
            else
            {
                cardPrepareList[0].Rtf.anchoredPosition = new Vector2(cardPrepareList[0].Rtf.anchoredPosition.x, 0);
            }
        });

        cardPrepareList[1].Rtf.SetAsLastSibling();
        DOTween.To(() => cardPrepareList[1].Rtf.anchoredPosition, x => cardPrepareList[1].Rtf.anchoredPosition = x, cardPrepareList[0].Pos, .25f).OnComplete(() => {
            cardPrepareList[1].Rtf.SetSiblingIndex(cardPrepareList[0].Slib);
            cardPrepareList[1].Border.SetActive(false);
            if (cardPrepareList[1].IsClicked)
                cardPrepareList[1].Rtf.anchoredPosition = new Vector2(cardPrepareList[1].Rtf.anchoredPosition.x, 65);
            else
            {
                cardPrepareList[1].Rtf.anchoredPosition = new Vector2(cardPrepareList[1].Rtf.anchoredPosition.x, 0);
            }
            cardPrepareList.Clear();
            phomObjList[17].SetActive(false);

            _isAnimationFinish = true;

            if (discardPlayerActionCallback != null)
            {
                Debug.Log("CALLBACK CALL 2");
                discardPlayerActionCallback.Invoke();
            }


            //phomObjList[19].SetActive(false);
        });

        //App.trace("ĐỔI " + cardPrepareList[0].Pos.x + "|" + cardPrepareList[0].Pos.y);
        //App.trace("VỚI " + cardPrepareList[1].Pos.x + "|" + cardPrepareList[1].Pos.y);


    }
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
    #endregion

    #region ĐI QUÂN BÀI CỦA MÌNH
    public void _myCard()   //This method is used in Unity Editor
    {
        myCard(new List<int>() { 1 }, "take");

    }
    public void myCard(List<int> ids, string action, int targetSlotId = -1, int targetIndex = -1)
    {
        #region//======MÌNH HẠ BÀI==============
        IsAutoDiscard = true;
        if (action == "final")
        {
            App.trace("============ MÌNH HẠ BÀI ============", Color.green.ToString());
            discardPlayerActionCallback.RemoveAllListeners();
            DOTween.Kill("itake");
            Transform tfm = phomImageList[9].transform.parent;
            int count = tfm.childCount;
            //App.trace("==================" + count);
            int childVisible = 0;
            for (int i = count - 1; i > -1; i--)
            {
                //tfm.GetChild(i).gameObject.SetActive(false);
                if (tfm.GetChild(i).gameObject.name.Contains("Clone"))
                {
                    tfm.GetChild(i).gameObject.SetActive(false);
                    //DestroyImmediate(tfm.GetChild(i).gameObject);
                    childVisible++;
                }

            }
            for (int k = 0; k < ids.Count; k++)
            {
                bool isEnd = false;
                bool needClone = true;
                int kTmp = k;
                for (int i = 0; i < playerCardList.Count; i++)
                {
                    int tmp = i;
                    if (kTmp > playerCardList.Count - 1 || kTmp == ids.Count - 1)
                        isEnd = true;
                    if (tmp < myCardIdList.Count)
                    {
                        if (myCardIdList[tmp] - 1 == ids[k])
                        {
                            needClone = false;
                            Vector2 vec = new Vector2(65, 0);
                            /*
                            Vector2 mPos = new Vector2(scX * coordinatesList[11].x, scY * coordinatesList[11].y);
                            mPos += vec * (phomImageList[9].transform.parent.childCount - 1);
                            playerCardList[tmp].Rtf.parent = playerCardList[tmp].Rtf.parent.parent;
                            */
                            Vector2 mPos = vec * (phomImageList[9].transform.parent.childCount - 1 - childVisible);
                            ; App.trace(mPos.x + "|pos = " + kTmp);
                            playerCardList[tmp].Rtf.parent = phomImageList[9].transform.parent;
                            playerCardList[tmp].Rtf.DOScale(.8348f, .25f).SetEase(Ease.InBack);
                            phomObjList[17].SetActive(true);
                            playerCardList[tmp].Trans.SetActive(false);
                            RectTransform rtf = playerCardList[tmp].Rtf;
                            playerCardList.RemoveAt(tmp);
                            myCardIdList.RemoveAt(tmp);
                            try
                            {
                                DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, mPos, .25f).OnComplete(() =>
                                {
                                    //playerCardList[tmp].Rtf.SetParent(phomImageList[9].transform.parent);
                                    rtf.SetAsLastSibling();
                                    phomObjList[17].SetActive(false);
                                    //App.trace("OUT = " + tmp);

                                    Destroy(rtf.gameObject.GetComponent<PhomCardCtrl>());
                                    firedCardList.Add(rtf.gameObject);
                                    if (isEnd == true)
                                    {
                                        for (int j = 0; j < playerCardList.Count; j++)
                                        {
                                            int temp = j;
                                            DOTween.To(() => playerCardList[temp].Rtf.anchoredPosition, x => playerCardList[temp].Rtf.anchoredPosition = x, new Vector2(90 * temp, 0), .25f);
                                            discardPlayerActionCallback.RemoveAllListeners();
                                        }
                                    }
                                });
                            }
                            catch
                            {
                                App.trace("tmp = " + tmp + "|playerCardList = " + playerCardList.Count);
                            }
                            break;
                        }
                    }
                    else
                    {
                        for (int j = 0; j < playerCardList.Count; j++)
                        {
                            int temp = j;
                            DOTween.To(() => playerCardList[temp].Rtf.anchoredPosition, x => playerCardList[temp].Rtf.anchoredPosition = x, new Vector2(90 * temp, 0), .25f);
                        }
                        break;
                    }
                }
                if (needClone == true)   //CLONE 1 lá bài đã ăn
                {
                    Image img = Instantiate(phomImageList[5], phomImageList[5].transform.parent.parent, false);
                    img.overrideSprite = cardFaces[ids[k]];
                    RectTransform rtf = img.GetComponent<RectTransform>();
                    rtf.anchoredPosition = coordinatesList[16];
                    img.gameObject.SetActive(true);
                    Vector2 vec = new Vector2(65, 0);
                    Vector2 mPos = vec * (phomImageList[9].transform.parent.childCount - 1 - childVisible);
                    rtf.parent = phomImageList[9].transform.parent;
                    rtf.DOScale(.8348f, .25f);
                    DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, mPos, .25f).OnComplete(() =>
                    {
                        //playerCardList[tmp].Rtf.SetParent(phomImageList[9].transform.parent);
                        rtf.SetAsLastSibling();
                        phomObjList[17].SetActive(false);

                        Destroy(rtf.gameObject.GetComponent<PhomCardCtrl>());
                        firedCardList.Add(rtf.gameObject);
                    });
                }
            }
            return;
        }
        #endregion

        #region //=========MÌNH ĂN BÀI==============
        if (action == "eat")
        {
            App.trace("============ MÌNH ĂN BÀI ============", Color.green.ToString());
            phomTextList[10].font = phomFontList[1];
            phomTextList[10].text = App.listKeyText["CARD_EAT"];//"ĂN";
            phomTextList[10].gameObject.SetActive(true);
            phomTextList[10].transform.DOScale(1.2f, 2f).SetEase(Ease.OutBounce).OnComplete(() =>
            {
                phomTextList[10].gameObject.SetActive(false);
                phomTextList[10].transform.localScale = Vector2.one;
            });

            Image img = Instantiate(phomImageList[9], phomImageList[9].transform.parent.parent, false);
            img.overrideSprite = cardFaces[ids[0]];
            RectTransform rtf = img.GetComponent<RectTransform>();
            if (preCardRtf == null)
            {
                for (int i = 3; i > 0; i--)
                {
                    if (exitsSlotList[i] == true)
                    {
                        int count = phomImageList[13 + i].transform.parent.childCount;
                        if (i == 2 || i == 3)
                        {
                            preCardRtf = phomImageList[13 + i].transform.parent.GetChild(count - 1).GetComponent<RectTransform>();
                        }
                        else
                        {
                            preCardRtf = phomImageList[13 + i].transform.parent.GetChild(0).GetComponent<RectTransform>();
                        }
                        break;
                    }
                }
            }
            preCardRtf.parent = phomImageList[9].transform.parent.parent;
            rtf.anchoredPosition = preCardRtf.anchoredPosition;
            //rtf.localScale = Vector2.one * .7f;

            img.gameObject.SetActive(true);

            Vector2 vec = new Vector2(90, 0);
            Vector2 mPos = vec * (phomImageList[9].transform.parent.childCount - 1);
            Destroy(preCardRtf.gameObject);

            rtf.parent = phomImageList[9].transform.parent;
            if (rtf != null)
            {
                DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, mPos, .25f).OnComplete(() => {
                    rtf.parent = phomImageList[9].transform.parent;
                    rtf.SetAsLastSibling();
                    firedCardList.Add(img.gameObject);
                });
            }
            return;
        }
        #endregion

        #region//=====MÌNH BỐC BÀI=============
        if (action == "take")
        {
            App.trace("============ MÌNH BỐC BÀI ============", Color.green.ToString());
            phomObjList[17].SetActive(true);
            Image img = Instantiate(phomImageList[5], phomImageList[5].transform.parent.parent, false);
            img.overrideSprite = cardFaces[52];
            RectTransform rtf = img.GetComponent<RectTransform>();
            rtf.localScale = Vector2.one * .7043f;
            rtf.anchoredPosition = coordinatesList[15];
            img.gameObject.SetActive(true);
            Vector2 vec = new Vector2(90, 0);
            //Vector2 mPos = vec * (phomImageList[5].transform.parent.childCount - 1);
            //playerCardList.Add(new PlayerCard(rtf, img, img.transform.GetChild(0).gameObject));
            //myCardIdList.Add(int.Parse(cardFaces[ids[0]].name));

            Vector2 mPos = Vector2.zero;
            if (targetIndex < playerCardList.Count)
            {
                playerCardList.Insert(targetIndex, new PlayerCard(rtf, img, img.transform.GetChild(0).gameObject));
                myCardIdList.Insert(targetIndex, int.Parse(cardFaces[ids[0]].name));

                mPos = vec * targetIndex;
                for (int i = targetIndex; i < playerCardList.Count; i++)
                {
                    //DOTween.To(() => playerCardList[i].Rtf.anchoredPosition, x => playerCardList[i].Rtf.anchoredPosition = x, new Vector2(90 * targetIndex + 180, 0), .25f);
                    int temp = i;
                    App.trace("ĐÃ DONE " + temp + "playerCardListCount = " + playerCardList.Count + "|myIdCardListCount = " + myCardIdList.Count);
                    //CÓ VẤN ĐỀ VỀ CHỈ SỐ
                    RectTransform mRtf = playerCardList[temp].Rtf;
                    if (mRtf == null)
                        continue;
                    DOTween.To(() => mRtf.anchoredPosition, x => mRtf.anchoredPosition = x, new Vector2(90 * (mRtf.GetSiblingIndex()), 0), .25f).OnComplete(() => {
                        if (temp == playerCardList.Count - 1)
                        {
                            /*
                            playerCardList.Insert(targetIndex, new PlayerCard(rtf, img, img.transform.GetChild(0).gameObject));
                            myCardIdList.Insert(targetIndex, int.Parse(cardFaces[ids[0]].name));
                            */
                        }
                    });
                }


            }
            else
            {
                mPos = vec * (phomImageList[5].transform.parent.childCount - 1);
                playerCardList.Add(new PlayerCard(rtf, img, img.transform.GetChild(0).gameObject));
                myCardIdList.Add(int.Parse(cardFaces[ids[0]].name));
            }


            rtf.parent = phomImageList[5].transform.parent;
            rtf.DOScale(1, .25f);
            SoundManager.instance.PlayUISound(SoundFX.CARD_DROP);
            firedCardList.Add(rtf.gameObject);
            rtf.SetSiblingIndex(targetIndex + 1);
            DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, mPos, .5f).SetId("itake").OnComplete(() => {


            });
            rtf.DORotate(new Vector3(0, 90, 0), .25f).OnComplete(() =>
            {
                try
                {
                    img.overrideSprite = cardFaces[ids[0]];

                }
                catch
                {
                    //App.trace("FACK DIU = " + ids[tmp]);
                }

                rtf.DORotate(new Vector3(0, 0, 0), .25f);
                //rtf.SetSiblingIndex(targetIndex + 1);
                phomObjList[17].SetActive(false);
                isDragging = false;
                _hadDrawcard = true;
                App.trace("BỐC OK " + mPos.x);
            });
            return;
        }
        #endregion

        #region //============MÌNH NHẢ BÀI==============
        if (action == "remove")
        {
            App.trace("============ MÌNH NHẢ BÀI ============", Color.green.ToString());
            App.trace("NHẢ BÀI");
            for (int i = 0; i < myCardIdList.Count; i++)
            {
                int tmp = i;
                if (myCardIdList[i] - 1 == ids[0])
                {

                    Vector2 vec = new Vector2(-65, 0);
                    Vector2 mPos = vec * (phomImageList[13].transform.parent.childCount - 1);
                    mPos += new Vector2(260, 0);
                    playerCardList[i].Rtf.parent = playerCardList[i].Rtf.parent.parent;
                    playerCardList[i].Rtf.DOScale(.7043f, .25f).SetEase(Ease.InBack);
                    phomObjList[17].SetActive(true);
                    playerCardList[i].Trans.SetActive(false);
                    Destroy(playerCardList[i].Rtf.gameObject.GetComponent<PhomCardCtrl>());
                    firedCardList.Add(playerCardList[i].Rtf.gameObject);
                    playerCardList[i].Rtf.SetParent(phomImageList[13].transform.parent);
                    DOTween.To(() => playerCardList[i].Rtf.anchoredPosition, x => playerCardList[i].Rtf.anchoredPosition = x, mPos, .25f).OnComplete(() =>
                    {
                        SoundManager.instance.PlayUISound(SoundFX.CARD_DROP);
                        playerCardList[i].Rtf.SetAsFirstSibling();
                        phomObjList[17].SetActive(false);
                        preCardRtf = playerCardList[i].Rtf;
                        playerCardList.RemoveAt(tmp);
                        myCardIdList.RemoveAt(tmp);

                        for (int j = tmp; j < playerCardList.Count; j++)
                        {
                            int temp = j;
                            App.trace("playerCArdList count = " + playerCardList.Count + "|temp = " + temp);
                            //CÓ VẤN ĐỀ VỀ CHỈ SỐ
                            RectTransform mRtf = playerCardList[temp].Rtf;
                            if (mRtf == null)
                                continue;
                            try
                            {
                                DOTween.To(() => mRtf.anchoredPosition, x => mRtf.anchoredPosition = x, new Vector2(90 * (mRtf.GetSiblingIndex() - 1), 0), .25f);
                            }
                            catch
                            {

                            }
                        }
                    });

                    break;
                }
            }
            return;
        }
        #endregion

        #region //=========MÌNH GỬI BÀI==============
        if (action == "send" && targetSlotId > -1)
        {
            App.trace("============ MÌNH GỬI BÀI ============", Color.green.ToString());
            for (int i = 0; i < myCardIdList.Count; i++)
            {
                int tmp = i;
                if (myCardIdList[i] - 1 == ids[0])
                {

                    Vector2 vec = new Vector2(65, 0);
                    if (targetSlotId == 1 || targetSlotId == 3)
                    {
                        vec = new Vector2(0, 65);
                    }
                    Vector2 mPos = Vector2.zero;
                    int mChildCount = phomImageList[9 + targetSlotId].transform.parent.childCount;
                    mPos = vec * (mChildCount - 1);


                    playerCardList[i].Rtf.parent = phomImageList[9 + targetSlotId].transform.parent;

                    if (targetSlotId == 1 || targetSlotId == 3)  //Theo hàng dọc
                    {
                        mPos -= new Vector2(0, 32.5f * (mChildCount - 1) / 2 + 17.5f);
                        mPos = phomImageList[9 + targetSlotId].transform.parent.GetChild(0).GetComponent<RectTransform>().anchoredPosition + vec;
                        playerCardList[i].Rtf.SetAsFirstSibling();
                    }
                    else
                    {
                        mPos = phomImageList[9 + targetSlotId].transform.parent.GetChild(mChildCount - 1).GetComponent<RectTransform>().anchoredPosition + vec;
                        playerCardList[i].Rtf.SetAsLastSibling();
                    }

                    playerCardList[i].Rtf.DOScale(.7043f, .25f).SetEase(Ease.InBack);

                    phomObjList[17].SetActive(true);
                    playerCardList[i].Trans.SetActive(false);
                    Destroy(playerCardList[i].Rtf.gameObject.GetComponent<PhomCardCtrl>());
                    firedCardList.Add(playerCardList[i].Rtf.gameObject);
                    DOTween.To(() => playerCardList[i].Rtf.anchoredPosition, x => playerCardList[i].Rtf.anchoredPosition = x, mPos, .25f).OnComplete(() =>
                    {

                        phomObjList[17].SetActive(false);
                        preCardRtf = playerCardList[i].Rtf;
                        playerCardList.RemoveAt(tmp);
                        myCardIdList.RemoveAt(tmp);

                        for (int j = 0; j < playerCardList.Count; j++)
                        {
                            int temp = j;
                            DOTween.To(() => playerCardList[temp].Rtf.anchoredPosition, x => playerCardList[temp].Rtf.anchoredPosition = x, new Vector2(90 * (playerCardList[temp].Rtf.GetSiblingIndex() - 1), 0), .25f);
                        }
                    });

                    break;
                }
            }
            return;
        }
        #endregion

    }
    #endregion

    public void stateBtnActions(string act)
    {
        OutBounMessage req = null;
        //Debug.Log("Click ");
        switch (act)
        {
            case "take":
                req = new OutBounMessage("TAKE");
                break;
            case "eat":
                req = new OutBounMessage("EAT");
                break;
            case "remove":
                req = new OutBounMessage("REMOVE");
                break;
            case "drop":
                req = new OutBounMessage("DROP_AVAILABLE_BAND");
                break;
            case "drop2":
                req = new OutBounMessage("DROP_BAND");
                break;
            case "send":
                //Debug.Log("Click => Send");
                req = new OutBounMessage("SEND_CARD");
                break;
        }

        if (req != null)
        {
            req.addHead();
            if (act == "remove" || act == "drop2" || act == "send")
            {
                List<int> ids = new List<int>();
                for (int i = 0; i < myCardIdList.Count; i++)
                {
                    if (playerCardList[i].Rtf.anchoredPosition.y < 80 && 50 < playerCardList[i].Rtf.anchoredPosition.y)
                    {
                        ids.Add(myCardIdList[i] - 1);
                        //Debug.Log(i + "|card = " + myCardIdList[i]);
                    }

                }
                if (ids.Count == 0)
                {
                    //App.showErr("Bạn chưa chọn bài");
                    App.showErr(App.listKeyText["CARD_NOT_SELECTED"]);
                    return;
                }
                if (ids.Count > 1 && act == "remove")
                {
                    //App.showErr("Bạn không được chọn quá 2 quân bài");
                    string tempString = App.listKeyText["CARD_SELECTED_TOO_MANY"];
                    string new1 = tempString.Replace("#1","2");
                    App.showErr(new1);
                    return;
                }
                if (state.Mode == 1)
                {
                    req.writeByte(ids[0]);
                }
                else
                {
                    req.writeBytes(ids);
                }
            }
            //App.ws.send(req.getReq(), null, true, 0);
            App.ws.send(req.getReq(), (res) => {
                if (act == "remove" || act == "drop")
                {
                    IsAutoDiscard = false;
                }
            }, true, 0);
        }
    }

    public void balanceChanged(int slotId, long chipBalance, long starBalance)
    {
        int mSl = getSlotIdBySvrId(slotId);
        if (mSl > -1)
        {
            //StartCoroutine(_balanceChanged(chipList[mSl], chipBalance, mbTextList[13 + mSl]));
            chipList[mSl] = chipBalance;
            playerList[mSl].ChipBalance = chipBalance;
            playerList[mSl].StarBalance = starBalance;
            phomTextList[4 + mSl].text = mSl == 0 ? App.formatMoney(chipBalance.ToString()) : App.formatMoneyAuto(chipBalance);
        }

    }

    private IEnumerator _quit(List<int> winLs, List<int> loseLs)
    {

        if (regQuit)
        {
            yield return new WaitForSeconds(1f);
            phomTextList[18].gameObject.SetActive(false);
            for (int i = 0; i < 4; i++)
            {
                if (rsList[i] != "")
                {
                    phomTextList[10 + i].gameObject.SetActive(true);
                    if (rsList[i].Contains('-'))
                    {
                        phomTextList[14 + i].font = phomFontList[0];
                    }

                    else
                        phomTextList[14 + i].font = phomFontList[1];

                    phomTextList[14 + i].text = rsList[i];
                    phomTextList[14 + i].gameObject.SetActive(true);
                    if ((isFullBand == true && rsList[i].Contains('-')))
                    {
                        Vector3 mVec = phomTextList[14 + i].transform.localPosition;
                        Transform mRrtf = phomTextList[14 + i].transform;
                        mRrtf.DOLocalMoveY(mVec.y + 100, 1f);
                    }
                }
            }
            yield return new WaitForSeconds(3f);
            isPlaying = false;
            backToTableList();
        }
        else
        {
            yield return new WaitForSeconds(2.5f);
            #region //BAY TIỀN
            if (regQuit == false && exited == false && CPlayer.preScene.Contains("TableList"))
            {
                SoundManager.instance.PlayUISound(SoundFX.CARD_TAI_XIU_BET);
                for (int i = 0; i < winLs.Count; i++)
                {
                    Vector2 end = coordinatesList[winLs[i]];
                    for (int j = 0; j < loseLs.Count; j++)
                    {
                        Vector2 start = coordinatesList[loseLs[j]];

                        LoadingControl.instance.flyCoins("line", 10, start, end);
                    }
                }
            }
            #endregion
            phomTextList[18].gameObject.SetActive(false);
            for (int i = 0; i < 4; i++)
            {
                if (rsList[i] != "")
                {
                    phomTextList[10 + i].gameObject.SetActive(true);
                    if (rsList[i].Contains('-'))
                        phomTextList[14 + i].font = phomFontList[0];
                    else
                        phomTextList[14 + i].font = phomFontList[1];

                    phomTextList[14 + i].text = rsList[i];
                    //phomTextList[14 + i].transform.DOScale(1.5f, 2f);
                    phomTextList[14 + i].gameObject.SetActive(true);
                    if ((isFullBand == true && rsList[i].Contains('-')))
                    {
                        Vector3 mVec = phomTextList[14 + i].transform.localPosition;
                        Transform mRrtf = phomTextList[14 + i].transform;
                        mRrtf.DOLocalMoveY(mVec.y + 100, 1f);
                    }
                }
            }
            yield return new WaitForSeconds(1f);
            coinFlyed = true;
            isPlaying = false;
            if (regQuit == true)
            {
                backToTableList();
            }
        }

    }

    public void test(int id)
    {
        //myCard(new List<int>() { id }, "send", 2);    GỬI BÀI

        //int slot = 2;
        //slot = detecSvrBySlotId(slot);
        //moveCard(new List<int>() { id }, -1, slot, 0, 0);    //Nó bốc bài

        //moveCard(new List<int>() { id }, 5, slot, 3, 1);    //Nó ăn bài

        //moveCard(new List<int>() { id }, slot, slot, 0, 3);    //Nó nhả

        //moveCard(new List<int>() { 1, 2, 3}, slot, slot, 0, 1);   //Nó hạ

        //moveCard(new List<int>() { 1, 2, 3,23,35,34,10,21,19 }, slot, slot, 0, -9);   //Nó show bài

        //myCard(new List<int>() { id }, "take"); //Mình bốc bài
        //myCard(new List<int>() { id }, "eat");    //Mình ăn bài

        //LoadingControl.instance.flyCoins("line", 10, coordinatesList[0], coordinatesList[1]);   //Bay xu


        /*
        RectTransform rtf = phomTextList[18].gameObject.GetComponent<RectTransform>();
        rtf.DOMove(phomObjList[18].transform.position, .01f);
        rtf.localScale = Vector2.one * 10f;
        phomTextList[18].gameObject.SetActive(true);

        Vector2 mPos = phomTextList[10 + id].gameObject.transform.position;

        rtf.DOScale(2f, 1f).SetEase(Ease.OutBounce).OnComplete(() => {
            rtf.DOScale(.5f, 2f).OnComplete(() => {
                phomTextList[18].gameObject.SetActive(false);
            });
            //DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, coordinatesList[2], 1f);
            rtf.DOMove(mPos, 1f);
        });
        */
        if (id != 0)
        {
            myCard(new List<int>() { id }, "remove", 0, 2);
            return;
        }
        //myCard(new List<int>() { 8, 21, 35, 10, 24, 37, 4, 5, 6 }, "final");

        int slotId = 3;
        int mCount = phomImageList[slotId + 13].transform.parent.childCount - 1;
        if (mCount < 1)
        {
            return;
        }
        //RECV [MOVE] IdsCount = 1 | sourceSlotId = 1 | sourceLineId = 3| targetSlotId = 0| targetLineId = 3 | targetIndex = -1| CON DATA =
        RectTransform rtf = null;
        if (slotId < 2)
        {
            rtf = phomImageList[slotId + 13].transform.parent.GetChild(0).gameObject.GetComponent<RectTransform>();
        }
        else
        {
            rtf = phomImageList[slotId + 13].transform.parent.GetChild(mCount).gameObject.GetComponent<RectTransform>();
        }
        slotId = 0;
        Vector2 vec = new Vector2(65, 0);
        Vector2 mPos = Vector2.zero;

        mPos = vec * (phomImageList[13 + slotId].transform.parent.childCount - 1);
        if (slotId == 0 || slotId == 1)
        {
            mPos = new Vector2(260, 0) - mPos;
        }
        rtf.SetParent(phomImageList[slotId + 13].transform.parent);
        DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, mPos, .25f).OnComplete(() => {
            if (slotId > 1)
            {
                rtf.SetAsLastSibling();
            }
            else
            {
                rtf.SetAsFirstSibling();
            }
        });
    }

    public void openHelp(bool isShow)
    {
        if (isShow)
        {
            phomObjList[20].SetActive(true);
            return;
        }
        phomObjList[20].SetActive(false);
    }
}
